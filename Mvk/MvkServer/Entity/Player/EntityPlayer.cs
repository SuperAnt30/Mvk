using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока
    /// </summary>
    public abstract class EntityPlayer : EntityLivingHead
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Порядковый номер игрока на сервере, с момента запуска сервера
        /// </summary>
        public ushort Id { get; protected set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости
        /// </summary>
        public vec2i[] DistSqrt { get; protected set; }
        /// <summary>
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();


        protected EntityPlayer()
        {
            Standing();
            SpeedSurvival();
            StepHeight = 1.2f;
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk, int plusDistSqrt)
        {
            OverviewChunk = overviewChunk;
            DistSqrt = MvkStatic.GetSqrt(overviewChunk + plusDistSqrt);
        }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

        /// <summary>
        /// Проверка смещения чанка на выбранное положение
        /// </summary>
        public bool CheckPosManaged(int bias)
        {
            vec2i chunk = GetChunkPos();
            return Mth.Abs(chunk.x - ChunkPosManaged.x) >= bias || Mth.Abs(chunk.y - ChunkPosManaged.y) >= bias;
        }
            
    }
}
