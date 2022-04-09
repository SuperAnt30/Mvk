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
        public const int OVERVIEW_CHUNK_START = 5;

        /// <summary>
        /// Время чистки чанков на сервере и клиенте, в тактах
        /// 6000 = 5 min
        /// 400 = 20 sec
        /// </summary>
        public const int CHUNK_CLEANING_TIME = 400; // 6000;

        /// <summary>
        /// Визуальная отладка прогрузки чанков
        /// </summary>
        public const bool IS_DRAW_DEBUG_CHUNK = true;
        //public const bool IS_DRAW_DEBUG_CHUNK = false;
        /// <summary>
        /// Активация имитации задержки локальной сети
        /// </summary>
        public const bool IS_DEBUG_SLEEP_NET = false;
        /// <summary>
        /// Ник по сети с постфиксом Net
        /// </summary>
        public const bool IS_DEBUG_NICKNAME = true;

        /// <summary>
        /// Cколько пакетов чанков передавать по сети за один ТПС
        /// </summary>
        public const int COUNT_PACKET_CHUNK_TPS = 10;

        /// <summary>
        /// Cколько псевдочанков рендерим в один кадр
        /// </summary>
        public const int COUNT_RENDER_CHUNK_FRAME = 16; // было 5 целых чанков, теперь псевдо чанки; потом 50

        /// <summary>
        /// Эффект покачивания при движении
        /// </summary>
        public const bool WIGGLE_EFFECT = true;

        /// <summary>
        /// На каком растоянии от глаз, камера при виде сзади или спереди
        /// </summary>
        public const float CAMERA_DIST = 8f;// 12f;
        /// <summary>
        /// Расстояние для блоков
        /// </summary>
        public const float RAY_CAST_DISTANCE = 6f;
        /// <summary>
        /// Какой радиус для рендера псевдо чанков альфа блоков, при смещении больше 16 блоков
        /// </summary>
        public const int UPDATE_ALPHE_CHUNK = 4;
    }
}
