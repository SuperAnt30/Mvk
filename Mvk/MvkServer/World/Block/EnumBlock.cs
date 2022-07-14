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
        /// Коренная порода
        /// </summary>
        Bedrock = 1,
        /// <summary>
        /// Камень
        /// </summary>
        Stone = 2,
        /// <summary>
        /// Булыжник
        /// </summary>
        Cobblestone = 3,
        /// <summary>
        /// Известняк
        /// </summary>
        Limestone = 4,
        /// <summary>
        /// Диорит
        /// </summary>
        Diorite = 5,
        /// <summary>
        /// Песчаник
        /// </summary>
        Sandstone = 6,
        /// <summary>
        /// Земля
        /// </summary>
        Dirt = 7,
        /// <summary>
        /// Дёрн
        /// </summary>
        Turf = 8,
        /// <summary>
        /// Песок
        /// </summary>
        Sand = 9,
        /// <summary>
        /// Гравий
        /// </summary>
        Gravel = 10,
        /// <summary>
        /// Глина
        /// </summary>
        Clay = 11,
        /// <summary>
        /// Вода
        /// </summary>
        Water = 12,
        /// <summary>
        /// Текучая вода
        /// </summary>
        WaterFlowing = 13,
        /// <summary>
        /// Лава
        /// </summary>
        Lava = 14,
        /// <summary>
        /// Текучая лава
        /// </summary>
        LavaFlowing = 15,
        /// <summary>
        /// Нефть
        /// </summary>
        Oil = 16,
        /// <summary>
        /// Текучая нефть
        /// </summary>
        OilFlowing = 17,
        /// <summary>
        /// Бревно дуб
        /// </summary>
        LogOak = 18,
        /// <summary>
        /// Бревно берёза
        /// </summary>
        LogBirch = 19,
        /// <summary>
        /// Бревно ель
        /// </summary>
        LogSpruce = 20,
        /// <summary>
        /// Бревно плодовое
        /// </summary>
        LogFruit = 21,
        /// <summary>
        /// Бревно пальмы
        /// </summary>
        LogPalm = 22,
        /// <summary>
        /// Доски дуб
        /// </summary>
        PlanksOak = 23,
        /// <summary>
        /// Доски берёза
        /// </summary>
        PlanksBirch = 24,
        /// <summary>
        /// Доски ель
        /// </summary>
        PlanksSpruce = 25,
        /// <summary>
        /// Доски плодовое
        /// </summary>
        PlanksFruit = 26,
        /// <summary>
        /// Доски пальмы
        /// </summary>
        PlanksPalm = 27,
        /// <summary>
        /// Листва дуб
        /// </summary>
        LeavesOak = 28,
        /// <summary>
        /// Листва берёза
        /// </summary>
        LeavesBirch = 29,
        /// <summary>
        /// Листва ель
        /// </summary>
        LeavesSpruce = 30,
        /// <summary>
        /// Листва плодовое
        /// </summary>
        LeavesFruit = 31,
        /// <summary>
        /// Листва пальмы
        /// </summary>
        LeavesPalm = 32,
        /// <summary>
        /// Саженец дуб
        /// </summary>
        SaplingOak = 33,
        /// <summary>
        /// Саженец берёза
        /// </summary>
        SaplingBirch = 34,
        /// <summary>
        /// Саженец ель
        /// </summary>
        SaplingSpruce = 35,
        /// <summary>
        /// Саженец плодовое
        /// </summary>
        SaplingFruit = 36,
        /// <summary>
        /// Саженец пальмы
        /// </summary>
        SaplingPalm = 37,
        /// <summary>
        /// Кактус
        /// </summary>
        Cactus = 38,
        /// <summary>
        /// Угольная руда
        /// </summary>
        OreCoal = 39,
        /// <summary>
        /// Железная руда
        /// </summary>
        OreIron = 40,
        /// <summary>
        /// Золотая руда
        /// </summary>
        OreGold = 41,
        /// <summary>
        /// Брол
        /// </summary>
        Brol = 42,
        /// <summary>
        /// Блок высокой травы
        /// </summary>
        TallGrass = 43,
        /// <summary>
        /// Цветок одуванчик
        /// </summary>
        FlowerDandelion = 44,
        /// <summary>
        /// Цветок клевер белый
        /// </summary>
        FlowerClover = 45,
        /// <summary>
        /// Стекло прозрачное
        /// </summary>
        Glass = 46,
        /// <summary>
        /// Стекло белое
        /// </summary>
        GlassWhite = 47,
        /// <summary>
        /// Стеклянная панель прозрачная
        /// </summary>
        GlassPane = 48,
        /// <summary>
        /// Стеклянная панель белая
        /// </summary>
        GlassPaneWhite = 49,

        /// <summary>
        /// Отладочный блок
        /// </summary>
        Debug = 50
    }

    /// <summary>
    /// Количество блоков
    /// </summary>
    public class BlocksCount
    {
        public const int COUNT = 50;
    }
}
