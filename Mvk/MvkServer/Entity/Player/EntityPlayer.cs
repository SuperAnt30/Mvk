using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

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
        public int OverviewChunk { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        public int OverviewChunkPrev { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости
        /// </summary>
        public vec2i[] DistSqrt { get; protected set; }
        /// <summary>
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();

        /// <summary>
        /// Слот выбранного
        /// </summary>
        public int slot = 0;

        protected EntityPlayer(WorldBase world) : base(world)
        {
            Type = EnumEntities.Player;
            StepHeight = 1.2f;
        }

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 20;

        //public override void Update()
        //{
        //    base.Update();
        //}
        //protected override void LivingUpdate()
        //{
        //    base.LivingUpdate();
        //}

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public virtual void SetOverviewChunk(int overviewChunk) => OverviewChunk = overviewChunk;

        /// <summary>
        /// Равны ли обзоры чанков между тактами
        /// </summary>
        public bool SameOverviewChunkPrev() => OverviewChunk == OverviewChunkPrev;

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
