namespace MvkServer
{
    /// <summary>
    /// Объект глобальных констант
    /// </summary>
    public class MvkGlobal
    {
        /// <summary>
        /// При загрузке мира, сколько загружается обзор мира
        /// </summary>
        public const int OVERVIEW_CHUNK_START = 2;

        /// <summary>
        /// Время чистки чанков на сервере и клиенте, в тактах
        /// 6000 = 5 min
        /// </summary>
        public const int CHUNK_CLEANING_TIME = 100; // 6000;

        /// <summary>
        /// Визуальная отладка прогрузки чанков
        /// </summary>
        //public const bool IS_DRAW_DEBUG_CHUNK = true;
        public const bool IS_DRAW_DEBUG_CHUNK = false;

    }
}
