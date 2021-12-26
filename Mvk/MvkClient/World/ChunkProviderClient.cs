using MvkClient.Renderer.Chunk;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.World.Chunk;
using System.Collections;

namespace MvkClient.World
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        public ChunkProviderClient(WorldClient worldIn) => world = worldIn;

        /// <summary>
        /// Очистить все чанки, ТОЛЬКО для клиента
        /// </summary>
        public override void ClearAllChunks() => chunkMapping.Clear();

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        public override void RemoveChunk(vec2i pos) => chunkMapping.Remove(pos);

        /// <summary>
        /// Загрузить, если нет такого создаём для клиента
        /// </summary>
        public ChunkRender GetChunkRender(vec2i pos, bool isCreate)
        {
            if (!(chunkMapping.Get(pos) is ChunkRender chunk))
            {
                chunk = new ChunkRender((WorldClient)world, pos);
                chunkMapping.Set(chunk);
                return chunk;
            }
            return chunk;
        }

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// для клиента
        /// </summary>
        public void FixOverviewChunk(EntityPlayer entity)
        {
            // + 2 к обзору для кэша из-за клона обработки, разных потоков
            vec2i min = entity.HitBox.ChunkPos - (entity.OverviewChunk + 2);
            vec2i max = entity.HitBox.ChunkPos + (entity.OverviewChunk + 2);

            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkBase chunk in ht.Values)
            {
                if (chunk.Position.x < min.x || chunk.Position.x > max.x || chunk.Position.y < min.y || chunk.Position.y > max.y)
                {
                    chunkMapping.Remove(chunk.Position);
                }
            }
        }
    }
}
