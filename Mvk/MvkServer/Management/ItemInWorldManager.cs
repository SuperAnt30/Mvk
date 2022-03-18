using MvkServer.Entity.Player;
using MvkServer.Glm;
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
        /// Прочность, оставшаяся в блоке 
        /// </summary>
        private int durabilityRemainingOnBlock = -1;

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
        /// Истинно, если игрок уничтожает блок
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
                pause = 5;
                world.SendBlockBreakProgress(entityPlayer.Id, blockPos, durabilityRemainingOnBlock);
            }
        }

        /// <summary>
        /// Отмена разрушения
        /// </summary>
        public void DestroyAbout()
        {
            world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, -1);
            durabilityRemainingOnBlock = -1;
            BlockPosDestroy = new BlockPos();
        }

        /// <summary>
        /// Окончено разрушение, блок сломан
        /// </summary>
        public void DestroyStop()
        {
            world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, -2);
            durabilityRemainingOnBlock = -1;
            world.SetBlockState(BlockPosDestroy, EnumBlock.Air);
            BlockPosDestroy = new BlockPos();
        }

        /// <summary>
        /// Установить блок
        /// </summary>
        public void Put(BlockPos blockPos, vec3 facing)
        {
            BlockBase block = world.GetBlock(blockPos);
            if (block != null)
            {
                if (block.IsAir)
                {
                    // TODO:: надо занести в обновление чтоб проверка и установка была в такте игровом
                    if (world.GetEntitiesWithinAABBExcludingEntity(null, block.GetCollision()).Count == 0)
                    {
                        BlockPosDestroy = new BlockPos();
                        curblockDamage = 0;
                        initialDamage = 0;
                        durabilityRemainingOnBlock = -1;
                        pause = 5;
                        world.SetBlockState(blockPos, (EnumBlock)entityPlayer.slot);
                        return;
                    }
                }
                if (world is WorldServer)
                {
                    world.SetBlockState(blockPos, block.EBlock);
                }
            }
        }

        /// <summary>
        /// Обновление разрушения блока
        /// </summary>
        public void UpdateBlock()
        {
            if (pause > 0) pause--;
            if (IsDestroyingBlock)
            {
                curblockDamage++;

                BlockBase block = world.GetBlock(BlockPosDestroy);
                if (block == null || block.IsAir)
                {
                    DestroyAbout();
                }
                else
                {
                    int process = GetProcess();
                    if (process != durabilityRemainingOnBlock)
                    {
                        durabilityRemainingOnBlock = process;
                        world.SendBlockBreakProgress(entityPlayer.Id, BlockPosDestroy, durabilityRemainingOnBlock);
                    }
                }
            }
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
            int process = initialDamage <= 1 ? -1 : curblockDamage * 10 / initialDamage;
            if (process > 9) process = -1;
            return process;
        }
    }
}
