using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
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
            chunk.UpdateTime();
            Hashtable mapThreadSafe = Hashtable.Synchronized(map);
            if (mapThreadSafe.ContainsKey(chunk.Position))
            {
                mapThreadSafe[chunk.Position] = chunk;
            }
            else
            {
                mapThreadSafe.Add(chunk.Position, chunk);
            }
        }

        /// <summary>
        /// Получить чанк с массива
        /// </summary>
        public ChunkBase Get(vec2i pos)
        {
            Hashtable mapThreadSafe = Hashtable.Synchronized(map);
            if (mapThreadSafe.ContainsKey(pos))
            {
                return mapThreadSafe[pos] as ChunkBase;
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
            Hashtable mapThreadSafe = Hashtable.Synchronized(map);
            if (mapThreadSafe.ContainsKey(pos))
            {
                mapThreadSafe.Remove(pos);
            }
        }

        /// <summary>
        /// Добавить в список мусор удаляющих чанков для сервера!
        /// </summary>
        public void DroopedChunkStatusMin(MapList droppedChunks, Hashtable playersClone)
        {
            Hashtable ht = map.Clone() as Hashtable;
            foreach (ChunkBase chunk in ht.Values)
            {
                if (chunk.DoneStatus < 4 && chunk.IsOldTime())
                {
                    droppedChunks.Add(chunk.Position);
                }
                else
                {
                    bool b = false;
                    foreach (EntityPlayerServer player in playersClone.Values)
                    {
                        int radius = player.OverviewChunk + 1;
                        vec2i min = player.HitBox.ChunkPosManaged - radius;
                        vec2i max = player.HitBox.ChunkPosManaged + radius;
                        if (chunk.Position.x >= min.x && chunk.Position.x <= max.x && chunk.Position.y >= min.y && chunk.Position.y <= max.y)
                        {
                            b = true;
                            break;
                        }
                    }
                    if (!b)
                    {
                        droppedChunks.Add(chunk.Position);
                    }
                }
            }
        }

        /// <summary>
        /// Получить количество
        /// </summary>
        public int Count => map.Count;

        /// <summary>
        /// Получить клон карты
        /// </summary>
        public Hashtable CloneMap() => map.Clone() as Hashtable;
    }
}
