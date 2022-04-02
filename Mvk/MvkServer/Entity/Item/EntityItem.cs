using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

namespace MvkServer.Entity.Item
{
    /// <summary>
    /// Сущность предмета
    /// </summary>
    public class EntityItem : EntityBase
    {
        /// <summary>
        /// Объект стака предметов
        /// </summary>
        public ItemStack Stack { get; private set; }
        /// <summary>
        /// Возраст (используется для его анимации вверх и вниз, а также для истечения срока его действия) 
        /// </summary>
        public int Age { get; private set; }
        /// <summary>
        /// Состояние (Например, урон для инструментов) 
        /// </summary>
        public int Health { get; protected set; } = 5;
        /// <summary>
        /// Случайная начальная высота с плавающей запятой 
        /// </summary>
        public float HoverStart { get; private set; }
        /// <summary>
        /// Было ли перемещение вещи
        /// </summary>
        public bool IsMoving { get; private set; } = false;
        /// <summary>
        /// Задержка перед получением 
        /// </summary>
        private int delayBeforeCanPickup = 0;

        /// <summary>
        /// Владелец, кто выкинул
        /// </summary>
        //private string owner;

        /// <summary>
        /// Вращение
        /// </summary>
        //private float rotationYaw;

        

        public EntityItem(WorldBase world) : base(world)
        {
            SetSize(.25f, .5f);
            Type = EnumEntities.Item;
            Stack = new ItemStack(Blocks.GetBlock(EnumBlock.Dirt));
        }

        public EntityItem(WorldBase world, vec3 pos) : this(world)
        {
            SetPosition(pos);
            HoverStart = (float)World.Rand.NextDouble() * glm.pi * 2f;
            //rotationYaw = (float)World.Rand.NextDouble() * glm.pi360;
            Motion = new vec3((float)World.Rand.NextDouble() * .2f - .1f, .2f, (float)World.Rand.NextDouble() * .2f - .1f);
        }

        public EntityItem(WorldBase world, vec3 pos, ItemStack stack) : this(world, pos) => Stack = stack;

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public override string GetName() => "item." + Stack.ToString();

        public void SetPosSpawn(vec3 pos)
        {
            SetPosition(pos);
            PositionServer = PositionPrev = LastTickPos = Position;
        }

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        //public override bool HasCustomName() => true;

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        //public override bool CanBeCollidedWith() => !IsDead;

        public override void Update()
        {
            if (Stack == null || Position.y < -16)
            {
                SetDead();
            }
            else
            {
                base.Update();
                if (delayBeforeCanPickup > 0 && delayBeforeCanPickup != 32767) delayBeforeCanPickup --;

                PositionPrev = Position;
                Motion += new vec3(0, -.04f, 0);
                NoClip = PushOutOfBlocks(new vec3(Position.x, (BoundingBox.Min.y + BoundingBox.Max.y) / 2f, Position.z));
                MoveEntity(Motion);

                // Если мелочь убираем
                Motion = new vec3(
                    Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x,
                    Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y,
                    Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
                );
                // Фиксируем было ли перемещение
                IsMoving = Motion.x != 0 || Motion.y != 0 || Motion.z != 0;

                if (IsMoving)
                {
                    // Если лава подпрыгивает , шипит...
                    //if (this.worldObj.getBlockState(new BlockPos(this)).getBlock().getMaterial() == Material.lava)
                    //{
                    //    this.motionY = 0.20000000298023224D;
                    //    this.motionX = (double)((this.rand.nextFloat() - this.rand.nextFloat()) * 0.2F);
                    //    this.motionZ = (double)((this.rand.nextFloat() - this.rand.nextFloat()) * 0.2F);
                    //    this.playSound("random.fizz", 0.4F, 2.0F + this.rand.nextFloat() * 0.4F);
                    //}

                    if (!World.IsRemote)
                    {
                        // Ищет другие стопки предметов поблизости и пытается сложить их вместе
                        SearchForOtherItemsNearby();
                    }
                }

                float move = .98f;

                if (OnGround)
                {
                    // скользкий блок
                    move = .6f * .98f;
                    //move = World.GetBlockState(new BlockPos(MathHelper.floor_double(this.posX), MathHelper.floor_double(this.getEntityBoundingBox().minY) - 1, MathHelper.floor_double(this.posZ))).getBlock().slipperiness * 0.98F;
                }

                vec3 motion = Motion * new vec3(move, .98f, move);

                if (OnGround) motion.y *= -.5f;

                Motion = motion;
                // ------------------

                if (Age != -32768) Age++;

                // Если сущность живёт 5 мин или больше она умирает
                if (!World.IsRemote && Age >= 6000) SetDead();
            }
        }

