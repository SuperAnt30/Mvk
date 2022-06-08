using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Collections.Generic;

namespace MvkServer.World.Chunk.Light
{
    /// <summary>
    /// Алгоритмы обработки освещения
    /// </summary>
    public class WorkingLight : ChunkHeir
    {
        /// <summary>
        /// Массив кэша
        /// </summary>
        private byte[,,] cacheList;
        private bool[,,] cacheListD;
        /// <summary>
        /// основная коллекция
        /// </summary>
        private List<LightStruct> list;
        /// <summary>
        /// Впомогательная коллекция
        /// </summary>
        private List<LightStruct> listCache;
        /// <summary>
        /// Карта блоков освещения
        /// </summary>
        private MapLight mapLight = new MapLight();
        /// <summary>
        /// Массив блоков которые надо обновить
        /// </summary>
        private List<vec3i> modifiedBlocks = new List<vec3i>();

        public WorkingLight(ChunkBase chunk) : base(chunk) { }
        public WorkingLight(ChunkBase chunk, BlockPos pos) : base(chunk) => modifiedBlocks.Add(pos.ToVec3i());

        /// <summary>
        /// Запустить запрос рендера
        /// </summary>
        public void ModifiedRender()
        {
            foreach (LightStruct light in mapLight.Values)
            {
                SetLight0(new BlockPos(light.Pos), light.Sky ? EnumSkyBlock.Sky : EnumSkyBlock.Block, light.Light);
            }
            foreach (vec3i pos in modifiedBlocks)
            {
                BlockPos blockPos = new BlockPos(pos);
                World.MarkBlockForUpdate(blockPos);
                for (int i = 0; i < 6; i++)
                {
                    BlockPos bPos = blockPos.Offset((Pole)i);
                    if (bPos.Y >= 0 && bPos.Y < ChunkBase.COUNT_HEIGHT_BLOCK)
                    {
                        World.MarkBlockForUpdate(bPos);
                    }
                }
            }
        }

        #region SetGetLight

        /// <summary>
        /// Получить уровень яркости тикущего блока, глобальные  координаты
        /// </summary>
        /// <param name="type">тип света</param>
        /// <param name="pos">позиция блока</param>
        /// <returns>яркость</returns>
        public byte GetLight(BlockPos pos, EnumSkyBlock type)
        {
            vec3i pos0 = pos.ToVec3i();

            LightStruct light = mapLight.Get(pos0, type == EnumSkyBlock.Sky);
            if (!light.IsEmpty()) return light.Light;

            if (pos.Y < 0) pos = new BlockPos(pos.X, 0, pos.Z);
            ChunkBase chunk = World.GetChunk(pos.GetPositionChunk());
            int sy = pos.GetPositionChunkY();
            //TODO::2022-06-06 IsEmptyData
            if (chunk == null || !chunk.StorageArrays[sy].IsSky())// || chunk.StorageArrays[sy].IsEmptyData())
                return (byte)type;

            if (type == EnumSkyBlock.Sky)
                return (byte)chunk.StorageArrays[sy].GetLightSky(pos.X & 15, pos.Y & 15, pos.Z & 15);
            return (byte)chunk.StorageArrays[sy].GetLightBlock(pos.X & 15, pos.Y & 15, pos.Z & 15);
        }
        /// <summary>
        /// Задать уровень яркости тикущего блока
        /// </summary>
        /// <param name="type">тип света</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="lightValue">яркость 0-15</param>
        public void SetLight(BlockPos pos, EnumSkyBlock type, int lightValue)
        {
            int light = GetLight(pos, type);
            if (light != lightValue)
            {
                mapLight.Set(new LightStruct(pos.ToVec3i(), (byte)lightValue, type == EnumSkyBlock.Sky));
            }
        }
        /// <summary>
        /// Задать уровень яркости тикущего блока
        /// </summary>
        /// <param name="type">тип света</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="lightValue">яркость 0-15</param>
        public void SetLight0(BlockPos pos, EnumSkyBlock type, int lightValue)
        {
            if (!pos.IsValidY()) return;
            
            ChunkBase chunk = World.GetChunk(pos.GetPositionChunk());
            if (chunk != null)
            {
                int yc = pos.GetPositionChunkY();

                // Нужна проверка, по созданию новых псевдо чанков освещения
                // Причём нужна проверка по всей высоте, чтоб не было промежутков
                if (yc > chunk.Light.MaxSkyChunk)
                {
                    int yc0 = chunk.Light.MaxSkyChunk + 1;
                    for (int i = yc0; i <= yc; i++)
                    {
                        chunk.StorageArrays[i].CheckBrightenBlockSky();
                    }
                    chunk.Light.MaxSkyChunk = yc;
                }

                if (type == EnumSkyBlock.Sky)
                {
                    chunk.StorageArrays[yc].SetLightSky(pos.X & 15, pos.Y & 15, pos.Z & 15, (byte)lightValue);
                }
                else
                {
                    chunk.StorageArrays[yc].SetLightBlock(pos.X & 15, pos.Y & 15, pos.Z & 15, (byte)lightValue);
                }
                //chunk.StorageArrays[pos.GetPositionChunkY()].SetLightFor(pos.X & 15, pos.Y & 15, pos.Z & 15, type, (byte)lightValue);
            }
            // зафиксировать диапазон
            if (!modifiedBlocks.Contains(pos.ToVec3i())) modifiedBlocks.Add(pos.ToVec3i());
        }

