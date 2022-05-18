//using MvkServer.Util;
//using MvkServer.World.Block;
//using MvkServer.World.Chunk;

//namespace MvkServer.World.Light
//{
//    public class LightChunk
//    {
//        public ChunkBase Chunk { get; private set; }
//        public WorldBase World { get; private set; }

//        /// <summary>
//        /// Карта высот по чанку, XZ
//        /// </summary>
//        private int[,] heightMap = new int[16, 16];
//        /// <summary>
//        /// Наименьшее значение на карте высот
//        /// </summary>
//        //private int heightMapMinimum;

//        /// <summary>
//        /// Карта, похожая на heightMap, которая отслеживает, как далеко могут падать осадки.
//        /// </summary>
//      //  private int[,] precipitationHeightMap = new int[16, 16];


//        public LightChunk(ChunkBase chunk)
//        {
//            Chunk = chunk;
//            World = chunk.World;
//        }

//        /// <summary>
//        /// Создает исходную карту просвета для чанка при генерации или загрузке.
//        /// </summary>
//        public void GenerateSkylightMap()
//        {
//            int sy = Chunk.GetTopFilledSegment();
//            //heightMapMinimum = 999;

//            for (int x = 0; x < 16; ++x)
//            {
//                int z = 0;

//                while (z < 16)
//                {
//                    //precipitationHeightMap[x, z] = -999;
//                    int y = sy + 16;

//                    while (true)
//                    {
//                        if (y > 0)
//                        {
//                            if (GetBlockLightOpacity(x, y - 1, z) == 0)
//                            {
//                                // если небо или подобное
//                                y--;
//                                continue;
//                            }

//                            heightMap[x, z] = y;

//                            //if (y < heightMapMinimum) heightMapMinimum = y;
//                        }

//                        if (!World.HasNoSky)
//                        {
//                            // Если небо в мире есть, пробегаем в тикущем псевдо чанке сверху до блока где есть освещения от неба
//                            y = 15;
//                            int y0 = sy + 16 - 1;

//                            do
//                            {
//                                int opacity = GetBlockLightOpacity(x, y0, z);
//                                if (opacity == 0 && y != 15) opacity = 1;
//                                y -= opacity;

//                                if (y > 0)
//                                {
//                                    if (!Chunk.StorageArrays[y0 >> 4].IsEmpty())
//                                    {
//                                        Chunk.StorageArrays[y0 >> 4].SetLightFor(
//                                            x, y0 & 15, z, EnumSkyBlock.Sky, (byte)y);
//                                           World.MarkBlockForUpdate(new BlockPos((Chunk.Position.x << 4) + x, y0, (Chunk.Position.y << 4) + z));
//                                    }
//                                }
//                                y0--;
//                            }
//                            while (y0 > 0 && y > 0);
//                        }
//                        z++;
//                        break;
//                    }
//                }
//            }
//            Chunk.Modified();
//        }

//        /// <summary>
//        /// Получить сколько света проходит через блок
//        /// </summary>
//        private int GetBlockLightOpacity(int x, int y, int z)
//        {
//            return Chunk.GetBlockState(x, y, z).GetBlock().LightOpacity;
//            //EnumBlock enumBlock = Chunk.GetEBlock(x, y, z);
//            //BlockBase block = Blocks.GetBlockCache(enumBlock);
//            //return block.LightOpacity;
//        }
//    }
//}
