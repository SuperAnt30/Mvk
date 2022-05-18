//using MvkServer.Glm;
//using MvkServer.Util;
//using MvkServer.World.Block;
//using MvkServer.World.Chunk;

//namespace MvkServer.World.Light
//{
//    /// <summary>
//    /// Объект работы с освещением
//    /// </summary>
//    public class ChunkLight
//    {
//        /// <summary>
//        /// Объект базового мира
//        /// </summary>
//        public WorldBase World { get; private set; }
//        /// <summary>
//        /// Объект тикущего чанка
//        /// </summary>
//        public ChunkBase Chunk { get; private set; }

//        /// <summary>
//        /// Карта высот по чанку, XZ
//        /// </summary>
//        private int[,] heightMap = new int[16, 16];

//        /// <summary>
//        /// Наименьшее значение на карте высот
//        /// </summary>
//        private int heightMapMinimum;

//        /// <summary>
//        /// Карта, похожая на heightMap, которая отслеживает, как далеко могут падать осадки.
//        /// </summary>
//        private int[,] precipitationHeightMap = new int[16, 16];

//        /// <summary>
//        /// Какие столбцы нуждаются в обновлении их skylightMaps
//        /// </summary>
//        private bool[,] updateSkylightColumns = new bool[16, 16];

//        /// <summary>
//        /// Нужен ли боковой рендер света, проверям в тиках
//        /// </summary>
//        private bool isGapLightingUpdated = true; // false;

//        /// <summary>
//        /// Содержит текущий индекс циклической проверки повторной проверки, 
//        /// а также подразумевается как место проверки повторной проверки.
//        /// </summary>
//        private int queuedLightChecks = 4096;

//        /// <summary>
//        /// Логическое значение, указывающее, заселен ли ландшафт.
//        /// </summary>
//        private bool isTerrainPopulated;
//        private bool isLightPopulated;

//        public ChunkLight(ChunkBase chunk)
//        {
//            Chunk = chunk;
//            World = chunk.World;
//            //RecheckGapsStart();
//        }

//        /// <summary>
//        /// Наименьшее значение на карте высот
//        /// </summary>
//        public int GetLowestHeight() => heightMapMinimum;

//        /// <summary>
//        /// Получить высоту блока
//        /// </summary>
//        /// <param name="x">0..15</param>
//        /// <param name="z">0..15</param>
//        public int GetHeight(int x, int z) => heightMap[x, z];

//        /// <summary>
//        /// Получить сколько света проходит через блок
//        /// </summary>
//        private int GetBlockLightOpacity(int x, int y, int z)
//        {
//            EnumBlock enumBlock = Chunk.GetEBlock(x, y, z);
//            BlockBase block = Blocks.GetBlockCache(enumBlock);
//            return block.LightOpacity;
//        }

//        /// <summary>
//        /// Генерирует карту высот для чанка с нуля
//        /// </summary>
//        public void GenerateHeightMap()
//        {
//            int sy = Chunk.GetTopFilledSegment();
//            heightMapMinimum = 999;

//            for (int x = 0; x < 16; ++x)
//            {
//                int z = 0;

//                while (z < 16)
//                {
//                    precipitationHeightMap[x, z] = -999;
//                    int y = sy + 16;

//                    while (true)
//                    {
//                        if (y > 0)
//                        {
//                            if (GetBlockLightOpacity(x, y - 1, z) == 0)
//                            {
//                                y--;
//                                continue;
//                            }
//                            heightMap[x, z] = y;
//                            if (y < heightMapMinimum) heightMapMinimum = y;
//                        }
//                        z++;
//                        break;
//                    }
//                }
//            }

//            Chunk.Modified();
//        }

//        /// <summary>
//        /// Создает исходную карту просвета для чанка при генерации или загрузке.
//        /// </summary>
//        public void GenerateSkylightMap()
//        {
//            int sy = Chunk.GetTopFilledSegment();
//            heightMapMinimum = 999;

//            for (int x = 0; x < 16; ++x)
//            {
//                int z = 0;

//                while (z < 16)
//                {
//                    precipitationHeightMap[x, z] = -999;
//                    int y = sy + 16;

//                    while (true)
//                    {
//                        if (y > 0)
//                        {
//                            if (GetBlockLightOpacity(x, y - 1, z) == 0)
//                            {
//                                y--;
//                                continue;
//                            }

