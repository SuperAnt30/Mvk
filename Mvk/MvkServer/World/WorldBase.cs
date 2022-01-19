using MvkServer.Glm;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections;

namespace MvkServer.World
{
    /// <summary>
    /// Базовый объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public CollisionBase Collision { get; protected set; }
        


        protected WorldBase() => Collision = new CollisionBase(this);

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public virtual void Tick()
        {

        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public virtual string ToStringDebug() => "";

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(vec2i pos) => ChunkPr.GetChunk(pos);

        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="pos">глобальная позиция блока</param>
        public BlockBase GetBlock(vec3i pos)
        {
            if (pos.y >= 0 && pos.y <= 255)
            {
                ChunkBase chunk = GetChunk(new vec2i(pos.x >> 4, pos.z >> 4));
                if (chunk != null)
                {
                    return chunk.GetBlock0(new vec3i(pos.x & 15, pos.y, pos.z & 15));
                }
            }
            return Blocks.GetAir(pos);
        }

        


    }
}
