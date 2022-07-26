using MvkServer.Entity.Item;
using MvkServer.Entity.Mob;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using System.Collections.Generic;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности, отвечает за движение вращение и прочее
    /// </summary>
    public abstract class EntityLiving : EntityLook
    {
        /// <summary>
        /// Объект ввода кликов клавиатуры
        /// </summary>
        public EnumInput Input { get; protected set; } = EnumInput.None;
        /// <summary>
        /// Счётчик движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwing { get; protected set; } = 0;
        /// <summary>
        /// Скорость движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwingAmount { get; protected set; } = 0;
        /// <summary>
        /// Движение из-за смещения
        /// </summary>
        public vec3 MotionPush { get; set; } = new vec3(0);
        /// <summary>
        /// Прыгаем
        /// </summary>
        public bool IsJumping { get; protected set; } = false;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как «мертвая», то есть иметь в мире труп.
        /// </summary>
        public int DeathTime { get; protected set; } = 0;
        /// <summary>
        /// Уровень здоровья
        /// </summary>
        public float Health { get; protected set; }
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как травмированная, то есть маргает красным
        /// </summary>
        public int DamageTime { get; protected set; } = 0;
        /// <summary>
        /// Значение поворота вокруг своей оси с сервера
        /// </summary>
        public float RotationYawServer { get; protected set; }
        /// <summary>
        /// Значение поворота вверх вниз с сервера
        /// </summary>
        public float RotationPitchServer { get; protected set; }
        /// <summary>
        /// Сколько тиков эта сущность пробежала с тех пор, как была жива 
        /// </summary>
        public int TicksExisted { get; private set; } = 0;
        /// <summary>
        /// Флаг спавна, пометка начального спавна игрока 
        /// ПОКА НЕ ЗНАЮ ЗАЧЕМ я это сделал!!!
        /// </summary>
        public bool FlagSpawn { get; set; } = false;
        /// <summary>
        /// Пометка что было движение и подобное для сервера, чтоб отправлять пакеты
        /// </summary>
        public EnumActionChanged ActionChanged { get; set; } = EnumActionChanged.None;
        /// <summary>
        /// Объект скорости сущности
        /// </summary>
        public EntitySpeed Speed { get; protected set; }

        /// <summary>
        /// Имя
        /// </summary>
        protected string name = "";
        /// <summary>
        /// Скорость движения на предыдущем тике. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        protected float limbSwingAmountPrev = 0;
        /// <summary>
        /// Анимация движения руки 0..1
        /// </summary>
        public float swingProgress = 0;
        /// <summary>
        /// Анимация движения руки предыдущего такта
        /// </summary>
        private float swingProgressPrev = 0;
        /// <summary>
        /// Счётчик тактов для анимации руки 
        /// </summary>
        private int swingProgressInt = 0;
        /// <summary>
        /// Запущен ли счётчик анимации руки
        /// </summary>
        private bool isSwingInProgress = false;
        
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int jumpTicks = 0;
        /// <summary>
        /// Дистанция падения
        /// </summary>
        private float fallDistance = 0;
        /// <summary>
        /// Результат падения, отладка!!!
        /// </summary>
        protected float fallDistanceResult = 0;
        /// <summary>
        /// Оборудование, которое этот моб ранее носил, использовалось для синхронизации
        /// </summary>
        protected ItemStack[] previousEquipment = new ItemStack[0];
        /// <summary>
        /// Возраст этой сущности (используется для определения, когда она умирает)
        /// </summary>
        protected int entityAge;

        /// <summary>
        /// Расстояние, которое необходимо преодолеть, чтобы вызвать новый звук шага
        /// </summary>
        private int nextStepDistance = 1;
        /// <summary>
        /// Пройденное расстояние умножается на 0,3, для звукового эффекта
        /// </summary>
        private float distanceWalkedOnStepModified = 0;
        /// <summary>
        /// Семплы хотьбы
        /// </summary>
        protected AssetsSample[] samplesStep = new AssetsSample[0];

        public EntityLiving(WorldBase world) : base(world)
        {
            Health = GetHelathMax();
            Standing();
            SpeedSurvival();
        }

        protected override void AddMetaData()
        {
            MetaData.Add(0, (byte)0); // флаги
            MetaData.Add(1, (short)300); // флаги
        }

        #region Flag

        /// <summary>
        /// Возвращает true, если флаг активен для сущности.Известные флаги:
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        protected bool GetFlag(int flag) => (MetaData.GetWatchableObjectByte(0) & 1 << flag) != 0;

        /// <summary>
        /// Включите или отключите флаг сущности
        /// 0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый
        /// </summary>
        /// <param name="flag">0) горит; 1) крадется; 2) едет на чем-то; 3) бегает; 4) ест; 5) невидимый</param>
        protected void SetFlag(int flag, bool set)
        {
            byte var3 = MetaData.GetWatchableObjectByte(0);
            if (set) MetaData.UpdateObject(0, (byte)(var3 | 1 << flag));
            else MetaData.UpdateObject(0, (byte)(var3 & ~(1 << flag)));
        }

        /// <summary>
        /// Ускорение
        /// </summary>
        public bool IsSprinting() => GetFlag(3);
        /// <summary>
        /// Задать значение ускоряется ли
        /// </summary>
        protected void SetSprinting(bool sprinting) => SetFlag(3, sprinting);

        /// <summary>
        /// Крадется
        /// </summary>
        public bool IsSneaking() => GetFlag(1);
        /// <summary>
        /// Задать значение крадется ли
        /// </summary>
        protected void SetSneaking(bool sneaking) => SetFlag(1, sneaking);

        /// <summary>
        /// Получить параметр кислорода
        /// </summary>
        public int GetAir() => MetaData.GetWatchableObjectShort(1);

        /// <summary>
        /// Задать параметр кислорода
        /// </summary>
        public void SetAir(int air) => MetaData.UpdateObject(1, (short)air);

        #endregion

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public override string GetName() => name == "" ? "entity." + Type.ToString() : name;

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public override bool HasCustomName() => name != "";

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected virtual float GetHelathMax() => 10;

        #region Input

        /// <summary>
        /// Нет нажатий
        /// </summary>
        public virtual void InputNone() => Input = EnumInput.None;
        /// <summary>
        /// Добавить нажатие
        /// </summary>
        public void InputAdd(EnumInput input) => Input |= input;
        /// <summary>
        /// Убрать нажатие
        /// </summary>
        public void InputRemove(EnumInput input)
        {
            if (Input.HasFlag(input)) Input ^= input;
        }

        #endregion

        #region Action

        /// <summary>
        /// Нет действий
        /// </summary>
        public void ActionNone() => ActionChanged = EnumActionChanged.None;
        /// <summary>
        /// Добавить действие
        /// </summary>
        public void ActionAdd(EnumActionChanged input) => ActionChanged |= input;
        /// <summary>
        /// Убрать действие
        /// </summary>
        public void ActionRemove(EnumActionChanged input)
        {
            if (ActionChanged.HasFlag(input)) ActionChanged ^= input;
        }

        /// <summary>
        /// Задать действие для позиции
        /// </summary>
        protected override void ActionAddPosition() => ActionAdd(EnumActionChanged.Position);
        /// <summary>
        /// Задать действие для вращения
        /// </summary>
        protected override void ActionAddLook() => ActionAdd(EnumActionChanged.Look);

        #endregion

        /// <summary>
        /// Проверяет, жив ли целевой объект
        /// </summary>
        public bool IsEntityAlive() => !IsDead && Health > 0f;

        /// <summary>
        /// Надо ли обрабатывать LivingUpdate, для мобов на сервере, и игроки у себя
        /// </summary>
        protected virtual bool IsLivingUpdate()
        {
            bool server = World is WorldServer;
            // TODO:: доработать проверку физики IsLivingUpdate
            bool player = this is EntityChicken;
            return server && player;
        }

        /// <summary>
        /// Обновления предметов которые могут видеть игроки, что в руке, броня
        /// </summary>
        protected void UpdateItems()
        {
            if (!World.IsRemote)
            {
                // Проверка изменения ячеке и если они изменены, то отправляем видящим клиентам
                for (int slot = 0; slot < previousEquipment.Length; ++slot)
                {
                    ItemStack itemStackPrev = previousEquipment[slot];
                    ItemStack itemStackNew = GetEquipmentInSlot(slot);

                    if (!ItemStack.AreItemStacksEqual(itemStackNew, itemStackPrev))
                    {
                        ((WorldServer)World).Tracker.SendToAllTrackingEntity(this, new PacketS04EntityEquipment(Id, slot, itemStackNew));
                        previousEquipment[slot] = itemStackNew?.Copy();
                    }
                }
            }
        }

        public override void Update()
        {
            TicksExistedMore();

            base.Update();

            EntityUpdate();

            if (!IsDead)
            {
                // Обновления предметов которые могут видеть игроки, что в руке, броня
                UpdateItems();

                if (IsLivingUpdate())
                {
                    // Если надо управление физики
                    LivingUpdate();
                }

                // Расчёт амплитуды движения 
                UpLimbSwing();

                // Просчёт взмаха руки
                UpdateArmSwingProgress();
            }
        }

        

        /// <summary>
        /// Обновление сущности на сервер
        /// </summary>
        protected void EntityUpdateServer()
        {
            if (!World.IsRemote)
            {
                // метод определения если есть ускорение и мы не на воде, определяем по нижниму блоку какой спавн частиц и спавним их
                // ... func_174830_Y

                // определяем горим ли мы, и раз в секунду %20, наносим урон
                // ...

                // определяем тонем ли мы
                DrownServer();

                //// если мы ниже -128 по Y убиваем игрока
                //if (Position.y < -128) Kill();
            }
        }

        /// <summary>
        /// Определение местоположение сущности
        /// </summary>
        protected void EntityUpdateLocation()
        {
            // метод проверки нахождения по кализии в воде ли мы, и меняем статус IsWater
            HandleLiquidMovement();

            // определяем в лаве ли мы по кализии
            // ... func_180799_ab
            if (IsInLava())
            {
                // надо поджечь
                //AttackEntityFrom(EnumDamageSource.Lava, 4f);
                fallDistance *= .5f;
            }
            if (IsInOil())
            {
                fallDistance *= .5f;
            }

            // если мы ниже -128 по Y убиваем игрока
            if (Position.y < -128)
            {
                AttackEntityFrom(EnumDamageSource.OutOfWorld, 100f);
            }
        }

        protected void EntityUpdate()
        {
            EntityUpdateLocation();

            // Только на сервере
            EntityUpdateServer();

            // Частички при беге блока
            ParticleBlockSprinting();

            // Было ли падение
            Fall();

            // если нет хп обновлям смертельную картинку
            if (Health <= 0f && DeathTime != -1) DeathUpdate();

            // Счётчик получения урона для анимации
            if (DamageTime > 0) DamageTime--;

            // Если был толчёк, мы его дабавляем и обнуляем
            if (!MotionPush.Equals(new vec3(0)))
            {
                vec3 motionPush = MotionPush;
                MotionPush = new vec3(0);
                // Защита от дабл прыжка, и если сущность летает, нет броска
                if (Motion.y > 0 || IsFlying) motionPush.y = 0;
                Motion += motionPush;
            }
        }

        /// <summary>
        /// Уменьшает подачу воздуха сущности под водой
        /// </summary>
        protected int DecreaseAirSupply(int oldAir, bool isOil)
        {
            //int air = 1; // TODO:: тут чара, броня и прочее в будущем для воды
            return oldAir - (isOil ? 2 : 1);
            //return air > 0 && rand.Next(air + 1) > 0 ? oldAir : oldAir - 1;
        }

        /// <summary>
        /// Определяем тонем ли мы
        /// </summary>
        protected void DrownServer()
        {
            if (IsEntityAlive())
            {
                bool isWater = IsInsideOfMaterial(EnumMaterial.Water);
                bool isOil = IsInsideOfMaterial(EnumMaterial.Oil);
                if (isWater || isOil)
                {
                    //if (!this.canBreatheUnderwater() && !this.isPotionActive(Potion.waterBreathing.id) && !var7)
                    {
                        SetAir(DecreaseAirSupply(GetAir(), isOil));

                        if (GetAir() == -20)
                        {
                            SetAir(0);

                            // эффект раз в секунду, и урон
                            //for (int var3 = 0; var3 < 8; ++var3)
                            //{
                            //    float var4 = this.rand.nextFloat() - this.rand.nextFloat();
                            //    float var5 = this.rand.nextFloat() - this.rand.nextFloat();
                            //    float var6 = this.rand.nextFloat() - this.rand.nextFloat();
                            //    this.worldObj.spawnParticle(EnumParticleTypes.WATER_BUBBLE, this.posX + (double)var4, this.posY + (double)var5, this.posZ + (double)var6, this.motionX, this.motionY, this.motionZ, new int[0]);
                            //}
                            //this.attackEntityFrom(DamageSource.drown, 2.0F);
                            AttackEntityFrom(EnumDamageSource.Drown, 2f);
                        }
                    }

                    //if (!World.IsRemote && this.IsRiding() && this.ridingEntity instanceof EntityLivingBase)
                    //{
                    //    this.mountEntity((Entity)null);
                    //}
                }
                else
                {
                    int air = GetAir();
                    if (air < 300)
                    {
                        // воссстановление кислорода в 2 раза быстрее
                        air += 2;
                        if (air > 300) air = 300;
                        SetAir(air);
                    }
                }
            }
        }

        /// <summary>
        /// Сущности наносит урон только на сервере
        /// </summary>
        /// <param name="amount">сила урона</param>
        /// <returns>true - урон был нанесён</returns>
        public bool AttackEntityFrom(EnumDamageSource source, float amount, string name = "")
        {
            if (World.IsRemote) return false;
            entityAge = 0;
            if (Health <= 0f) return false;

            Health -= amount;
            if (World is WorldServer worldServer)
            {
                if (Health <= 0f) worldServer.ServerMain.Log.Log("{1} {0}", source, this.name);
                worldServer.ResponseHealth(this);
            }
            
            return true;
        }

        /// <summary>
        /// Обновляет активное действие, и возращает strafe, forward, vertical через vec3
        /// </summary>
        /// <returns>strafe, forward</returns>
        protected vec2 UpdateEntityActionState()
        {
            entityAge++;
            float strafe = 0f;
            float forward = 0f;

            if (IsFlying)
            {
                float vertical = (Input.HasFlag(EnumInput.Up) ? 1f : 0f) - (Input.HasFlag(EnumInput.Down) ? 1f : 0f);
                float y = Motion.y;
                y += vertical * Speed.Vertical;
                Motion = new vec3(Motion.x, y, Motion.z);
                IsJumping = false;
            }

            // Прыжок, только выживание
            else if (IsJumping)
            {
                // для воды свои правила, плыть вверх
                if (IsInWater()) WaterUp();
                else if (IsInLava()) LavaUp();
                else if (IsInOil()) OilUp();
                // Для прыжка надо стоять на земле, и чтоб счётик прыжка был = 0
                else if (OnGround && jumpTicks == 0)
                {
                    Jump();
                    jumpTicks = 10;
                }
            }
            else
            {
                jumpTicks = 0;
                // для воды свои правила, плыть вниз
                if (Input.HasFlag(EnumInput.Down))
                {
                    if (IsInWater()) WaterDown();
                    else if (IsInLava()) LavaDown();
                    else if (IsInOil()) OilDown();
                }
            }

            strafe = (Input.HasFlag(EnumInput.Right) ? 1f : 0) - (Input.HasFlag(EnumInput.Left) ? 1f : 0);
            forward = (Input.HasFlag(EnumInput.Back) ? 1f : 0f) - (Input.HasFlag(EnumInput.Forward) ? 1f : 0f);

            // Обновить положение сидя
            bool isSneaking = IsSneaking();
            if (!IsFlying && (OnGround || IsInWater()) && Input.HasFlag(EnumInput.Down) && !isSneaking)
            {
                // Только в выживании можно сесть
                SetSneaking(true);
                Sitting();
            }
            // Если хотим встать
            if (!Input.HasFlag(EnumInput.Down) && IsSneaking())
            {
                // Проверка коллизии вверхней части при положении стоя
                Standing();
                // Хочется как-то ловить колизию положение встать в MoveCheckCollision
                if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position)))
                {
                    SetSneaking(false);
                }
                else
                {
                    Sitting();
                }
            }
            if (isSneaking != IsSneaking())
            {
                ActionAdd(EnumActionChanged.IsSneaking);
            }

            // Sprinting
            bool isSprinting = Input.HasFlag(EnumInput.Sprinting | EnumInput.Forward) && !IsSneaking();
            if (IsSprinting() != isSprinting)
            {
                SetSprinting(isSprinting);
                ActionAdd(EnumActionChanged.IsSprinting);
            }

            // Jumping
            IsJumping = Input.HasFlag(EnumInput.Up);

            return new vec2(strafe, forward);
        }

        /// <summary>
        /// Метод отвечает за жизнь сущности, точнее её управление, перемещения, мобы Ai
        /// должен работать у клиента для EntityPlayerSP и на сервере для мобов
        /// так же может работать у клиента для всех сущностей эффектов вне сервера.
        /// </summary>
        protected virtual void LivingUpdate()
        {
            // счётчик прыжка
            if (jumpTicks > 0) jumpTicks--;

            // Продумать перемещение по тактам, с параметром newPosRotationIncrements

            // Если нет перемещения по тактам, запускаем трение воздуха
            Motion = new vec3(Motion.x * .98f, Motion.y, Motion.z * .98f);

            // Если мелочь убираем
            Motion = new vec3(
                Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x,
                Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y,
                Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
            );

            float strafe = 0f;
            float forward = 0f;

            if (!IsMovementBlocked())
            {
                // Если нет блокировки
                vec2 sf = UpdateEntityActionState();
                strafe = sf.x;
                forward = sf.y;
            }

           // if (Health <= 0) InputNone();

            // Тут правильнее сделать метод updateEntityActionState 
            // где SP присваивает strafe и forward
            // или сервер Ai для мобов

            if (IsFlying)
            {
                float y = Motion.y;
                MoveWithHeading(strafe, forward, .6f * Speed.Forward * (IsSprinting() ? Speed.Sprinting : 1f));
                Motion = new vec3(Motion.x, y * .6f, Motion.z);
            }
            else
            {
                MoveWithHeading(strafe, forward, .04f);
            }
        }

        /// <summary>
        /// Проверка колизии по вектору движения
        /// </summary>
        /// <param name="motion">вектор движения</param>
        public void UpMoveCheckCollision(vec3 motion)
        {
            MoveEntity(motion);
        }

        /// <summary>
        /// Мертвые и спящие существа не могут двигаться
        /// </summary>
        protected bool IsMovementBlocked() => Health <= 0f;

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected virtual void HeadTurn() { }

        /// <summary>
        /// Возвращает элемент, который держит в руке
        /// </summary>
        public virtual ItemStack GetHeldItem() => null;

        /// <summary>
        /// Конвертация от направления движения в XYZ координаты с кооректировками скоростей
        /// </summary>
        protected void MoveWithHeading(float strafe, float forward, float jumpMovementFactor)
        {
            vec3 motion = new vec3();
            
            // трение
            float study;
            // делим на три части, вода, лава, остальное
            if (IsInWater())// && this is EntityPlayer)
            {
                float posY = Position.y;
                study = .8f; // .8f;
                // скорость
                float speed = .02f;// .02f;
                float speedEnch = 1f;//  (float)EnchantmentHelper.func_180318_b(this); чара
                //if (speedEnch > 3f) speedEnch = 3f;

                if (!OnGround) speedEnch *= .5f;

                if (speedEnch > 0f)
                {
                    study += (0.54600006f - study) * speedEnch / 3f;
                    speed += (GetAIMoveSpeed(strafe, forward) - speed) * speedEnch / 3f;
                }

                motion = MotionAngle(strafe, forward, speed);
                MoveEntity(motion);
                motion = Motion;
                motion.x *= study;
                motion.y *= .800001f;
                motion.z *= study;
                // Настроено на падение 0,4 м/с
                motion.y -= .008f;// .02f;

                // дополнительный прыжок, возле обрыва, если хотим вылести с воды на берег
                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(new vec3(motion.x, motion.y + .6f - Position.y + posY, motion.z)))
                {
                    // при 0.6 запрыгиваем на блок над водой
                    // при 0.3 запрыгиваем на блок в ровень с водой
                    motion.y = 0.600001f; 
                }
            }
            else if (IsInLava() || IsInOil())
            {
                // Lava или нефть
                float posY = Position.y;
                motion = MotionAngle(strafe, forward, .02f);
                MoveEntity(motion);
                motion *= .5f;
                motion.y -= .008f;
                // дополнительный прыжок, возле обрыва, если хотим вылести с воды на берег
                if (IsCollidedHorizontally && IsOffsetPositionInLiquid(new vec3(motion.x, motion.y + .6f - Position.y + posY, motion.z)))
                {
                    // при 0.6 запрыгиваем на блок над водой
                    // при 0.3 запрыгиваем на блок в ровень с водой
                    motion.y = 0.600001f;
                }
            }
            else
            // расматриваем остальное
            {
                // Коэффициент трения блока
                //float study = .954f;// 0.91f; // для воздух
                //if (OnGround) study = 0.739F;// 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f
                study = 0.91f; // для воздух
                if (OnGround)
                {
                    
                    study = World.GetBlockState(new BlockPos(Mth.Floor(Position.x), Mth.Floor(BoundingBox.Min.y) - 1, Mth.Floor(Position.z))).GetBlock().Slipperiness * .91f;
                }
                //if (OnGround) study = 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f

                //float param = 0.403583419f / (study * study * study);
                float param = 0.16277136f / (study * study * study);

                // трение, по умолчанию параметр ускорения падения вниз 
                float friction = jumpMovementFactor;
                if (OnGround)
                {
                    // корректировка скорости, с трением
                    friction = GetAIMoveSpeed(strafe, forward) * param;
                }

                motion = MotionAngle(strafe, forward, friction);

                // Тут надо корректировать леcтницу, вверх вниз Motion.y
                // ...

                // Проверка столкновения
                MoveEntity(motion);

                motion = Motion;
                // если есть горизонтальное столкновение и это лестница, то 
                // ... Motion.y = 0.2f;

                // Параметр падение 
                motion.y -= .16f;
                //motion.y -= .08f;

                motion.y *= .98f;
                motion.x *= study;
                motion.z *= study;
            }

            Motion = motion;
        }

        /// <summary>
        /// Скорость перемещения
        /// </summary>
        protected virtual float GetAIMoveSpeed(float strafe, float forward)
        {
            bool isSneaking = IsSneaking();
            float speed = Speed.Forward * Mth.Abs(forward);
            if (IsSprinting() && forward < 0 && !isSneaking)
            {
                // Бег 
                speed *= Speed.Sprinting;
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && isSneaking)
            {
                // Крадёмся
                speed *= Speed.Sneaking;
            }
            return speed;
        }

        /// <summary>
        /// Значения для првжка
        /// </summary>
        protected void Jump()
        {
            // Стартовое значение прыжка, чтоб на 6 так допрыгнут наивысшую точку в 2,5 блока
            vec3 motion = new vec3(0, .84f, 0);
            //vec3 motion = new vec3(0, .42f, 0);
            if (IsSprinting())
            {
                // Если прыжок с бегом, то скорость увеличивается
                motion.x += glm.sin(RotationYaw) * 0.4f;
                motion.z -= glm.cos(RotationYaw) * 0.4f;
                //motion.x += glm.sin(RotationYaw) * 0.2f;
                //motion.z -= glm.cos(RotationYaw) * 0.2f;
            }
            Motion = new vec3(Motion.x + motion.x, motion.y, Motion.z + motion.z);
        }
        //0,008
        /// <summary>
        /// Плыввёем вверх в воде
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void WaterUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в воде
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void WaterDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);
        /// <summary>
        /// Плыввёем вверх в лаве
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void LavaUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в лаве
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void LavaDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);
        /// <summary>
        /// Плыввёем вверх в нефте
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void OilUp() => Motion = new vec3(Motion.x, Motion.y + .048f, Motion.z);
        /// <summary>
        /// Плывём вниз в нефте
        /// настроено на скорость 2 м/с
        /// </summary>
        protected void OilDown() => Motion = new vec3(Motion.x, Motion.y - .032f, Motion.z);

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected vec3 MotionAngle(float strafe, float forward, float friction)
        {
            vec3 motion = Motion;

            float sf = strafe * strafe + forward * forward;
            if (sf >= 0.0001f)
            {
                sf = Mth.Sqrt(sf);
                if (sf < 1f) sf = 1f;
                sf = friction / sf;
                strafe *= sf;
                forward *= sf;
                float yaw = GetRotationYaw();
                float ysin = glm.sin(yaw);
                float ycos = glm.cos(yaw);
                motion.x += ycos * strafe - ysin * forward;
                motion.z += ycos * forward + ysin * strafe;
            }
            return motion;
        }

        /// <summary>
        /// Получить градус поворота по Yaw
        /// </summary>
        protected virtual float GetRotationYaw() => RotationYaw;

        #region Frame

        /// <summary>
        /// Скорость движения для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public float GetLimbSwingAmountFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || LimbSwingAmount.Equals(limbSwingAmountPrev)) return LimbSwingAmount;
            return limbSwingAmountPrev + (LimbSwingAmount - limbSwingAmountPrev) * timeIndex;
        }

        /// <summary>
        /// Получить анимацию руки для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public virtual float GetSwingProgressFrame(float timeIndex)
        {
            if (isSwingInProgress)
            {
                if (timeIndex >= 1.0f || swingProgress.Equals(swingProgressPrev)) return swingProgress;
                return swingProgressPrev + (swingProgress - swingProgressPrev) * timeIndex;
            }
            return 0;
        }

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public virtual float GetEyeHeightFrame() => GetEyeHeight();

        #endregion

        /// <summary>
        /// Высота глаз
        /// </summary>
        public virtual float GetEyeHeight() => Height * 0.85f;

        /// <summary>
        /// Расчёт амплитуды конечностей, при движении
        /// </summary>
        protected void UpLimbSwing()
        {
            limbSwingAmountPrev = LimbSwingAmount;
            float xx = Position.x - PositionPrev.x;
            float zz = Position.z - PositionPrev.z;
            float xxzz = xx * xx + zz * zz;
            float qxxzz = Mth.Sqrt(xxzz);
            float xz = qxxzz * 1.4f;
            if (xz > 1.0f) xz = 1.0f;
            LimbSwingAmount += (xz - LimbSwingAmount) * 0.4f;
            LimbSwing += LimbSwingAmount;

            distanceWalkedOnStepModified += qxxzz * .3f;
            if (distanceWalkedOnStepModified > nextStepDistance)
            {
                BlockBase blockDown = World.GetBlockState(new BlockPos(Position.x, Position.y - 0.20002f, Position.z)).GetBlock();
                if (!blockDown.IsAir)
                {
                    nextStepDistance = (int)distanceWalkedOnStepModified + 1;
                    SoundStep(blockDown);
                }
            }
        }

        /// <summary>
        /// Звуковой эффект шага
        /// </summary>
        protected void SoundStep(BlockBase blockDown)
        {
            if (IsInWater())
            {
                // Звук в воде
                SoundEffectWater(Blocks.GetBlockCache(EnumBlock.Water).SampleStep(World), .35f);
            }
            else if (!IsSneaking() && blockDown.Material != EnumMaterial.Water && IsSampleStep(blockDown))
            {
                // Звук шага
                World.PlaySound(this, SampleStep(World, blockDown), Position, .25f, 1f);
            }
        }

        private void SoundEffectWater(AssetsSample assetsSample, float volumeFX)
        {
            // Звук в воде
            float xx = Position.x - PositionPrev.x;
            float yy = Position.y - PositionPrev.y;
            float zz = Position.z - PositionPrev.z;
            // В зависимости от скорости перемещения вводе меняем звук
            float volume = Mth.Sqrt(xx * xx * .2f + yy * yy + zz * zz * .2f) * volumeFX;
            // 0.03 - 0.2 когда плыву, 0.4 - 0.7 когда ныряю
            if (volume > 1f) volume = 1f;

            World.PlaySound(this, assetsSample, Position, volume, 1f + (float)(rand.NextDouble() - rand.NextDouble()) * .4f);
        }

        /// <summary>
        /// Эффект попадания в воду
        /// </summary>
        protected override void EffectsFallingIntoWater()
        {
            SoundEffectWater(Blocks.GetBlockCache(EnumBlock.Water).SampleBreak(World), .2f);
            vec3 pos = new vec3(0, BoundingBox.Min.y + 1f, 0);
            vec3 motion = Motion;
            float width = Width * 2f;
            //for(int i = 0; i < 10; i++)
            //{
            //    motion.y = Motion.y - (float)rand.NextDouble() * .2f;
            //    pos.x = Position.x + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    pos.z = Position.z + ((float)rand.NextDouble() * 2f - 1f) * width;
            //    World.SpawnParticle(EnumParticle.Suspend, pos, motion);
            //}
            for (int i = 0; i < 10; i++)
            {
                motion.y = Motion.y - (float)rand.NextDouble() * .2f;
                pos.x = Position.x + ((float)rand.NextDouble() * 2f - 1f) * width;
                pos.z = Position.z + ((float)rand.NextDouble() * 2f - 1f) * width;
                World.SpawnParticle(EnumParticle.Digging, pos, motion, 1);
            }
        }

        /// <summary>
        /// Воздействия при нахождении в воде
        /// </summary>
        protected override void EffectsContactWithWater()
        {
            fallDistance = 0f;
            //fire = 0;
        }

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public virtual bool IsSampleStep(BlockBase blockDown) => samplesStep.Length > 0;

        /// <summary>
        /// Семпл хотьбы
        /// </summary>
        public virtual AssetsSample SampleStep(WorldBase worldIn, BlockBase blockDown) => samplesStep[worldIn.Rand.Next(samplesStep.Length)];

        /// <summary>
        /// Скакой скоростью анимируется удар рукой, в тактах, менять можно от инструмента, чар и навыков
        /// </summary>
        private int GetArmSwingAnimationEnd() => 6; 

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public virtual void SwingItem()
        {
            if (!isSwingInProgress || swingProgressInt >= GetArmSwingAnimationEnd() / 2 || swingProgressInt < 0)
            {
                swingProgressInt = -1;
                isSwingInProgress = true;

                if (World is WorldServer)
                {
                    ((WorldServer)World).Tracker.SendToAllTrackingEntity(this, new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem));
                    //((WorldServer)World).Players.ResponsePacketAll(new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem), Id);
                }
            }
        }

        /// <summary>
        /// Обновляет счетчики прогресса взмаха руки и прогресс анимации. 
        /// </summary>
        protected void UpdateArmSwingProgress()
        {
            swingProgressPrev = swingProgress;

            int asa = GetArmSwingAnimationEnd();

            if (isSwingInProgress)
            {
                swingProgressInt++;
                if (swingProgressInt >= asa)
                {
                    swingProgressInt = 0;
                    isSwingInProgress = false;
                }
            }
            else
            {
                swingProgressInt = 0;
            }

            swingProgress = (float)swingProgressInt / (float)asa;
        }

        ///// <summary>
        ///// Задать положение сидя и бега
        ///// </summary>
        //public virtual void SetSneakingSprinting(bool sneaking, bool sprinting)
        //{
        //    if (IsSneaking() != sneaking)
        //    {
        //        SetSneaking(sneaking);
        //        if (sneaking) Sitting(); else Standing();
        //    }
        //    SetSprinting(sprinting);
        //}

        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetMotionServer(vec3 pos, float yaw, float pitch, bool onGround)
        {
            //if (IsSneaking != sneaking)
            //{
            //    IsSneaking = sneaking;
            //    if (IsSneaking) Sitting(); else Standing();
            //}
            //IsSprinting = sprinting;
            RotationYawServer = yaw;
            RotationPitchServer = pitch;
            SetMotionServer(pos, onGround);
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public virtual void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            SetRotation(yaw, pitch);
            RotationYawServer = RotationYawPrev = RotationYaw;
            RotationPitchServer = RotationPitchPrev = RotationPitch;
            PositionServer = PositionPrev = LastTickPos = Position;
        }

        /// <summary>
        /// Дополнительное обновление сущности в клиентской части в зависимости от сущности
        /// </summary>
        protected virtual void UpdateEntityRotation() => SetRotation(RotationYawServer, RotationPitchServer);

        /// <summary>
        /// Управляет таймером смерти сущности, сферой опыта и созданием частиц
        /// </summary>
        protected void DeathUpdate()
        {
            DeathTime++;

            if (DeathTime >= 20)
            {
                for (int i = 0; i < 100; i++)
                {
                    World.SpawnParticle(EnumParticle.Test,
                    Position + new vec3(((float)rand.NextDouble() - .5f) * Width,
                    .1f,
                    ((float)rand.NextDouble() - .5f) * Width),
                    new vec3(0));
                }
                DeathTime = -1;
                SetDead();

                //Health = 20;
                //    int var1;

                //    if (!this.worldObj.isRemote && (this.recentlyHit > 0 || this.isPlayer()) && this.func_146066_aG() && this.worldObj.getGameRules().getGameRuleBooleanValue("doMobLoot"))
                //    {
                //        var1 = this.getExperiencePoints(this.attackingPlayer);

                //        while (var1 > 0)
                //        {
                //            int var2 = EntityXPOrb.getXPSplit(var1);
                //            var1 -= var2;
                //            this.worldObj.spawnEntityInWorld(new EntityXPOrb(this.worldObj, this.posX, this.posY, this.posZ, var2));
                //        }
                //    }

                //    this.setDead();

                //    for (var1 = 0; var1 < 20; ++var1)
                //    {
                //        double var8 = this.rand.nextGaussian() * 0.02D;
                //        double var4 = this.rand.nextGaussian() * 0.02D;
                //        double var6 = this.rand.nextGaussian() * 0.02D;
                //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, this.posY + (double)(this.rand.nextFloat() * this.height), this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, var8, var4, var6, new int[0]);
                //    }
            }
        }

        /// <summary>
        /// Возобновить сущность
        /// </summary>
        public virtual void Respawn()
        {
            Health = GetHelathMax();
            DeathTime = 0;
            fallDistanceResult = 0f;
            fallDistance = 0f;
            Motion = new vec3(0);
            IsDead = false;
        }

        /// <summary>
        /// Задать новое значение здоровья
        /// </summary>
        public void SetHealth(float health) => Health = health < 0 ? 0 : health;

        /// <summary>
        /// Начать анимацию боли
        /// </summary>
        public void PerformHurtAnimation() => DamageTime = 5; // количество тактов

        /// <summary>
        /// Определяем дистанцию падения
        /// </summary>
        /// <param name="y">позиция Y</param>
        protected override void FallDetection(float y)
        {
            if (!IsInWater())
            {
                HandleLiquidMovement();
            }

            if (y < 0f) fallDistance -= y;
            if (OnGround)
            {
                if (IsFlying)
                {
                    ModeSurvival();
                    fallDistance = 0f;
                }
                else if (fallDistance > 0f)
                {
                    // Упал
                    fallDistanceResult = fallDistance;
                    fallDistance = 0f;
                }
            }
        }

        /// <summary>
        /// Падение
        /// </summary>
        protected virtual void Fall() { }

        /// <summary>
        /// Добавить тик жизни к сущности
        /// </summary>
        protected void TicksExistedMore() => TicksExisted++;

        /// <summary>
        /// Обновление сущности в клиентской части
        /// </summary>
        public override void UpdateClient()
        {
            base.UpdateClient();
            RotationPitchPrev = RotationPitch;
            RotationYawPrev = RotationYaw;
            UpdateEntityRotation();
        }

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected virtual void Standing() => SetSize(.6f, 3.6f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected virtual void Sitting() => SetSize(.6f, 2.99f);

        /// <summary>
        /// Активация режима полёта
        /// </summary>
        public void ModeFly()
        {
            if (!IsFlying)
            {
                IsFlying = true;
                Standing();
                SpeedFly();
            }
        }

        /// <summary>
        /// Активация режима выживания
        /// </summary>
        public void ModeSurvival()
        {
            if (IsFlying)
            {
                IsFlying = false;
                SpeedSurvival();
            }
        }

        /// <summary>
        /// Скорость для режима полёта
        /// </summary>
        protected virtual void SpeedFly() => Speed = new EntitySpeed(.2f, .2f, .3f, 5.0f);

        /// <summary>
        /// Скорость для режима выживания
        /// </summary>
        protected virtual void SpeedSurvival() => Speed = new EntitySpeed(.2f);//3837f);

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => !IsDead;

        /// <summary>
        /// Получить стак что в правой руке 0 или броня 1-4
        /// </summary>
        public virtual ItemStack GetEquipmentInSlot(int slot) => null;
        /// <summary>
        /// Получить слот брони 0-3
        /// </summary>
        public virtual ItemStack GetCurrentArmor(int slot) => null;
        /// <summary>
        /// Задать стак в слот, что в правой руке 0, или 1-4 слот брони
        /// </summary>
        public virtual void SetCurrentItemOrArmor(int slot, ItemStack itemStack) { }

        /// <summary>
        /// Вызывается всякий раз, когда предмет подбирается
        /// </summary>
        public void ItemPickup(EntityBase entity, int amount)
        {
            if (!entity.IsDead && !World.IsRemote)
            {
                EntityTracker tracker = ((WorldServer)World).Tracker;
                if (entity is EntityItem)
                {
                    tracker.SendToAllTrackingEntityCurrent(this, new PacketS0DCollectItem(Id, entity.Id));
                }
            }
        }

        /// <summary>
        /// Частички при беге блока
        /// </summary>
        private void ParticleBlockSprinting()
        {
            if (IsSprinting() && !IsInWater())
            {
                ParticleBlockDown(Position, 1);
                // TODO::звук при беге
            }
        }
        /// <summary>
        /// Частички при падении
        /// </summary>
        public void ParticleFall(float distance)
        {
            if (distance > 2)
            {
                int count = (int)distance + 2;
                if (count > 20) count = 20;
                ParticleBlockDown(Position, count);
                // TODO::звук при падении
            }
        }

        /// <summary>
        /// Частички под ногами, бег или падение
        /// </summary>
        private void ParticleBlockDown(vec3 pos, int count)
        {
            BlockBase block = World.GetBlockState(new BlockPos(pos.x, pos.y - 0.20002f, pos.z)).GetBlock();
            if (block.IsParticle)
            {
                for (int i = 0; i < count; i++)
                {
                    World.SpawnParticle(EnumParticle.Digging,
                    pos + new vec3(((float)rand.NextDouble() - .5f) * Width,
                    .1f,
                    ((float)rand.NextDouble() - .5f) * Width), 
                    new vec3(0),
                    (int)block.EBlock);
                }
            }
        }

        

        /// <summary>
        /// Проверяет, находится ли смещенная позиция от текущей позиции объекта внутри жидкости
        /// </summary>
        /// <param name="vec">вектор смещения</param>
        private bool IsOffsetPositionInLiquid(vec3 vec)
        {
            AxisAlignedBB aabb = BoundingBox.Offset(vec);
            return World.Collision.GetCollidingBoundingBoxes(aabb).Count == 0
                && !World.IsAnyLiquid(aabb);
        }

        /// <summary>
        /// Проверяет, относится ли текущий блок объекта находящий на глазах к указанному типу материала
        /// </summary>
        /// <param name="materialIn"></param>
        public bool IsInsideOfMaterial(EnumMaterial materialIn)
        {
            float y = Position.y + GetEyeHeight();
            BlockPos blockPos = new BlockPos(Position.x, y, Position.z);
            BlockBase block = World.GetBlockState(blockPos).GetBlock();

            if (block.Material == materialIn)
            {
                // нужна проверка течении воды, у неё блок не целый
                //float var7 = BlockLiquid.getLiquidHeightPercent(var5.getBlock().getMetaFromState(var5)) - 0.11111111F;
                //float var8 = (float)(blockPos.Y + 1f) - var7;
                //bool var9 = y < (double)var8;
                //return !var9 && this is EntityPlayer ? false : var9;
                return true;
                     
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Создает частицу взрыва вокруг местоположения Сущности
        /// </summary>
        public void SpawnExplosionParticle()
        {
            //if (this.worldObj.isRemote)
            //{
            //    for (int var1 = 0; var1 < 20; ++var1)
            //    {
            //        double var2 = this.rand.nextGaussian() * 0.02D;
            //        double var4 = this.rand.nextGaussian() * 0.02D;
            //        double var6 = this.rand.nextGaussian() * 0.02D;
            //        double var8 = 10.0D;
            //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var2 * var8, this.posY + (double)(this.rand.nextFloat() * this.height) - var4 * var8, this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var6 * var8, var2, var4, var6, new int[0]);
            //    }
            //}
            //else
            //{
            //    this.worldObj.setEntityState(this, (byte)20);
            //}
        }

        /// <summary>
        /// Визуализирует частицы сломанных предметов, используя заданный ItemStack
        /// </summary>
        /// <param name="p_70669_1_"></param>
        public void RenderBrokenItemStack(EnumBlock p_70669_1_)
        {
        //    this.playSound("random.break", 0.8F, 0.8F + this.worldObj.rand.nextFloat() * 0.4F);

        //    for (int var2 = 0; var2< 5; ++var2)
        //    {
        //        Vec3 var3 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.1D, Math.random() * 0.1D + 0.1D, 0.0D);
        //var3 = var3.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var3 = var3.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //double var4 = (double)(-this.rand.nextFloat()) * 0.6D - 0.3D;
        //Vec3 var6 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.3D, var4, 0.6D);
        //var6 = var6.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var6 = var6.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //var6 = var6.addVector(this.posX, this.posY + (double)this.getEyeHeight(), this.posZ);
        //        this.worldObj.spawnParticle(EnumParticleTypes.ITEM_CRACK, var6.xCoord, var6.yCoord, var6.zCoord, var3.xCoord, var3.yCoord + 0.05D, var3.zCoord, new int[] { Item.getIdFromItem(p_70669_1_.getItem()) });
        //    }
        }

        // Визуализирует частицы сломанных предметов, используя заданный ItemStack
        // renderBrokenItemStack
    }
}
