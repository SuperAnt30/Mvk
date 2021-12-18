using MvkServer.Glm;
using System.Collections;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Карта чанков
    /// </summary>
    public class ChunkMap
    {
        protected Hashtable map = new Hashtable();

        /// <summary>
        /// Добавить или изменить чанк
        /// </summary>
        public void Set(ChunkBase chunk)
        {
            if (map.ContainsKey(chunk.Position))
            {
                map[chunk.Position] = chunk;
            }
            else
            {
                map.Add(chunk.Position, chunk);
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public ChunkBase Get(vec2i pos)
        {
            if (map.ContainsKey(pos))
            {
                return map[pos] as ChunkBase;
            }
            return null;
        }

        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(ChunkBase chunk) => map.ContainsKey(chunk.Position);
        /// <summary>
        /// Проверить наличие чанка
        /// </summary>
        public bool Contains(vec2i pos) => map.ContainsKey(pos);

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear() => map.Clear();

        /// <summary>
        /// Удалить
        /// </summary>
        public void Remove(vec2i pos)
        {
            if (map.ContainsKey(pos))
            {
                map.Remove(pos);
            }
        }

        /// <summary>
        /// Получить количество
        /// </summary>
        public int Count => map.Count;
    }
}
