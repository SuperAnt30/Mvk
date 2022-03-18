namespace MvkServer.World.Block
{
    /// <summary>
    /// Тип блока
    /// </summary>
    public enum EnumBlock
    {
        /// <summary>
        /// Отсутствие блока, он же воздух, но с коллизией, пройти через него нельзя
        /// </summary>
        None = -1,
        /// <summary>
        /// Воздух
        /// </summary>
        Air = 0,
        /// <summary>
        /// Камень
        /// </summary>
        Stone = 1,
        /// <summary>
        /// Булыжник
        /// </summary>
        Cobblestone = 2,
        /// <summary>
        /// Земля
        /// </summary>
        Dirt = 3,
        /// <summary>
        /// Дёрн
        /// </summary>
        Turf = 4
    }

    /// <summary>
    /// Количество блоков
    /// </summary>
    public class BlocksCount
    {
        public const int COUNT = 4;
    }
}
