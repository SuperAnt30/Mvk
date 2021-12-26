namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока
    /// </summary>
    public class EntityPlayer : EntityBase
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = MvkGlobal.OVERVIEW_CHUNK_START;

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk) => OverviewChunk = overviewChunk;
    }
}