        /// <summary>
        /// Ищет другие стопки предметов поблизости и пытается сложить их вместе.
        /// </summary>
        private void SearchForOtherItemsNearby()
        {
            MapListEntity map = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityItem, BoundingBox.Expand(new vec3(.5f, 0, .5f)), -1);

            while (map.Count > 0)
            {
                EntityItem entityItem = map.FirstRemove() as EntityItem;
                CombineItems(entityItem);
            }
        }

        /// <summary>
        /// Пытается объединить этот элемент с элементом, переданным в качестве параметра.
        /// Возвращает true в случае успеха. Либо этот предмет, либо другой предмет будет удален из мира.
        /// </summary>
        private bool CombineItems(EntityItem other)
        {
            if (other == this) return false;
            else if (!other.IsDead && !IsDead)
            {
                ItemStack stack = Stack;
                ItemStack stackOther = other.Stack;

                if (delayBeforeCanPickup != 32767 && other.delayBeforeCanPickup != 32767
                    && Age != -32768 && other.Age != -32768)
                {
                    if (stackOther.Item != null 
                        && stackOther.Item.Id == stack.Item.Id
                        && stackOther.ItemDamage == stack.ItemDamage)
                    {
                        if (stackOther.Amount < stack.Amount) return other.CombineItems(this);
                        if (stackOther.Amount + stack.Amount <= stackOther.GetMaxStackSize())
                        {
                            stackOther.AddAmount(stack.Amount);
                            other.delayBeforeCanPickup = Mth.Max(other.delayBeforeCanPickup, delayBeforeCanPickup);
                            other.Age = Mth.Min(other.Age, Age);
                            //other.SetEntityItemStack(stackOther);
                            SetDead();
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Вызывается сущностью игрока при столкновении с сущностью 
        /// </summary>
        public void OnCollideWithPlayer(EntityPlayer entityIn)
        {
            ItemStack var2 = Stack;
            int var3 = var2.Amount;

            // добавляем стак в инвентарь, и отминусовываем с валяющего
            //entityIn.inventory.addItemStackToInventory(var2))
            if (delayBeforeCanPickup == 0// && (owner == null || 6000 - Age <= 200 || owner.equals(entityIn.getName())) 
                && entityIn.Inventory.AddItemStackToInventory(var2))
            {
                // Достижения!!!
                //if (var2.Item == Item.getItemFromBlock(Blocks.log))
                //{
                //    entityIn.triggerAchievement(AchievementList.mineWood);
                //}

                //if (var2.getItem() == Item.getItemFromBlock(Blocks.log2))
                //{
                //    entityIn.triggerAchievement(AchievementList.mineWood);
                //}

                //if (var2.getItem() == Items.leather)
                //{
                //    entityIn.triggerAchievement(AchievementList.killCow);
                //}

                //if (var2.getItem() == Items.diamond)
                //{
                //    entityIn.triggerAchievement(AchievementList.diamonds);
                //}

                //if (var2.getItem() == Items.blaze_rod)
                //{
                //    entityIn.triggerAchievement(AchievementList.blazeRod);
                //}

                //if (var2.getItem() == Items.diamond && this.getThrower() != null)
                //{
                //    EntityPlayer var4 = this.worldObj.getPlayerEntityByName(this.getThrower());

                //    if (var4 != null && var4 != entityIn)
                //    {
                //        var4.triggerAchievement(AchievementList.diamondsToYou);
                //    }
                //}

                //if (!this.isSlient())
                {
                    // Звук
                    //this.worldObj.playSoundAtEntity(entityIn, "random.pop", 0.2F, ((this.rand.nextFloat() - this.rand.nextFloat()) * 0.7F + 1.0F) * 2.0F);
                }

                // Вызывается всякий раз, когда предмет поднимается при наступлении на него. 
                entityIn.ItemPickup(this, var3);

                // Если стак пустой, объект помечаем на удаление
                if (var2.Amount <= 0) SetDead();
            }
        }

        #region delayBeforeCanPickup & Age

        public void SetDefaultPickupDelay() => delayBeforeCanPickup = 10;

        public void SetNoPickupDelay() => delayBeforeCanPickup = 0;

        public void SetInfinitePickupDelay() => delayBeforeCanPickup = 32767;

        public void SetPickupDelay(int ticks) => delayBeforeCanPickup = ticks;

        public bool IsPickupDelay() => delayBeforeCanPickup > 0;

        public void Age10min() => Age = -6000;

        public void AgeEnd()
        {
            SetInfinitePickupDelay();
            Age = 5999;
        }

        #endregion

        public override string ToString()
        {
            return string.Format("{0}-{1} XYZ {2}", Id, Type, Position);
        }
    }
}
