using MvkServer.Glm;
using MvkServer.Util;

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
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();

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
        /// <summary>
        /// Задать новое положение чанка, после обработки
        /// </summary>
        public void UpChunkPosManaged() => ChunkPosManaged = ChunkPos;

        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

        /// <summary>
        /// Проверка смещения чанка на выбранное положение
        /// </summary>
        public bool CheckPosManaged(int bias) 
            => Mth.Abs(ChunkPos.x - ChunkPosManaged.x) >= bias || Mth.Abs(ChunkPos.y - ChunkPosManaged.y) >= bias;
    }
}
