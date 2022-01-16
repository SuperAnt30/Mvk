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
        public const int OVERVIEW_CHUNK_START = 12;

        /// <summary>
        /// Время чистки чанков на сервере и клиенте, в тактах
        /// 6000 = 5 min
        /// </summary>
        public const int CHUNK_CLEANING_TIME = 400; // 6000;

        /// <summary>
        /// Визуальная отладка прогрузки чанков
        /// </summary>
        //public const bool IS_DRAW_DEBUG_CHUNK = true;
        public const bool IS_DRAW_DEBUG_CHUNK = false;

        /// <summary>
        /// Cколько пакетов чанков передавать по сети за один ТПС
        /// </summary>
        public const int COUNT_PACKET_CHUNK_TPS = 12;

        /// <summary>
        /// Cколько чанков рендерим в один кадр
        /// </summary>
        public const int COUNT_RENDER_CHUNK_FRAME = 10;


    }
}
