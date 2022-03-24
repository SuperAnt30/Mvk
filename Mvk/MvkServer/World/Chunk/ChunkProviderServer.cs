using MvkServer.Glm;
using MvkServer.Util;

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
        public MapListVec2i DroppedChunks { get; protected set; } = new MapListVec2i();

        /// <summary>
        /// Сылка на объект серверного мира
        /// </summary>
        private WorldServer worldServer;

        public ChunkProviderServer(WorldServer worldIn) => world = worldServer = worldIn;

        /// <summary>
        /// Загрузить чанк
        /// </summary>
        public ChunkBase LoadChunk(vec2i pos)
        {
            DroppedChunks.Remove(pos);
            ChunkBase chunk = GetChunk(pos);

            if (chunk == null)
            {
                // Загружаем
                // chunk = LoadChunkFromFile(pos);
                if (chunk == null)
                {
                    // чанка нет после загрузки, значит надо генерировать

                    // это пока временно
                    chunk = new ChunkBase(world, pos);
                    //chunk.ChunkLoadGen();

                    float[] heightNoise = new float[256];
                    float[] wetnessNoise = new float[256];
                    float scale = 0.2f;
                    worldServer.Noise.HeightBiome.GenerateNoise2d(heightNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
                    worldServer.Noise.WetnessBiome.GenerateNoise2d(wetnessNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);

                    int count = 0;
                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            float h = heightNoise[count] / 132f;
                            int y0 = (int)(-h * 64f) + 32;
                            chunk.SetEBlock(new vec3i(x, 0, z), Block.EnumBlock.Stone);
                            if (y0 > 0)
                            {
                                for (int y = 1; y < 256; y++)
                                {
                                    if (y < y0)
                                    {
                                        chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Dirt);
                                    }
                                    else
                                    {
                                        chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Turf);
                                        break;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                }

                chunkMapping.Set(chunk);
                chunk.OnChunkLoad();
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
                    chunk.OnChunkUnload();
                    // TODO::Тут сохраняем чанк
                    chunkMapping.Remove(pos);
                    i++;
                }
            }
        }

        /// <summary>
        /// Добавить чанк на удаление
        /// </summary>
        public void DropChunk(vec2i pos) => DroppedChunks.Add(pos);

    }
}
