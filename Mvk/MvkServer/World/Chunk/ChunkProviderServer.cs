using MvkServer.Glm;
using MvkServer.Util;
using System.Collections;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Объект сервер который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderServer : ChunkProvider
    {
        /// <summary>
        /// Список чанков которые надо выгрузить
        /// </summary>
        public MapList DroppedChunks { get; protected set; } = new MapList();

        public ChunkProviderServer(WorldServer worldIn) => world = worldIn;

        /// <summary>
        /// Загрузить чанк
        /// </summary>
        public ChunkBase LoadChunk(vec2i pos) => GetStatusChunk(pos, 4);

        /// <summary>
        /// Получить чанк по статусу, если статуса не хватает, догружаем рядом лежащие пока не получим нужный статус
        /// </summary>
        protected ChunkBase GetStatusChunk(vec2i pos, int status)
        {
            DroppedChunks.Remove(pos);
            ChunkBase chunk = GetChunk(pos);
            if (chunk == null)
            {
                chunk = new ChunkBase(world, pos);
                chunk.ChunkLoadGen();
                chunkMapping.Set(chunk);
            }
            //TODO :: оптимизировать надо!!! чтоб проходы были быстрее
            if (chunk.DoneStatus < status)
            {
                for (int i = 0; i < 8; i++)
                {
                    GetStatusChunk(pos + ArrayStatic.areaOne8[i], status - 1);
                }
                //System.Threading.Thread.Sleep(1);
                chunk.DoneStatus = status;
            }
            return chunk;
        }

        /// <summary>
        /// Выгрузка ненужных чанков Для сервера
        /// </summary>
        public void UnloadQueuedChunks()
        {
            int i = 0;
            while (DroppedChunks.Count > 0 && i < 100) // 100
            {
                vec2i pos = DroppedChunks.FirstRemove();
                ChunkBase chunk = chunkMapping.Get(pos);
                if (chunk != null)
                {
                    chunk.ChunkUnload();
                    // TODO::Тут сохраняем чанк
                    chunkMapping.Remove(pos);
                    i++;
                }
            }
        }

        /// <summary>
        /// Добавить в список удаляющих чанков которые не полного статуса
        /// </summary>
        public void DroopedChunkStatusMin(Hashtable playersClone) => chunkMapping.DroopedChunkStatusMin(DroppedChunks, playersClone);
    }
}
