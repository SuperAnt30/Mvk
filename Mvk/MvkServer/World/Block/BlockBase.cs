using MvkServer.Util;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public abstract class BlockBase
    {
        /// <summary>
        /// Коробки
        /// </summary>
        public Box[] Boxes { get; protected set; } = new Box[] { new Box() };
        /// <summary>
        /// Хит бокс блока
        /// </summary>
        public Box HitBox { get; protected set; } = new Box();
        /// <summary>
        /// Получить тип блока
        /// </summary>
        public EnumBlock EBlock { get; protected set; }
        /// <summary>
        /// Позиция блока в мире
        /// </summary>
        public BlockPos Position { get; protected set; } = new BlockPos();
        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir => EBlock == EnumBlock.Air;
        /// <summary>
        /// Трава ли это
        /// </summary>
        public bool IsGrass { get; protected set; } = false;
        /// <summary>
        /// Есть ли столкновение
        /// </summary>
        public bool IsCollision { get; protected set; } = true;


        /// <summary>
        /// Задать позицию блока
        /// </summary>
        public void SetPosition(BlockPos pos) => Position = pos;
        /// <summary>
        /// Задать тип блока
        /// </summary>
        public void SetEnumBlock(EnumBlock enumBlock) => EBlock = enumBlock;

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => EBlock.ToString() + " " + Position.ToString();
    }
}
