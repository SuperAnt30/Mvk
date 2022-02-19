using MvkServer.Entity;
using MvkServer.Glm;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш чанков
    /// </summary>
    public abstract class ChunkProvider
    {
        /// <summary>
        /// Список чанков
        /// </summary>
        protected ChunkMap chunkMapping = new ChunkMap();
        
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        protected WorldBase world;

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        public virtual void RemoveChunk(vec2i pos) { }

        /// <summary>
        /// Проверить наличие чанка в массиве
        /// </summary>
        public bool IsChunk(vec2i pos)
        {
            if (chunkMapping.Contains(pos))
            {
                ChunkBase chunk = GetChunk(pos);
                return chunk != null && chunk.IsChunkLoaded;
            }
            return false;
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(vec2i pos) => chunkMapping.Get(pos);

        /// <summary>
        /// Количество чанков в кэше
        /// </summary>
        public int Count => chunkMapping.Count;

        /// <summary>
        /// Список чанков для отладки
        /// </summary>
        [Obsolete("Список чанков только для отладки")]
        public List<vec3i> GetListDebug()
        {
            Hashtable ht = chunkMapping.CloneMap();
            List<vec3i> list = new List<vec3i>();
            foreach (ChunkBase chunk in ht.Values)
            {
                list.Add(new vec3i(chunk.Position.x, chunk.DoneStatus, chunk.Position.y));
            }
            return list;
        }
        /// <summary>
        /// Список чанков где сущность для отладки
        /// </summary>
        [Obsolete("Список чанков где сущность только для отладки")]
        public List<vec3i> GetListEntityDebug()
        {
            Hashtable ht = chunkMapping.CloneMap();
            List<vec3i> list = new List<vec3i>();
            foreach (ChunkBase chunk in ht.Values)
            {
                if (chunk.CountEntity() > 0) // Для дебага сущностей в чанке
                    list.Add(new vec3i(chunk.Position.x, 7, chunk.Position.y));
            }
            return list;
        }

        [Obsolete("Список сущностей в мире в чанках только для отладки")]
        public int GetCountEntityDebug()
        {
            Hashtable ht = chunkMapping.CloneMap();
            List<EntityLiving> list = new List<EntityLiving>();

            foreach (ChunkBase chunk in ht.Values)
            {
                list.AddRange(chunk.GetEntities());
            }
            return list.Count;
        }
    }
}
