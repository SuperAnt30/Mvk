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
        /// Получить чанк по координатам чанка
        /// </summary>
        //public override ChunkBase GetChunk(vec2i pos)
        //{
        //    //2022-04-04 эмитация не загруженного чанка
        //    if (pos.x == 4 && pos.y == 0) return null;
        //    return base.GetChunk(pos);
        //}

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
                    int stolb = 0;
                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            float h = heightNoise[count] / 132f;
                            int y0 = (int)(-h * 64f) + 32;
                            if (x == 7 && z == 7) stolb = y0;
                            chunk.SetEBlock(new vec3i(x, 0, z), Block.EnumBlock.Stone);
                            //if (y0 > 0)
                            {
                                bool stop = false;
                                for (int y = 1; y < 256; y++)
                                {
                                    if (y < y0)
                                    {
                                        chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Dirt);
                                    }
                                    else
                                    {
                                        stop = true;
                                        if (y <= 16) chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Water);
                                        else chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Turf);
                                        //break;
                                    }
                                    if (y >= 16 && stop)
                                    {
                                        break;
                                    }
                                }
                            }
                            count++;
                        }
                    }

                    if (pos.x == -1 && pos.y == -1)
                    {
                        for (int y = 1; y <= 16; y++)
                        {
                            //chunk.SetEBlock(new vec3i(7, y + stolb, 7), Block.EnumBlock.Dirt);
                            chunk.SetEBlock(new vec3i(7, y + stolb, 5), Block.EnumBlock.Glass);
                            chunk.SetEBlock(new vec3i(7, y + stolb, 9), Block.EnumBlock.GlassRed);
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
