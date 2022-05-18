//using MvkServer.Glm;
//using MvkServer.Util;
//using MvkServer.World.Block;
//using MvkServer.World.Chunk;

//namespace MvkServer.World.Light
//{
//    /// <summary>
//    /// Объект который принадлежит WorldBase
//    /// </summary>
//    public class WorldLight
//    {
//        /// <summary>
//        /// Объект базового мира
//        /// </summary>
//        public WorldBase World { get; private set; }

//        /// <summary>
//        /// Это временный список блоков и значений освещения, используемых при обновлении уровней освещения. 
//        /// Вмещает до 32x32x32 блоков (максимальное влияние источника света). 
//        /// Каждый элемент представляет собой упакованное битовое значение: 0000000000LLLLzzzzzzyyyyyyxxxxxx. 
//        /// 4-битный L — это уровень освещенности, используемый при затемнении блоков. 
//        /// 6-битные числа x, y и z представляют смещение блока относительно исходного блока плюс 32 
//        /// (т. е. значение 31 будет означать смещение -1).
//        /// </summary>
//        private int[] lightUpdateBlockList = new int[32768];

//        public WorldLight(WorldBase world) => World = world;


//        public bool CheckLightFor(EnumSkyBlock type, BlockPos blockPos)
//        {
//            if (!World.IsAreaLoaded(blockPos, 17)) return false;

//            World.TheProfiler().StartSection("getBrightness");

//            int var3 = 0;
//            int var4 = 0;
//            int var5 = GetLightFor(type, blockPos);
//            int var6 = func_175638_a(blockPos, type);
//            int x0 = blockPos.X;
//            int y0 = blockPos.Y;
//            int z0 = blockPos.Z;
//            int lightBup;
//            int x;
//            int y;
//            int z;
//            int light;
//            int x2;
//            int y2;
//            int z2;

//            if (var6 > var5)
//            {
//                lightUpdateBlockList[var4++] = 133152;
//            }
//            else if (var6 < var5)
//            {
//                lightUpdateBlockList[var4++] = 133152 | var5 << 18;

//                while (var3 < var4)
//                {
//                    lightBup = lightUpdateBlockList[var3++];
//                    x = (lightBup & 63) - 32 + x0;
//                    y = (lightBup >> 6 & 63) - 32 + y0;
//                    z = (lightBup >> 12 & 63) - 32 + z0;
//                    int light2 = lightBup >> 18 & 15;
//                    BlockPos blockPos2 = new BlockPos(x, y, z);
//                    light = GetLightFor(type, blockPos2);

//                    if (light == light2)
//                    {
//                        SetLightFor(type, blockPos2, 0);

//                        if (light2 > 0)
//                        {
//                            x2 = Mth.Abs(x - x0);
//                            y2 = Mth.Abs(y - y0);
//                            z2 = Mth.Abs(z - z0);

//                            if (x2 + y2 + z2 < 17)
//                            {
//                                for (int var22 = 0; var22 < 6; ++var22)
//                                {
//                                    vec3i vec = EnumFacing.DirectionVec((Pole)var22);
//                                    int var24 = x + vec.x;
//                                    int var25 = y + vec.y;
//                                    int var26 = z + vec.z;
//                                    BlockPos var27 = new BlockPos(var24, var25, var26);
//                                    BlockBase block = World.GetBlockCache(var27);
//                                    int var28 = Mth.Max((byte)1, block.LightOpacity);
//                                    light = GetLightFor(type, var27);

//                                    if (light == light2 - var28 && var4 < lightUpdateBlockList.Length)
//                                    {
//                                        lightUpdateBlockList[var4++] = var24 - x0 + 32 | var25 - y0 + 32 << 6 | var26 - z0 + 32 << 12 | light2 - var28 << 18;
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }

//                var3 = 0;
//            }

//            World.TheProfiler().EndStartSection("checkedPosition < toCheckCount");

//            while (var3 < var4)
//            {
//                lightBup = lightUpdateBlockList[var3++];
//                x = (lightBup & 63) - 32 + x0;
//                y = (lightBup >> 6 & 63) - 32 + y0;
//                z = (lightBup >> 12 & 63) - 32 + z0;
//                BlockPos var29 = new BlockPos(x, y, z);
//                int var30 = GetLightFor(type, var29);
//                light = func_175638_a(var29, type);

//                if (light != var30)
//                {
//                    SetLightFor(type, var29, light);

//                    if (light > var30)
//                    {
//                        x2 = Mth.Abs(x - x0);
//                        y2 = Mth.Abs(y - y0);
//                        z2 = Mth.Abs(z - z0);
//                        bool var31 = var4 < lightUpdateBlockList.Length - 6;

//                        if (x2 + y2 + z2 < 17 && var31)
//                        {
//                            if (GetLightFor(type, var29.OffsetWest()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x - 1 - x0 + 32 + (y - y0 + 32 << 6) + (z - z0 + 32 << 12);
//                            }

//                            if (GetLightFor(type, var29.OffsetEast()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x + 1 - x0 + 32 + (y - y0 + 32 << 6) + (z - z0 + 32 << 12);
//                            }

//                            if (GetLightFor(type, var29.OffsetDown()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x - x0 + 32 + (y - 1 - y0 + 32 << 6) + (z - z0 + 32 << 12);
//                            }

//                            if (GetLightFor(type, var29.OffsetUp()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x - x0 + 32 + (y + 1 - y0 + 32 << 6) + (z - z0 + 32 << 12);
//                            }

//                            if (GetLightFor(type, var29.OffsetNorth()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x - x0 + 32 + (y - y0 + 32 << 6) + (z - 1 - z0 + 32 << 12);
//                            }