        /// <summary>
        /// Задать уровень яркости тикущего блока с проверкой небесного неба
        /// </summary>
        /// <param name="type">тип света</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="lightValue">яркость 0-15</param>
        protected void SetLightCheckSky(BlockPos pos, EnumSkyBlock type, int light)
        {
            if (type == EnumSkyBlock.Sky && IsAgainstSky(pos))
            {
                light = 15;
            }
            SetLight(pos, type, light);
        }

        /// <summary>
        /// Возвращает уровень яркости блока, анализируя соседние блоки
        /// </summary>
        /// <param name="pos">позиция</param>
        /// <param name="type">тип света</param>
        /// <returns>яркость</returns>
        private int LevelBright(BlockPos pos, EnumSkyBlock type)
        {
            if (type == EnumSkyBlock.Sky && IsAgainstSky(pos))
            {
                // Если небо, и видим небо, яркость максимальная
                return 15;
            }
            BlockBase block = World.GetBlockState(pos).GetBlock();
            // Количество излучаемого света
            int light = type == EnumSkyBlock.Sky ? 0 : block.LightValue;

            // Сколько света вычитается для прохождения этого блока
            int opacity = block.LightOpacity;
            if (opacity >= 15 && block.LightValue > 0) opacity = 1;
            if (opacity < 1) opacity = 1;

            // Если блок не проводит свет, значит темно
            if (opacity >= 15) return 0;
            // Если блок яркий выводим значение
            if (light >= 14) return light;

            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            for (int i = 0; i < 6; i++)
            {
                BlockPos pos2 = pos.Offset((Pole)i);
                if (pos2.Y >= 0 && pos2.Y < ChunkBase.COUNT_HEIGHT_BLOCK)
                {
                    int lightNew = GetLight(pos2, type) - opacity;
                    // Если соседний блок ярче текущего блока
                    if (lightNew > light) light = lightNew;
                    // Если блок яркий выводим значение
                    if (light >= 14) return light;
                }
            }
            return light;
        }

        /// <summary>
        /// Может ли видеть небо
        /// </summary>
        private bool IsAgainstSky(BlockPos pos)
        {
            ChunkBase chunk = World.GetChunk(pos.GetPositionChunk());
            if (chunk == null) return false;
            return chunk.Light.IsAgainstSky(pos);
        }

        /// <summary>
        /// зафиксировать диапазон
        /// </summary>
        /// <param name="pos">позиция блока</param>
        //public void BlockModify(BlockPos pos)
        //{
        //    if (!modifiedBlocks.Contains(pos.Position))
        //        modifiedBlocks.Add(pos.Position);
        //    //World.MarkBlockForUpdate(pos);
        //   // modified.BlockModify(pos);
        //}

        #endregion

