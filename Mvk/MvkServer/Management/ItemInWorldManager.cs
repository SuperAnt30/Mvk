using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект работы над игроком разрушения блока, итомы и прочее
    /// </summary>
    public class ItemInWorldManager
    {
        /// <summary>
        /// Позиция блока который разрушаем
        /// </summary>
        public BlockPos BlockPosDestroy { get; private set; } = new BlockPos();
        /// <summary>
        /// Направление установки блока
        /// </summary>
        private vec3 facing;
        /// <summary>
        /// Тип блока для установки
        /// </summary>
        //private EnumBlock enumBlock = EnumBlock.None;
        /// <summary>
        /// Номер слота, с какого ставим блок
        /// </summary>
        private int slotPut = 0;
        /// <summary>
        /// Пауза между ударами, если удар не отключали (Update)
        /// </summary>
        private int pause = 0;
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        private WorldBase world;
        /// <summary>
        /// Объект игрока
        /// </summary>
        private EntityPlayer entityPlayer;
        /// <summary>
        /// Начальный урон блока
        /// </summary>
        private int initialDamage;
        /// <summary>
        /// Счётчик тактов нанесения урона на блок
        /// </summary>
        private int curblockDamage;
        /// <summary>
        /// Прочность, оставшаяся в блоке.
        /// -1 отмена, -2 сломан блок, -3 ставим блок
        /// </summary>
        private int durabilityRemainingOnBlock = -1;
        /// <summary>
        /// Статус действия
        /// </summary>
        private Status status = Status.None;

        public ItemInWorldManager(WorldBase world, EntityPlayer entityPlayer)
        {
            this.world = world;
            this.entityPlayer = entityPlayer;
        }

        /// <summary>
        /// Нет паузы в обновлении между ударами
        /// </summary>
        public bool NotPauseUpdate => pause == 0;

        /// <summary>
        /// Истинно, если игрок уничтожает блок или ставит
        /// </summary>
        public bool IsDestroyingBlock => !BlockPosDestroy.IsEmpty;

        /// <summary>
        /// Начато разрушение
        /// </summary>
        public void DestroyStart(BlockPos blockPos)
        {
            BlockBase block = world.GetBlock(blockPos);
            if (block != null && !block.IsAir)
            {
                BlockPosDestroy = blockPos;
                curblockDamage = 0;
                initialDamage = block.GetPlayerRelativeBlockHardness(entityPlayer);
                durabilityRemainingOnBlock = GetProcess();
                if (durabilityRemainingOnBlock < 0)
                {
                    // При старте нельзя помечать как стоп (-2) это должно быть в игровом такте
                    durabilityRemainingOnBlock = 0;
                }
                pause = 5;
            } 
        }

        /// <summary>
        /// Отмена разрушения
        /// </summary>
        public void DestroyAbout() => status = Status.About;

        /// <summary>
        /// Окончено разрушение, блок сломан
        /// </summary>
        public void DestroyStop() => status = Status.Stop;

        /// <summary>
        /// Установить блок
        /// </summary>
        public void Put(BlockPos blockPos, vec3 facing, int slot)
        {
            BlockBase block = world.GetBlock(blockPos);
            if (block != null && block.IsAir)
            {
                BlockPosDestroy = blockPos;
                this.facing = facing;
                slotPut = slot;
                status = Status.Put;
            }
        }

        /// <summary>
        /// Пауза между установками блоков, в тактах.
        /// true - между первым блоком и вторы,
        /// false - последующие, 4 успевает поставить 2 блока, когда прыгает
        /// </summary>
        public void PutPause(bool start) => pause = start ? 8 : 4;

        /// <summary>
        /// Установить блок не получилось, но мы фиксируем зажатие клавиши
        /// </summary>
        public void PutAbout()
        {
            BlockPosDestroy = new BlockPos();
            pause = 0;
        }

        /// <summary>
        /// Обновление разрушения блока
        /// </summary>
        public StatusAnimation UpdateBlock()
        {
            StatusAnimation statusUpdate = StatusAnimation.NotAnimation;

            if (status != Status.None)
            {
                durabilityRemainingOnBlock = (int)status;
                status = Status.None;
            }

            if (IsDestroyingBlock)
            {
                curblockDamage++;

                if (durabilityRemainingOnBlock >= 0)
                {
                    BlockBase block = world.GetBlock(BlockPosDestroy);
                    if (block == null || block.IsAir)
                    {
                        durabilityRemainingOnBlock = (int)Status.About;
                    }
                }

                if (durabilityRemainingOnBlock < 0)
                {
                    if (durabilityRemainingOnBlock == (int)Status.About)
                    {
                        // Отмена действия блока, убираем трещину
                        world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, durabilityRemainingOnBlock);
                    }
                    else if (durabilityRemainingOnBlock == (int)Status.Stop)
                    {
                        // Уничтожение блока
                        BlockBase.SpawnAsEntity(world, BlockPosDestroy, new ItemStack(world.GetBlock(BlockPosDestroy)));
                        world.SetBlockState(BlockPosDestroy, EnumBlock.Air);
                    }
                    else if (durabilityRemainingOnBlock == (int)Status.Put)
                    {
                        // Ставим блок
                        BlockBase blockOld = world.GetBlock(BlockPosDestroy);
                        if (blockOld != null)
                        {
                            if (world is WorldServer && entityPlayer is EntityPlayerServer entityPlayerServer)
                            {
                                ItemStack itemStack = entityPlayer.Inventory.GetStackInSlot(slotPut);
                                if (itemStack != null 
                                    && itemStack.ItemUse(entityPlayer, world, BlockPosDestroy, new EnumFacing(), new vec3(0)))
                                {
                                    // установлен
                                    entityPlayer.Inventory.DecrStackSize(slotPut, 1);
                                }
                                // TODO::20220401 обработка инвентаря на сервере при установке блока
                                // Для серверной части нужна проверка коллизии кроме тикущей сущности
                                //AxisAlignedBB axisBlock = blockNew.GetCollision();
                                //if (blockOld.IsAir && world.GetEntitiesWithinAABBExcludingEntity(entityPlayer, axisBlock).Count == 0)
                                //{
                                //    // Устанавливаем блок
                                //    world.SetBlockState(BlockPosDestroy, enumBlock);
                                //}
                                //else
                                //{
                                //    // Проверка отката, если блок не корректно установился, сервер вернёт прошлое значение
                                //    world.SetBlockState(BlockPosDestroy, blockOld.EBlock);
                                //}
                            }
                            else
                            {
                                // Для клиентской
                                statusUpdate = StatusAnimation.Animation;
                               // world.SetBlockState(BlockPosDestroy, enumBlock);
                            }
                        }
                    }
                    BlockPosDestroy = new BlockPos();
                }
                else
                {
                    int process = GetProcess();
                    if (curblockDamage == 1)
                    {
                        // Первый удар на разрушении блока, статус начала анимации
                        statusUpdate = StatusAnimation.Animation;
                    }
                    else
                    {
                        statusUpdate = StatusAnimation.None;
                    }

                    if (process != durabilityRemainingOnBlock)
                    {
                        durabilityRemainingOnBlock = process;
                        world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, durabilityRemainingOnBlock);
                    }
                }
            }
            else
            {
                if (pause > 0) pause--;
            }

            return statusUpdate;
        }

        /// <summary>
        /// Проверка на блок тольо что сломался
        /// </summary>
        public bool IsDestroy() => IsDestroyingBlock && curblockDamage >= initialDamage;

        /// <summary>
        /// Получить значение процесса
        /// </summary>
        private int GetProcess()
        {
            int process = initialDamage <= 1 ? (int)Status.Stop : curblockDamage * 10 / initialDamage;
            if (process > 9) process = (int)Status.Stop;
            return process;
        }

        /// <summary>
        /// Статус анимации руки после обнавления игрового такта
        /// </summary>
        public enum StatusAnimation
        {
            /// <summary>
            /// Нет значения
            /// </summary>
            None,
            /// <summary>
            /// Есть анимация
            /// </summary>
            Animation,
            /// <summary>
            /// Нет анимации
            /// </summary>
            NotAnimation
        }

        private enum Status
        {
            None = 0,
            About = -1,
            Stop = -2,
            Put = -3
        }
    }
}