//                            heightMap[x, z] = y;

//                            if (y < heightMapMinimum) heightMapMinimum = y;
//                        }

//                        if (!World.HasNoSky)
//                        {
//                            y = 15;
//                            int y0 = sy + 16 - 1;

//                            do
//                            {
//                                int opacity = GetBlockLightOpacity(x, y0, z);

//                                if (opacity == 0 && y != 15)
//                                {
//                                    opacity = 1;
//                                }

//                                y -= opacity;

//                                if (y > 0)
//                                {
//                                    if (!Chunk.StorageArrays[y0 >> 4].IsEmpty())
//                                    {
//                                        Chunk.StorageArrays[y0 >> 4].SetLightFor(
//                                            x, y0 & 15, z, EnumSkyBlock.Sky, (byte)y);
//                                     //   World.MarkBlockForUpdate(new BlockPos((Chunk.Position.x << 4) + x, y0, (Chunk.Position.y << 4) + z));
//                                    }
//                                }
//                                y0--;
//                            }
//                            while (y0 > 0 && y > 0);
//                        }
//                        ++z;
//                        break;
//                    }
//                }
//            }
//            Chunk.Modified();
//        }

//        /// <summary>
//        /// Обновление в такте
//        /// </summary>
//        public void Update()
//        {
//            if (isGapLightingUpdated && !World.HasNoSky)
//            {
//                RecheckGaps(World.IsRemote);
//            }
//            if (!isLightPopulated && isTerrainPopulated)
//            {
//                func_150809_p();
//            }
//            World.TheProfiler().StartSection("checkLight");
//            EnqueueRelightChecks();
//            World.TheProfiler().EndSection();
//        }

//        private void RecheckGapsStart()
//        {
//            for (int x = 0; x < 16; x++)
//            {
//                for (int z = 0; z < 16; z++)
//                {
//                    updateSkylightColumns[x, z] = true;
//                }
//            }
//            //isGapLightingUpdated = true;
//            RecheckGaps(false);
//        }

//        /// <summary>
//        /// При необходимости распространяет значение освещенности данного видимого с неба блока вниз и вверх на соседние блоки.
//        /// Координаты локальные 0..15
//        /// </summary>
//        private void PropagateSkylightOcclusion(int x, int z)
//        {
//            updateSkylightColumns[x, z] = true;
//            isGapLightingUpdated = true;
//        }

//        private void RecheckGaps(bool worldIsRemote)
//        {
//            World.TheProfiler().StartSection("RecheckGaps");
//            int chX16 = Chunk.Position.x << 4;
//            int chZ16 = Chunk.Position.y << 4;
//            if (World.IsAreaLoaded(new BlockPos(chX16 + 8, 0, chZ16 + 8), 16))
//            {
//                for (int x = 0; x < 16; ++x)
//                {
//                    for (int z = 0; z < 16; ++z)
//                    {
//                        if (updateSkylightColumns[x, z])
//                        {
//                            updateSkylightColumns[x, z] = false;
//                            int height = heightMap[x, z];
//                            int xb = chX16 + x;
//                            int zb = chZ16 + z;
//                            int yh = 999;
//                            //Iterator var8;
//                            //Pole var9;

//                            foreach (Pole pole in EnumFacing.ArrayHorizontal())
//                            {
//                                vec3i vec = EnumFacing.DirectionVec(pole);
//                                yh = Mth.Min(yh, World.GetChunksLowestHorizon(xb + vec.x, zb + vec.z));
//                            }
//                            CheckSkylightNeighborHeight(xb, zb, yh);

//                            foreach (Pole pole in EnumFacing.ArrayHorizontal())
//                            {
//                                vec3i vec = EnumFacing.DirectionVec(pole);
//                                CheckSkylightNeighborHeight(xb + vec.x, zb + vec.z, height);
//                                yh = Mth.Min(yh, World.GetChunksLowestHorizon(xb + vec.x, zb + vec.z));
//                            }

//                            if (worldIsRemote)
//                            {
//                                World.TheProfiler().EndSection();
//                                return;
//                            }
//                        }
//                    }
//                }

//                isGapLightingUpdated = false;
//            }

//            World.TheProfiler().EndSection();
//        }