        /// <summary>
        /// Проверяем освещение блока и неба
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="isSky">надо ли проверять небо</param>
        public void CheckLight(BlockPos pos, bool isSky)
        {
            if (isSky) CheckLightFor(pos.X, pos.Y, pos.Y, pos.Z);
            CheckLightFor(pos.X, pos.Y, -1, pos.Z);
        }

        /// <summary>
        /// Осветляем за счёт блока или неба при старте
        /// </summary>
        /// <param name="x">глобальная X</param>
        /// <param name="yDown">нижняя точка Y</param>
        /// <param name="yUp">Если равен -1, то проверка блоков, если больше, то это верхняя позиция столба, для проверки неба</param>
        /// <param name="z">глобальная Z</param>
        public void CheckLightFor(int x, int yDown, int yUp, int z)
        {
            EnumSkyBlock type = yUp == -1 ? EnumSkyBlock.Block : EnumSkyBlock.Sky;

            BlockPos pos = new BlockPos(x, yDown, z);
            // тикущаяя яркость
            int lightVox = GetLight(pos, type);
            // Планируемая световая яркость
            int light = LevelBright(pos, type);

            if (lightVox < light || (lightVox == 15 && type == EnumSkyBlock.Sky))
            {
                // Осветлить
                BrighterLightFor(pos, type, (byte)light, yUp);
                // Тест столба для визуализации ввиде стекла
                //for (int y = pos.Y; y <= yUp; y++) World.SetBlockDebug(new BlockPos(pos.X, y, pos.Z), EnumBlock.Glass);
            }
            else if (lightVox > light && lightVox > 0)
            {
                // Затемнить
                List<LightStruct> list = DarkenLightFor(pos, type, (byte)lightVox, yUp);
                // Возврат осветления при затемнении
                foreach (LightStruct c in list)
                {
                    BlockPos pos2 = new BlockPos(c.Pos + c.Vec);
                    BrighterLightFor(pos2, type, c.Light, pos2.Y);
                    // Тест столба для визуализации ввиде стекла
                    //World.SetBlockDebug(pos2, EnumBlock.Glass);//, c.Light);
                }
            }
        }

        /// <summary>
        /// Затемняем неба
        /// </summary>
        /// <param name="x">глобальная X</param>
        /// <param name="yDown">нижняя точка Y</param>
        /// <param name="yUp">Если равен 0, то проверка блоков, если больше, то это верхняя позиция столба, для проверки неба</param>
        /// <param name="z">глобальная Z</param>
        public void CheckDarkenLightFor(int x, int yDown, int yUp, int z)
        {
            // Тест столба для визуализации ввиде стекла
            //for (int y = yDown; y <= yUp; y++) Chunk.SetBlockState(x & 15, y, z & 15, EnumBlock.Glass);

            EnumSkyBlock type = yUp == -1 ? EnumSkyBlock.Block : EnumSkyBlock.Sky;

            BlockPos pos = new BlockPos(x, yDown, z);
            // тикущаяя яркость
            int lightVox = GetLight(pos, type);
            // Затемнить
            List<LightStruct> list = DarkenLightFor(pos, type, (byte)lightVox, yUp);
            // Возврат осветления при затемнении
            foreach (LightStruct c in list)
            {
                BlockPos pos2 = new BlockPos(c.Pos + c.Vec);
                BrighterLightFor(pos2, type, c.Light, pos2.Y);
                // Тест столба для визуализации ввиде стекла
                //World.SetBlock(pos2, EnumBlock.Glass, c.Light);
            }
        }

        #region Brighter