//                            if (GetLightFor(type, var29.OffsetSouth()) < light)
//                            {
//                                lightUpdateBlockList[var4++] = x - x0 + 32 + (y - y0 + 32 << 6) + (z + 1 - z0 + 32 << 12);
//                            }
//                        }
//                    }
//                }
//            }

//            World.TheProfiler().EndSection();
//            return true;
//        }

//        /// <summary>
//        /// Получить уровень яркости тикущего блока, глобальные  координаты
//        /// </summary>
//        /// <param name="type">тип света</param>
//        /// <param name="pos">позиция блока</param>
//        private int GetLightFor(EnumSkyBlock type, BlockPos pos)
//        {
//            if (pos.Y < 0) pos = new BlockPos(pos.X, 0, pos.Z);
//            if (!World.IsValid(pos)) return (int)type;

//            ChunkBase chunk = World.GetChunk(pos);
//            int sy = pos.GetPositionChunkY();
//            if (chunk == null || chunk.StorageArrays[sy].IsEmpty()) return (byte)type;
//            return chunk.StorageArrays[sy].GetLightFor(pos.X & 15, pos.Y & 15, pos.Z & 15, type);
//        }

//        /// <summary>
//        /// Задать уровень яркости тикущего блока
//        /// </summary>
//        /// <param name="type">тип света</param>
//        /// <param name="pos">позиция блока</param>
//        /// <param name="lightValue">яркость 0-15</param>
//        private void SetLightFor(EnumSkyBlock type, BlockPos pos, int lightValue)
//        {
//            if (World.IsValid(pos))
//            {
//                ChunkBase chunk = World.GetChunk(pos);
//                if (chunk != null)
//                {
//                    chunk.StorageArrays[pos.GetPositionChunkY()].SetLightFor(pos.X & 15, pos.Y & 15, pos.Z & 15, type, (byte)lightValue);
//                    NotifyLightSet(pos);
//                }
//            }
//        }

//        /// <summary>
//        /// Может ли видеть небо
//        /// </summary>
//        private bool IsAgainstSky(BlockPos pos) => World.GetChunk(pos.GetPositionChunk()).Light.CanSeeSky(pos);

//        private int func_175638_a(BlockPos p_175638_1_, EnumSkyBlock p_175638_2_)
//        {
//            if (p_175638_2_ == EnumSkyBlock.Sky && IsAgainstSky(p_175638_1_)) return 15;
//            BlockBase var3 = World.GetBlockCache(p_175638_1_);// this.getBlockState(p_175638_1_).getBlock();
//            int var4 = p_175638_2_ == EnumSkyBlock.Sky ? 0 : var3.LightValue;
//            int var5 = var3.LightOpacity;

//            if (var5 >= 15 && var3.LightValue > 0) var5 = 1;
//            if (var5 < 1) var5 = 1;

//            if (var5 >= 15) return 0;
//            if (var4 >= 14) return var4;

//            for (int var8 = 0; var8 < 6; ++var8)
//            {
//                BlockPos var10 = p_175638_1_.Offset((Pole)var8);
//                int var11 = GetLightFor(p_175638_2_, var10) - var5;

//                if (var11 > var4) var4 = var11;
//                if (var4 >= 14) return var4;
//            }
//            return var4;
//        }

//        public void NotifyLightSet(BlockPos blockPos)
//        {
//            // TODO:: Light возможно надо шыре на один блок делать
//            //World.MarkBlockForUpdate(pos);

//            int x0 = blockPos.X;
//            int y0 = blockPos.Y;
//            int z0 = blockPos.Z;

//            for (int x = x0 - 1; x <= x0 + 1; x++)
//            {
//                for (int y = y0 - 1; y <= y0 + 1; y++)
//                {
//                    for (int z = z0 - 1; z <= z0 + 1; z++)
//                    {
//                        World.MarkBlockForUpdate(new BlockPos(x, y, z));
//                    }
//                }
//            }


//            //int x = pos.X;
//            //int y = pos.Y;
//            //int z = pos.Z;
//            ////World.MarkBlocksForUpdate(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
//            ///
//            //World.MarkBlockRangeForRenderUpdate(x - 1, y - 1, z - 1, x + 1, y + 1, z + 1);
//        }

//        /// <summary>
//        /// Помечает вертикальную линию блоков как грязную
//        /// </summary>
//        public void MarkBlocksDirtyVertical(int x1, int z1, int y1, int y2)
//        {
//            int y;
//            if (y1 > y2)
//            {
//                y = y2;
//                y2 = y1;
//                y1 = y;
//            }

//            if (!World.HasNoSky)
//            {
//                for (y = y1; y <= y2; ++y)
//                {
//                    CheckLightFor(EnumSkyBlock.Sky, new BlockPos(x1, y, z1));
//                }
//            }
//            World.MarkBlockRangeForRenderUpdate(x1, y1, z1, x1, y2, z1);
//        }

//        /// <summary>
//        /// Проверка освещения
//        /// </summary>
//        public bool CheckLight(BlockPos blockPos)
//        {
//            bool var2 = false;

//            if (!World.HasNoSky)
//            {
//                var2 |= CheckLightFor(EnumSkyBlock.Sky, blockPos);
//            }
//            var2 |= CheckLightFor(EnumSkyBlock.Block, blockPos);
//            return var2;
//        }

//        public void CheclLightBlock(BlockPos blockPos)
//        {
//            for (int i = 0; i < 6; i++)
//            {
//                CheckLightFor(EnumSkyBlock.Block, blockPos.Offset((Pole)i));
//            }
//        }
//    }


//}
