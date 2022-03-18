using MvkServer.Util;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект процесса разрушения блока
    /// </summary>
    public class DestroyBlockProgress
    {
        public BlockPos Position { get; private set; }
        /// <summary>
        /// Значение урона
        /// </summary>
        public int PartialBlockProgress { get; private set; } = -1;
        /// <summary>
        /// Время такта, когда был создан
        /// </summary>
        public uint CreatedAtCloudUpdateTick { get; private set; }
        /// <summary>
        /// Id сущности игрока который разрушает блок
        /// </summary>
        public int BreakerId { get; private set; }

        public DestroyBlockProgress(int breakerId, BlockPos blockPos)
        {
            BreakerId = breakerId;
            Position = blockPos;
        }

        /// <summary>
        /// Вставляет значение урона в этот частично разрушенный блок. 
        /// -1 заставляет клиентский рендерер удалить его, в противном случае колеблется от 1 до 10
        /// </summary>
        public void SetPartialBlockDamage(int damage)
        {
            if (damage > 10) damage = 10;
            PartialBlockProgress = damage;
        }
        
        /// <summary>
        /// Задать текущий такт обновления
        /// </summary>
        public void SetCloudUpdateTick(uint tick) => CreatedAtCloudUpdateTick = tick;
    }
}