        /// <summary>
        /// Осветляем за счёт блока или неба при старте
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="type">тип освещения</param>
        /// <param name="light">планируемая яркость</param>
        /// <param name="yUp">максимальная высота, если столб</param>
        protected void BrighterLightFor(BlockPos pos, EnumSkyBlock type, byte light, int yUp)
        {
            if (yUp >= 0)
            {
                for (int y = pos.Y; y <= yUp; y++)
                {
                    SetLightCheckSky(new BlockPos(pos.X, y, pos.Z), type, light);
                }
            }
            else
            {
                SetLightCheckSky(pos, type, light);
            }

            // основная коллекция
            list = new List<LightStruct>() { new LightStruct(pos.ToVec3i(), light) };
            // Впомогательная коллекция
            listCache = new List<LightStruct>();

            // Кэш массива проверки яркости блока, 31*31*31
            int yh = yUp > pos.Y ? 31 + yUp - pos.Y : 31;
            cacheList = new byte[31, yh, 31];
            // Параметр для первого блока
            bool isBegin = true;

            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (list.Count > 0)
            {
                foreach (LightStruct c in list)
                {
                    int lightNew = c.Light;
                    if (isBegin || RefreshBrighterLight(pos.Offset(c.Vec), c.Light, type, out lightNew))
                    {
                        if (yh > 31 && isBegin)
                        {
                            // для столбца в первом проходе
                            int yMax = yUp - pos.Y;
                            for (int y = 0; y < yMax; y++) // Цикл рядов
                            {
                                for (int i = 0; i < 4; i++) // Цикл сторон
                                {
                                    SetCacheBrighter(pos, EnumFacing.GetHorizontal(i), new vec3i(0, y, 0), (byte)lightNew);
                                }
                            }
                            // добавляем вверхний блок
                            SetCacheBrighter(pos, Pole.Up, new vec3i(0, yMax - 1, 0), (byte)lightNew);
                            // добавляем нижний блок
                            SetCacheBrighter(pos, Pole.Down, new vec3i(0), (byte)lightNew);
                        }
                        else
                        {
                            // для блока
                            for (int i = 0; i < 6; i++) // Цикл сторон
                            {
                                SetCacheBrighter(pos, (Pole)i, c.Vec, (byte)lightNew);
                            }
                        }
                        isBegin = false;
                    }
                }
                list = listCache;
                listCache = new List<LightStruct>();
            }
        }

        /// <summary>
        /// Изменение кэша в момент осветления
        /// </summary>
        protected void SetCacheBrighter(BlockPos pos, Pole pole, vec3i vec, byte lightNew)
        {
            vec3i v = vec + EnumFacing.DirectionVec(pole);
            if (cacheList[v.x + 15, v.y + 15, v.z + 15] > lightNew) return;
            cacheList[v.x + 15, v.y + 15, v.z + 15] = lightNew;
            listCache.Add(new LightStruct(pos.ToVec3i(), v, lightNew));
        }

        /// <summary>
        /// Обновить освещения в момент осветления
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="light">освещение</param>
        /// <param name="type">тип света</param>
        /// <param name="lightNew">будущее освещение</param>
        /// <returns>true было изменение, false нет изменения</returns>
        protected bool RefreshBrighterLight(BlockPos pos, int light, EnumSkyBlock type, out int lightNew)
        {
            // Определяем тикущую яркость блока
            lightNew = GetLight(pos, type);
            if (!pos.IsValidY()) return false;
            BlockState blockState = World.GetBlockState(pos);
            //if (blockState.IsEmpty()) return false; // TODO:: из-за этого не корректно работал null псевдо чанк света
            // Определяем яркость, какая должна
            light = light - (blockState.GetBlock().LightOpacity + 1);
            if (light < 0) light = 0;
            if (lightNew >= light) return false;
            // Если тикущая темнее, осветляем её
            lightNew = light;
            SetLight(pos, type, (byte)lightNew);
            return true;
        }

        #endregion

        #region Darken 