//        /// <summary>
//        /// Проверяет высоту блока рядом с видимым в небе блоком и при необходимости планирует обновление освещения.
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="z"></param>
//        /// <param name="hy"></param>
//        private void CheckSkylightNeighborHeight(int x, int z, int hy)
//        {
//            int hy2 = World.GetHorizon(new BlockPos(x, 0, z)).Y;
//            if (hy2 > hy) UpdateSkylightNeighborHeight(x, z, hy, hy2 + 1);
//            else if (hy2 < hy) UpdateSkylightNeighborHeight(x, z, hy2, hy + 1);
//        }

//        private void UpdateSkylightNeighborHeight(int x, int z, int startY, int endY)
//        {
//            if (endY > startY && World.IsAreaLoaded(new BlockPos(x, 0, z), 16))
//            {
//                for (int y = startY; y < endY; y++)
//                {
//                    World.Light.CheckLightFor(EnumSkyBlock.Sky, new BlockPos(x, y, z));
//                }

//                Chunk.Modified();
//            }
//        }

//        /// <summary>
//        /// Получить яркость блока
//        /// </summary>
//        private int GetLightFor(EnumSkyBlock type, BlockPos blockPos)
//        {
//            vec3i pos = blockPos.GetPosition0();
//            int sy = blockPos.Y >> 4;
//            return Chunk.StorageArrays[sy].IsEmpty()
//                ? (CanSeeSky(blockPos) 
//                    ? (int)type 
//                    : 0)
//                : (type == EnumSkyBlock.Sky
//                    ? (World.HasNoSky 
//                        ? 0 
//                        : Chunk.StorageArrays[sy].GetLightFor(pos.x, pos.y & 15, pos.z, EnumSkyBlock.Sky))
//                    : (type == EnumSkyBlock.Block 
//                        ? Chunk.StorageArrays[sy].GetLightFor(pos.x, pos.y & 15, pos.z, EnumSkyBlock.Block) 
//                        : (int)type));
//        }

//        /// <summary>
//        /// Может видеть небо
//        /// </summary>
//        public bool CanSeeSky(BlockPos pos) => pos.Y >= heightMap[pos.X & 15, pos.Z & 15];

//        /// <summary>
//        /// Проверка высоты осадок
//        /// </summary>
//        public void CheckPrecipitationHeightMap(BlockPos blockPos)
//        {
//            vec3i pos0 = blockPos.GetPosition0();
//            if (pos0.y >= precipitationHeightMap[pos0.x, pos0.z] - 1) precipitationHeightMap[pos0.x, pos0.z] = -999;
//        }

//        public void CheckBlockState(BlockPos blockPos, int height, int lightOpacity, int lightOpacityOld)
//        {
//            int bx = blockPos.X & 15;
//            int by = blockPos.Y;
//            int bz = blockPos.Z & 15;

//            if (lightOpacity > 0)
//            {
//                if (by >= height) RelightBlock(bx, by + 1, bz);
//            }
//            else if (by == height - 1)
//            {
//                RelightBlock(bx, by, bz);
//            }

//            if (lightOpacity != lightOpacityOld 
//                && (lightOpacity < lightOpacityOld || GetLightFor(EnumSkyBlock.Sky, blockPos) > 0 
//                    || GetLightFor(EnumSkyBlock.Block, blockPos) > 0))
//            {
//                PropagateSkylightOcclusion(bx, bz);
//            }
//        }

//        /// <summary>
//        /// Инициирует перерасчет как блочного, так и небесного света для заданного блока внутри чанка.
//        /// </summary>
//        /// <param name="x"></param>
//        /// <param name="y"></param>
//        /// <param name="z"></param>
//        private void RelightBlock(int x, int y, int z)
//        {
//            int y1 = heightMap[x, z] & 255;
//            int y2 = y1;

//            if (y > y1) y2 = y;

//            while (y2 > 0 && GetBlockLightOpacity(x, y2 - 1, z) == 0)
//            {
//                --y2;
//            }

//            if (y2 != y1)
//            {
//                int gx = Chunk.Position.x << 4 | x;
//                int gz = Chunk.Position.y << 4 | z;
//                World.Light.MarkBlocksDirtyVertical(gx, gz, y2, y1);
//                heightMap[x, z] = y2;
//                int y0;

