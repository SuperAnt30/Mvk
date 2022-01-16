using MvkClient.Renderer.Chunk;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.World.Chunk;
using System.Collections;
using System.Collections.Generic;

namespace MvkClient.World
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        /// <summary>
        /// Список чанков Для удаления сетки основного потока
        /// </summary>
        public List<ChunkRender> RemoteMeshChunks { get; protected set; } = new List<ChunkRender>();

        public ChunkProviderClient(WorldClient worldIn) => world = worldIn;

        /// <summary>
        /// Очистить все чанки, ТОЛЬКО для клиента
        /// </summary>
        public void ClearAllChunks()
        {
            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkRender chunk in ht.Values)
            {
                UnloadChunk(chunk);
            }
        }

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        //public override void RemoveChunk(vec2i pos) => chunkMapping.Remove(pos);

        /// <summary>
        /// Загрузить, если нет такого создаём для клиента
        /// </summary>
        public ChunkRender GetChunkRender(vec2i pos, bool isCreate)
        {
            if (!(chunkMapping.Get(pos) is ChunkRender chunk))
            {
                if (isCreate)
                {
                    chunk = new ChunkRender((WorldClient)world, pos);
                    chunkMapping.Set(chunk);
                    return chunk;
                }
                return null;
            }
            return chunk;
        }

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(vec2i pos) => UnloadChunk((ChunkRender)GetChunk(pos));
        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkRender chunk)
        {
            if (chunk != null)
            {
                if (chunk.IsChunkLoaded)
                {
                    chunk.ChunkUnload();
                    // заносим в массив чистки чанков по сетки для основного потока
                    RemoteMeshChunks.Add(chunk);
                }
                // TODO:: Из-за этого тормозит у меня дома!!! Комп разгрузил и вроде лучше
                chunkMapping.Remove(chunk.Position);
            }
        }

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// для клиента
        /// </summary>
        public void FixOverviewChunk(EntityPlayer entity)
        {
            // дополнительно к обзору для кэша из-за клона обработки, разных потоков
            int additional = 6;
            vec2i chunkCoor = entity.GetChunkPos();
            vec2i min = chunkCoor - (entity.OverviewChunk + additional);
            vec2i max = chunkCoor + (entity.OverviewChunk + additional);

            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkRender chunk in ht.Values)
            {
                if (chunk.Position.x < min.x || chunk.Position.x > max.x || chunk.Position.y < min.y || chunk.Position.y > max.y)
                {
                    UnloadChunk(chunk);
                }
            }
        }
    }
}