        /// <summary>
        /// Затемнить 
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="type">тип освещения</param>
        /// <param name="light">планируемая яркость</param>
        /// <param name="yUp">максимальная высота, если столб</param>
        protected List<LightStruct> DarkenLightFor(BlockPos pos, EnumSkyBlock type, byte light, int yUp)
        {
            if (yUp >= 0)
            {
                for (int y = pos.Y; y <= yUp; y++)
                {
                    SetLight(new BlockPos(pos.X, y, pos.Z), type, 0);
                }
            }
            else
            {
                SetLight(pos, type, 0);
            }

            // основная коллекция
            list = new List<LightStruct>() { new LightStruct(pos.ToVec3i(), light) };
            // Впомогательная коллекция
            listCache = new List<LightStruct>();
            // Коллекция для подготовки осветления
            List<LightStruct> listResult = new List<LightStruct>();

            // Кэш массива проверки яркости блока, 31*31*31
            int yh = yUp > pos.Y ? 33 + yUp - pos.Y : 33;
            cacheListD = new bool[33, yh, 33];
            // Параметр для первого блока
            bool isBegin = true;

            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (list.Count > 0)
            {
                foreach (LightStruct c in list)
                {
                    int lightNew = c.Light;
                    bool b = isBegin; // Первый проход
                    if (!b)
                    {
                        int bb = RefreshDarkenLight(pos.Offset(c.Vec), c.Light, type, out lightNew);
                        if (bb == 2)
                        {
                            if (lightNew > 1)// && lightNew < 15)
                            {
                                // Блок ярче нужного, значит готовим блок для осветления
                                listResult.Add(new LightStruct(pos.ToVec3i(), c.Vec, (byte)lightNew));
                            }
                        }
                        else if (bb == 1) b = true;
                    }

                    if (b)
                    {
                        if (yh > 33 && isBegin)
                        {
                            // для столбца в первом проходе
                            int yMax = yUp - pos.Y;
                            for (int y = 0; y < yMax; y++) // Цикл рядов
                            {
                                for (int i = 0; i < 4; i++) // Цикл сторон
                                {
                                    SetCacheDarken(pos, EnumFacing.GetHorizontal(i), new vec3i(0, y, 0), (byte)lightNew);
                                }
                            }
                            // добавляем вверхний блок
                            SetCacheDarken(pos, Pole.Up, new vec3i(0, yMax - 1, 0), (byte)lightNew);
                            // добавляем нижний блок
                            SetCacheDarken(pos, Pole.Down, new vec3i(0), (byte)lightNew);
                        }
                        else
                        {
                            // для блока
                            for (int i = 0; i < 6; i++) // Цикл сторон
                            {
                                SetCacheDarken(pos, (Pole)i, c.Vec, (byte)lightNew);
                            }
                        }
                        isBegin = false;
                    }
                }
                list = listCache;
                listCache = new List<LightStruct>();
            }
            return listResult;
        }

        /// <summary>
        /// Обновить звтемнение
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="light">освещение</param>
        /// <param name="type">тип света</param>
        /// <returns>0 - остановить проход, 1 - продолжить проход, 2 - остановить проход с записью для освещения</returns>
        protected int RefreshDarkenLight(BlockPos pos, int light, EnumSkyBlock type, out int lightNew)
        {
            lightNew = 0;
            // Если блок уже равна темноте, останавливаем проход
            if (light == 0 || !pos.IsValidY()) return 0;
            BlockBase block = World.GetBlockState(pos).GetBlock();
            // Определяем тикущую яркость блока
            lightNew = GetLight(pos, type);
            // Если фактическая яркость больше уровня прохода,
            // значит зацепили соседний источник света, прерываем без изменения 
            // с будущей пометкой на проход освещения
            if (lightNew > light || (type == EnumSkyBlock.Block && block.LightValue > 0))
            {
                if (type == EnumSkyBlock.Block && block.LightValue > 0) lightNew = block.LightValue;
                return 2;
            }
            // Изменяем яркость в полностью тёмную
            if (type == EnumSkyBlock.Sky && IsAgainstSky(pos)) return 2;
            SetLight(pos, type, 0);
            lightNew = light - 1;
            return 1;
        }

        /// <summary>
        /// Изменение кэша в момент осветления
        /// </summary>
        protected void SetCacheDarken(BlockPos pos, Pole pole, vec3i vec, byte lightNew)
        {
            vec3i v = vec + EnumFacing.DirectionVec(pole);
            if (cacheListD[v.x + 16, v.y + 16, v.z + 16]) return;
            cacheListD[v.x + 16, v.y + 16, v.z + 16] = true;
            listCache.Add(new LightStruct(pos.ToVec3i(), v, lightNew));
        }

        #endregion
    }
}
