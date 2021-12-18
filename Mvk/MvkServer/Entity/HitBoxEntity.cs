using MvkServer.Glm;

namespace MvkServer.Entity
{
    /// <summary>
    /// Нитбокс сущности
    /// </summary>
    public class HitBoxEntity
    {
        /// <summary>
        /// Положение камеры
        /// </summary>
        public vec3 Position { get; protected set; }
        /// <summary>
        /// В каком чанке находится
        /// </summary>
        public vec2i ChunkPos { get; protected set; } = new vec2i();
        /// <summary>
        /// Позиция псевдо чанка
        /// </summary>
        public int ChunkY { get; protected set; }
        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i BlockPos { get; protected set; } = new vec3i();
        /// <summary>
        /// На каком стоим блоке
        /// </summary>
        public vec3i BlockPosDown { get; protected set; } = new vec3i();

        /// <summary>
        /// Задать позицию
        /// </summary>
        public void SetPos(vec3 pos)
        {
            if (!Position.Equals(pos))
            {
                Position = pos;
                BlockPos = new vec3i(Position);
                BlockPosDown = new vec3i(new vec3(pos.x, pos.y - 1, pos.z));
                ChunkPos = new vec2i((BlockPos.x) >> 4, (BlockPos.z) >> 4);
                ChunkY = (BlockPos.y) >> 4;
            }
        }

    }
}