//                if (!World.HasNoSky)
//                {
//                    if (y2 < y1)
//                    {
//                        for (y0 = y2; y0 < y1; ++y0)
//                        {
//                            if (!Chunk.StorageArrays[y0 >> 4].IsEmpty())
//                            {
//                                Chunk.StorageArrays[y0 >> 4].SetLightFor(x, y0 & 15, z, EnumSkyBlock.Sky, 15);
//                                World.Light.NotifyLightSet(new BlockPos((Chunk.Position.x << 4) + x, y0, (Chunk.Position.y << 4) + z));
//                            }
//                        }
//                    }
//                    else
//                    {
//                        for (y0 = y1; y0 < y2; ++y0)
//                        {
//                            if (!Chunk.StorageArrays[y0 >> 4].IsEmpty())
//                            {
//                                Chunk.StorageArrays[y0 >> 4].SetLightFor(x, y0 & 15, z, EnumSkyBlock.Sky, 0);
//                                World.Light.NotifyLightSet(new BlockPos((Chunk.Position.x << 4) + x, y0, (Chunk.Position.y << 4) + z));
//                            }
//                        }
//                    }

//                    y0 = 15;

//                    while (y2 > 0 && y0 > 0)
//                    {
//                        y2--;
//                        int lightOpacity = GetBlockLightOpacity(x, y2, z);

//                        if (lightOpacity == 0) lightOpacity = 1;
//                        y0 -= lightOpacity;
//                        if (y0 < 0) y0 = 0;

//                        if (!Chunk.StorageArrays[y2 >> 4].IsEmpty())
//                        {
//                            Chunk.StorageArrays[y0 >> 4].SetLightFor(x, y2 & 15, z, EnumSkyBlock.Sky, (byte)y0);
//                        }
//                    }
//                }

//                y0 = heightMap[x, z];
//                int y3 = y1;
//                int y4 = y0;

//                if (y0 < y1)
//                {
//                    y3 = y0;
//                    y4 = y1;
//                }

//                if (y0 < heightMapMinimum) heightMapMinimum = y0;

//                if (!World.HasNoSky)
//                {
//                    Pole[] poles = EnumFacing.ArrayHorizontal();
//                    foreach(Pole pole in poles)
//                    {
//                        vec3i vec = EnumFacing.DirectionVec(pole);
//                        UpdateSkylightNeighborHeight(gx + vec.x, gz + vec.z, y3, y4);
//                    }

//                    UpdateSkylightNeighborHeight(gx, gz, y3, y4);
//                }

//                Chunk.Modified();
//            }
//        }

//        /// <summary>
//        /// Сбрасывает индекс проверки повторного освещения на 0 для этого фрагмента.
//        /// </summary>
//        public void ResetRelightChecks() => queuedLightChecks = 0;

//        /// <summary>
//        /// Вызывается один раз на блок за тик и продвигает индекс циклической повторной проверки 
//        /// до 8 блоков за раз. В худшем случае потенциально может потребоваться до 25,6 секунд, 
//        /// рассчитанных с помощью (4096/8)/20, для повторной проверки всех блоков в чанке, 
//        /// что может объяснить отставание световых обновлений при первоначальной генерации мира.
//        /// </summary>
//        private void EnqueueRelightChecks()
//        {
//            if (queuedLightChecks >= 4096) return;

//            BlockPos blockPos = new BlockPos(Chunk.Position.x << 4, 0, Chunk.Position.y << 4);
//            int i = 0;
//            while(i < 16 && queuedLightChecks < 4096) 
//            //for (int i = 0; i < 8; ++i)
//            {
//                i++;
//                int y0 = queuedLightChecks % 16;
//                int x0 = queuedLightChecks / 16 % 16;
//                int z0 = queuedLightChecks / 256;
//                queuedLightChecks++;

//                for (int y = 0; y < 16; y++)
//                {
//                    BlockPos blockPos2 = blockPos.Add(x0, (y0 << 4) + y, z0);
//                    bool b = y == 0 || y == 15 || x0 == 0 || x0 == 15 || z0 == 0 || z0 == 15;

