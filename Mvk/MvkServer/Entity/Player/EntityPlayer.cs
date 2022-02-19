using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

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


        protected EntityPlayer(WorldBase world) : base(world)
        {
            Type = EnumEntities.Player;
            StepHeight = 1.2f;
        }

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 20;

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk, int plusDistSqrt)
        {
            OverviewChunk = overviewChunk;
            DistSqrt = MvkStatic.GetSqrt(overviewChunk + plusDistSqrt);
           // UpProjection(); // Fix Это была вроде проблема когда не загружалась сеть
        }

        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public virtual void UpProjection() { }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

    }
}
