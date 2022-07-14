using MvkServer.Glm;
using MvkServer.Util;
using System;
using System.Collections;
using System.Collections.Generic;

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
        /// Два режима работы: если передано true, сохранить все чанки за один раз. Если передано false, сохраните до двух фрагментов.
        /// </summary>
        /// <returns>Возвращает true, если все фрагменты были сохранены</returns>
        public bool SaveChunks(bool all)
        {
            int count = 0;
            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkBase chunk in ht.Values)
            {
                if (all)
                {
                    SaveChunkExtraData(chunk);
                }
                if (chunk.NeedsSaving())
                {
                    SaveChunkData(chunk);
                    chunk.SavedNotModified();
                    count++;
                    if (count == 24 && !all) return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Сохранить чанк
        /// </summary>
        private void SaveChunkData(ChunkBase chunk)
        {
            //if (chunkLoader != null)
            //{
            //    try
            //    {
            //        chunk.SetLastSaveTime(world.GetTotalWorldTime());
            //        chunkLoader.SaveChunk(world, chunk);
            //    }
            //    catch (Exception ex)
            //    {
            //        worldServer.Log.Error("Не удалось сохранить чанк {0}\r\n{1}", ex.Message, ex.StackTrace);
            //    }
            //}
        }

        /// <summary>
        /// Сохранить дополнительные данные чанка
        /// </summary>
        private void SaveChunkExtraData(ChunkBase chunk)
        {
            //if (chunkLoader != null)
            //{
            //    try
            //    {
            //        chunkLoader.SaveExtraChunkData(world, chunk);
            //    }
            //    catch (Exception ex)
            //    {
            //        worldServer.Log.Error("Не удалось сохранить чанк {0}\r\n{1}", ex.Message, ex.StackTrace);
            //    }
            //}
        }

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
                    float[] grassNoise = new float[256];
                    float scale = 0.2f;
                    worldServer.Noise.HeightBiome.GenerateNoise2d(heightNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
                    worldServer.Noise.WetnessBiome.GenerateNoise2d(wetnessNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);
                    scale = .5f;
                    worldServer.Noise.Down.GenerateNoise2d(grassNoise, chunk.Position.x * 16, chunk.Position.y * 16, 16, 16, scale, scale);

                    int count = 0;
                    int stolb = 0;
                    int yMax = 0;
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
                                        else
                                        {
                                            chunk.SetEBlock(new vec3i(x, y, z), Block.EnumBlock.Turf, chunk.World.Rand.Next(0, 4));
                                            if (grassNoise[count] > .71f)
                                            {
                                              //  chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.Brol);
                                            }
                                            else if (grassNoise[count] > .1f)
                                            {
                                                chunk.SetEBlock(new vec3i(x, y + 1, z), Block.EnumBlock.TallGrass, chunk.World.Rand.Next(0, 5));
                                            }
                                        }
                                        if (y > yMax) yMax = y;
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
                            chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
                            chunk.SetEBlock(new vec3i(7, y + stolb, 5), Block.EnumBlock.Glass);
                           // chunk.SetEBlock(new vec3i(7, y + stolb, 9), Block.EnumBlock.GlassWhite);
                            chunk.SetEBlock(new vec3i(11, y + stolb, 7), Block.EnumBlock.Lava);
                            chunk.SetEBlock(new vec3i(12, y + stolb, 7), Block.EnumBlock.LogOak);
                        }
                        for (int y = 16; y <= 216; y++) // 196 еррор на 5 мире
                        {
                            chunk.SetEBlock(new vec3i(3, y + stolb, 7), Block.EnumBlock.Dirt);
                        }
                        //chunk.SetEBlock(new vec3i(5, 2 + stolb, 5), Block.EnumBlock.Brol);
                        // TODO::2022-06-30 надо уметь осветить блоки при генерации
                    }

                    if (pos.x == -3 && pos.y == 0)
                    {
                        //for (int x = 1; x <= 5; x++)
                        //    for (int y = 3; y <= 8; y++)
                        //    {
                        //        chunk.SetEBlock(new vec3i(x, stolb + y, 5), Block.EnumBlock.Dirt);
                        //    }
                        for (int x = 1; x <= 3; x++) for (int y = 6; y <= 8; y++) for (int z = 1; z <= 3; z++)
                        {
                            chunk.SetEBlock(new vec3i(x, stolb + y, z), Block.EnumBlock.Dirt);
                        }

                        //for (int x = 0; x < 16; x++)
                        //{
                        //    for (int z = 0; z < 16; z++)
                        //    {
                        //        int y = 255;
                        //        while(y > 0)
                        //        {
                        //            if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
                        //            {
                        //                int yc = y >> 4;
                        //                int yb = y & 15;
                        //                chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
                        //                //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 15);
                        //                y--;
                        //            }
                        //            else
                        //            {
                        //                y = -1;
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    //if (pos.x == -3 && pos.y == 1)
                    //{
                    //    for (int x = 0; x < 16; x++)
                    //    {
                    //        for (int z = 0; z < 16; z++)
                    //        {
                    //            int y = 255;
                    //            while (y > 0)
                    //            {
                    //                if (chunk.GetEBlock(x, y, z) == Block.EnumBlock.Air)
                    //                {
                    //                    int yc = y >> 4;
                    //                    int yb = y & 15;
                    //                    //chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Sky, 15);
                    //                    chunk.StorageArrays[yc].SetLightFor(x, yb, z, Block.EnumSkyBlock.Block, 10);
                    //                    y--;
                    //                }
                    //                else
                    //                {
                    //                    y = -1;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    Cave(chunk, yMax >> 4);

                }

                List<vec3i> list = new List<vec3i>();

                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = 0; y <= ChunkBase.COUNT_HEIGHT_BLOCK; y++)
                        {
                            if (chunk.GetBlockState(x, y, z).GetBlock().LightValue > 0)
                            {
                                list.Add(new vec3i(x, y, z));
                            }
                        }
                    }
                }

                chunk.Light.SetLightBlocks(list.ToArray());

                chunkMapping.Set(chunk);
                chunk.Light.GenerateHeightMapSky();
                chunk.OnChunkLoad();
            }
            
            return chunk;
        }

        /// <summary>
        /// Генерация пещер
        /// </summary>
        private void Cave(ChunkBase chunk, int yMax)
        {
            int count = 0;
            float[] noise = new float[4096];
            for (int y0 = 0; y0 <= yMax; y0++)
            {
                worldServer.Noise.Cave.GenerateNoise3d(noise, chunk.Position.x * 16, y0 * 16, chunk.Position.y * 16, 16, 16, 16, .05f, .05f, .05f);
                count = 0;
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            if (noise[count] < -1f)
                            {
                                vec3i pos = new vec3i(x, y0 << 4 | y, z);
                                int y2 = y0 << 4 | y;
                                Block.EnumBlock enumBlock = chunk.GetBlockState(x, y2, z).GetBlock().EBlock;
                                if (enumBlock != Block.EnumBlock.Air && enumBlock != Block.EnumBlock.Stone
                                    && enumBlock != Block.EnumBlock.Water)
                                {
                                    chunk.SetEBlock(pos, Block.EnumBlock.Air);
                                }
                            }
                            count++;
                        }
                    }
                }
            }
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
