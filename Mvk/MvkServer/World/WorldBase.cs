using MvkServer.World.Chunk;
using System;

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
        /// Список сущностей
        /// </summary>
        //public Hashtable Entities { get; protected set; }

        public WorldBase()
        {
            ChunkPr = new ChunkProvider(this);
        }

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


        

    }
}