//                    if ((Chunk.StorageArrays[y0].IsEmpty() && b) 
//                        | (!Chunk.StorageArrays[y0].IsEmpty() && Chunk.StorageArrays[y0].GetEBlock(x0, y, z0) == EnumBlock.Air))
//                    {
//                        for (int j = 0; j < 6; j++)
//                        {
//                            BlockPos blockPos3 = blockPos2.Offset((Pole)j);
//                            if (World.GetBlockState(blockPos3).GetBlock().LightValue > 0)
//                            {
//                                World.Light.CheckLight(blockPos3);
//                            }
//                        }
//                        World.Light.CheckLight(blockPos2);
//                    }
//                }

//                //if (queuedLightChecks >= 4096) return;
//            }
//        }

//        public void func_150809_p()
//        {
//            isTerrainPopulated = true;
//            isLightPopulated = true;
//            BlockPos blockPos = new BlockPos(Chunk.Position.x << 4, 0, Chunk.Position.y << 4);

//            if (!World.HasNoSky)
//            {
//                if (World.IsAreaLoaded(blockPos.Add(7, 64, 7), 9))
//                {
//                    bool b = false;
//                    for (int x = 0; x < 16; x++)
//                    {
//                        for (int z = 0; z < 16; z++)
//                        {
//                            if (!func_150811_f(x, z))
//                            {
//                                isLightPopulated = false;
//                                b = true;
//                                break;
//                            }
//                        }
//                        if (b) break;
//                    }

//                    if (isLightPopulated)
//                    {
//                        Pole[] poles = EnumFacing.ArrayHorizontal();
                        
//                        foreach(Pole pole in poles)
//                        {
//                            vec3i vec = EnumFacing.DirectionVec(pole);
//                            if (vec.x > 0 || vec.y > 0 || vec.z > 0) vec *= 16;
//                            World.GetChunk(blockPos.Offset(vec)).Light.func_180700_a(EnumFacing.GetOpposite(pole));
//                        }
//                        RecheckGapsStart();
//                    }
//                }
//                else
//                {
//                    isLightPopulated = false;
//                }
//            }
//        }

//        public void func_180700_a(Pole p_180700_1_)
//        {
//            if (isTerrainPopulated)
//            {
//                int var2;

//                if (p_180700_1_ == Pole.East)
//                {
//                    for (var2 = 0; var2 < 16; ++var2)
//                    {
//                        func_150811_f(15, var2);
//                    }
//                }
//                else if (p_180700_1_ == Pole.West)
//                {
//                    for (var2 = 0; var2 < 16; ++var2)
//                    {
//                        func_150811_f(0, var2);
//                    }
//                }
//                else if (p_180700_1_ == Pole.South)
//                {
//                    for (var2 = 0; var2 < 16; ++var2)
//                    {
//                        func_150811_f(var2, 15);
//                    }
//                }
//                else if (p_180700_1_ == Pole.North)
//                {
//                    for (var2 = 0; var2 < 16; ++var2)
//                    {
//                        func_150811_f(var2, 0);
//                    }
//                }
//            }
//        }

//        private bool func_150811_f(int p_150811_1_, int p_150811_2_)
//        {
//            BlockPos blockPos = new BlockPos(Chunk.Position.x << 4, 0, Chunk.Position.y << 4);
//            int var4 = Chunk.GetTopFilledSegment();
//            bool var5 = false;
//            bool var6 = false;
//            int var7;
//            BlockPos var8;

//            for (var7 = var4 + 16 - 1; var7 > 63 || var7 > 0 && !var6; --var7)
//            {
//                var8 = blockPos.Add(p_150811_1_, var7, p_150811_2_);
//                vec3i pos = var8.GetPosition0();
//                int var9 = GetBlockLightOpacity(pos.x, pos.y, pos.z);

//                if (var9 == 255 && var7 < 63)
//                {
//                    var6 = true;
//                }

//                if (!var5 && var9 > 0)
//                {
//                    var5 = true;
//                }
//                else if (var5 && var9 == 0 && !World.Light.CheckLight(var8))
//                {
//                    return false;
//                }
//            }

//            for (; var7 > 0; --var7)
//            {
//                var8 = blockPos.Add(p_150811_1_, var7, p_150811_2_);

//                if (Blocks.GetBlockCache(Chunk.GetEBlock(var8.GetPosition0())).LightValue > 0)
//                {
//                    World.Light.CheckLight(var8);
//                }
//            }

//            return true;
//        }
//    }
//}
