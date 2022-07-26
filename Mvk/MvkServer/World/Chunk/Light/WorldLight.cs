using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Diagnostics;

namespace MvkServer.World.Chunk.Light
{
    /// <summary>
    /// Класс для работы освещения, прендалижит миру
    /// </summary>
    public class WorldLight
    {
        /// <summary>
        /// Количество изменённых блоков
        /// </summary>
        private int countBlock = 0;
        /// <summary>
        /// Отладочный стринг
        /// </summary>
        private string debugStr = "";
        /// <summary>
        /// Вспомогательный массив, значения в битах 00000000 LLLLzzzz zzyyyyyy yyxxxxxx
        /// 388096 * 3, где 388096 максимально возможное количество блоков в чанке плюс соседние блоки
        /// (16 * 16 + (14 * 16 + 13 * 6 + 13) * 4) * 256 = 388 096
        /// </summary>
        private readonly int[] arCache = new int[1164288];
        /// <summary>
        /// Начальный индекс вспомогательного массива
        /// </summary>
        private int indexBegin;
        /// <summary>
        /// Конечный индекс вспомогательного массива
        /// </summary>
        private int indexEnd;
        /// <summary>
        /// Тикущий индекс вспомогательного массива
        /// </summary>
        private int indexActive;
        /// <summary>
        /// Координаты изменения блоков
        /// </summary>
        private int axisX0, axisY0, axisZ0, axisX1, axisY1, axisZ1;
        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private readonly ChunkBase[] chunks = new ChunkBase[8];
        /// <summary>
        /// Чанк в котором был запуск обработки освещения
        /// </summary>
        private ChunkBase chunk;
        /// <summary>
        /// Координаты чанка, в котором был запуск освещения
        /// </summary>
        private int chBeginX, chBeginY;
        /// <summary>
        /// Смещение блока от основнока чанка
        /// </summary>
        private int bOffsetX, bOffsetZ;
        // Минимальная высота для соседнего блока
        private readonly int[] yMin = new int[4];
        // Максимальная высота для соседнего блока
        private readonly int[] yMax = new int[4];

        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldBase World { get; protected set; }

        public WorldLight(WorldBase worldIn)
        {
            World = worldIn;
        }

        #region Public

        /// <summary>
        /// Очистить отладочную строку
        /// </summary>
        public void ClearDebugString() => debugStr = "";
        /// <summary>
        /// Вернуть отладочную строку
        /// </summary>
        public string ToDebugString() => debugStr;

        /// <summary>
        /// Активировать чанк в котором будет обработка освещения
        /// </summary>
        public void ActionChunk(ChunkBase chunk)
        {
            this.chunk = chunk;
            chBeginX = chunk.Position.x;
            chBeginY = chunk.Position.y;
            bOffsetX = chBeginX << 4;
            bOffsetZ = chBeginY << 4;
        }

        /// <summary>
        /// Проверяем освещение блока и неба при изменении блока
        /// </summary>
        /// <param name="x">глобальная позиция блока x</param>
        /// <param name="y">глобальная позиция блока y</param>
        /// <param name="z">глобальная позиция блока z</param>
        public void CheckLightFor(int x, int y, int z)
        {
            if (y < 0 || y > ChunkBase.COUNT_HEIGHT_BLOCK) return;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            if (!UpChunks()) return;

            ChunkStorage chunkStorage = chunk.StorageArrays[y >> 4];
            int index = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
            byte lo = 0;
            if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[index] & 0xFFF];
            // текущаяя яркость
            byte lightOld = chunkStorage.lightBlock[index];
            // яркость от блока
            byte lightBlock = (byte)(lo & 0xF);
            // яркость от соседних блоков
            byte lightBeside = GetLevelBrightBlock(x, y, z, lo);
            // яркость какая по итогу
            byte lightNew = lightBeside > lightBlock ? lightBeside : lightBlock;

            // SET яркость блока
            chunkStorage.lightBlock[index] = lightNew;
            axisX0 = axisX1 = x;
            axisY0 = axisY1 = y;
            axisZ0 = axisZ1 = z;
            indexBegin = indexEnd = 0;
            countBlock = 0;

            if (lightOld < lightNew)
            {
                // Осветлить
                arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | lightNew << 20);
                BrighterLightBlock();
            }
            else if (lightOld > lightNew)
            {
                // Затемнить
                arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | lightOld << 20);
                DarkenLightBlock();
                BrighterLightBlock();
            }
            int c1 = countBlock;
            // Проверка яркости неба
            chunk.Light.CheckLightSky(x, y, z, lo);
            ModifiedRender();
            long le = stopwatch.ElapsedTicks;
            stopwatch.Stop();
            debugStr = string.Format("Count B/S: {1}/{2} Light: {0:0.00}ms",
                le / (float)MvkStatic.TimerFrequency, c1, countBlock - c1);
        }

        /// <summary>
        /// Проверка блока небесного освещения
        /// </summary>
        /// <param name="x">глобальная позиция блока x</param>
        /// <param name="y">глобальная позиция блока y</param>
        /// <param name="z">глобальная позиция блока z</param>
        /// <param name="lo">прозрачности и излучаемости освещения</param>
        public void CheckLightSky(int x, int y, int z, byte lo)
        {
            // Один блок проверки, не видящий неба
            byte lightOld = chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
            // яркость от соседних блоков
            byte lightBeside = GetLevelBrightSky(x, y, z, lo);

            if (lightOld < lightBeside)
            {
                // Осветлить
                BrighterLightSky(x, y, z, lo);
            }
            else if (lightOld > lightBeside)
            {
                // Затемнить
                DarkenLightSky(x, y, z);
            }
        }

        /// <summary>
        /// Осветление столба блоков неба
        /// </summary>
        public void BrighterLightColumnSky(int x, int y0, int z, int y1)
        {
            indexBegin = indexEnd = 0;
            for (int y = y0; y < y1; y++)
            {
                chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] = 15;
                arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | 15 << 20);
            }
            BrighterLightSky();
        }

        /// <summary>
        /// Затемнение столба блоков неба
        /// </summary>
        public void DarkenLightColumnSky(int x, int y0, int z, int y1)
        {
            indexBegin = indexEnd = 0;
            for (int y = y0; y < y1; y++)
            {
                byte l = chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
                chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] = 0;
                arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | l << 20);
            }
            DarkenLightSky();
            BrighterLightSky();
        }

        /// <summary>
        /// Проверяем освещение блоков при старте
        /// </summary>
        public void CheckBrighterLightBlocks(vec3i[] lightBlocks)
        {
            countBlock = 0;
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            int i, x, y, z, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            vec3i pos = lightBlocks[0];

            x = pos.x + bOffsetX;
            y = pos.y;
            z = pos.z + bOffsetZ;
            axisX0 = axisX1 = x;
            axisY0 = axisY1 = y;
            axisZ0 = axisZ1 = z;
            indexBegin = indexEnd = 0;
            countBlock = 0;

            for (i = 0; i < lightBlocks.Length; i++)
            {
                pos = lightBlocks[i];
                x = pos.x + bOffsetX;
                y = pos.y;
                z = pos.z + bOffsetZ;
                if (x < axisX0) axisX0 = x; else if (x > axisX1) axisX1 = x;
                if (y < axisY0) axisY0 = y; else if (y > axisY1) axisY1 = y;
                if (z < axisZ0) axisZ0 = z; else if (z > axisZ1) axisZ1 = z;

                yco = y >> 4;
                xco = (x >> 4) - chBeginX;
                zco = (z >> 4) - chBeginY;
                chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                chunkStorage = chunkCache.StorageArrays[yco];
                indexBlock = (y & 15) << 8 | (z & 15) << 4 | (x & 15);
                if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[indexBlock] & 0xFFF];
                else lo = 0;

                arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | lo << 20);
            }
            
            BrighterLightBlock();
            ModifiedRender();
        }

        /// <summary>
        /// Небесное боковое освещение
        /// </summary>
        public void HeavenSideLighting()
        {
            countBlock = 0;
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            int i, x, y, z, yh, yh2, xReal, zReal;
            vec2i vec;
            // координата блока
            int x0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            int xo, yo, zo;
            int yDown, yUp, yMin2, yMax2;
            byte lo;

            bool begin = true;

            for (x = 0; x < 16; x++)
            {
                xReal = bOffsetX + x;
                for (z = 0; z < 16; z++)
                {
                    zReal = bOffsetZ + z;
                    // Определяем наивысшый непрозрачный блок текущего ряда
                    yh = chunk.Light.GetHeight(x, z);
                    yMin[0] = yMin[1] = yMin[2] = yMin[3] = 256;
                    yMax[0] = yMax[1] = yMax[2] = yMax[3] = 0;
                    for (i = 0; i < 4; i++) // цикл сторон блока по горизонту
                    {
                        // Позиция соседнего блока, глобальные координаты
                        vec = MvkStatic.AreaOne4[i];
                        x0 = vec.x + xReal;
                        z0 = vec.y + zReal;
                        xco = (x0 >> 4) - chBeginX;
                        zco = (z0 >> 4) - chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                        // Позиция соседнего блока, координаты чанка
                        xo = x0 & 15;
                        zo = z0 & 15;
                        // Определяем наивысшый непрозрачный блок
                        yh2 = chunkCache.Light.GetHeight(xo, zo);

                        // Если соседний блок выше, начинаем обработку
                        if (yh < yh2)
                        {
                            // Координата Y от которой анализируем, на блок выше вверхней, так-как блок нам не интересен
                            yDown = yh;// + 1;
                            // Координата Y до которой анализируем, на блок ниже, так-как нам надо найти ущелие, а блок является перекрытием
                            yUp = yh2 - 1;
                            // Если нижняя координата ниже вверхней или равны, начинаем анализ
                            if (yDown <= yUp)
                            {
                                for (y = yDown; y <= yUp; y++) // цикл высоты
                                {
                                    yo = y;
                                    chunkStorage = chunkCache.StorageArrays[yo >> 4];
                                    indexBlock = (yo & 15) << 8 | zo << 4 | xo;
                                    if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[indexBlock] & 0xFFF];
                                    else lo = 0;
                                    if ((lo >> 4) < 14)
                                    {
                                        // Если блок прозрачный меняем высоты
                                        if (yMin[i] > y) yMin[i] = y;
                                        if (yMax[i] < y) yMax[i] = y;
                                    }
                                }
                            }
                        }
                    }
                    // Готовимся проверять высоты для изменения
                    yMin2 = 256;
                    yMax2 = 0;
                    for (i = 0; i < 4; i++)
                    {
                        if (yMin[i] < yMin2) yMin2 = yMin[i];
                        if (yMax[i] > yMax2) yMax2 = yMax[i];
                    }
                    // Если перепад высот имеется, запускаем правку небесного освещения ввиде столба
                    if (yMin2 != 256)
                    {
                        // Тест столба для визуализации ввиде стекла
                        if (begin)
                        {
                            begin = false;
                            axisX0 = axisX1 = xReal;
                            axisY0 = axisY1 = yMin2;
                            axisZ0 = axisZ1 = zReal;
                            indexBegin = indexEnd = 0;
                        }
                        for (y = yMin2; y <= yMax2; y++)
                        {
                            if (xReal < axisX0) axisX0 = xReal; else if (xReal > axisX1) axisX1 = xReal;
                            if (y < axisY0) axisY0 = y; else if (y > axisY1) axisY1 = y;
                            if (zReal < axisZ0) axisZ0 = zReal; else if (zReal > axisZ1) axisZ1 = zReal;
                            arCache[indexEnd++] = (xReal - bOffsetX + 32 | y << 6 | zReal - bOffsetZ + 32 << 14 | 15 << 20);
                            // World.SetBlockDebug(new BlockPos(xReal, y, zReal), EnumBlock.Glass);
                        }

                    }
                }
            }

            BrighterLightSky();
            ModifiedRender();
        }

        /// <summary>
        /// Обновить кэш соседних чанков, если не загружен хоть один или отсутствует вернёт false
        /// </summary>
        public bool UpChunks()
        {
            vec2i pos;
            for (int i = 0; i < 8; i++)
            {
                pos = MvkStatic.AreaOne8[i];
                pos.x += chBeginX;
                pos.y += chBeginY;
                chunks[i] = World.ChunkPr.GetChunk(pos);
                if (chunks[i] == null || !chunks[i].IsChunkLoaded) return false;
            }
            return true;
        }

        #endregion

        #region Block

        /// <summary>
        /// Осветляем блочный массив
        /// </summary>
        private void BrighterLightBlock()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList, i;
            // вектор стороны
            vec3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // Максимальный псевдо чанк осветленный небом + 1
            int ycMaxSky;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            indexActive = indexBegin == 0 ? 388096 : 0;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (indexEnd - indexBegin > 0)
            {
                for (iList = indexBegin; iList < indexEnd; iList++)
                {
                    listIndex = arCache[iList];
                    x = (listIndex & 63) - 32 + bOffsetX;
                    y = (listIndex >> 6 & 255);
                    z = (listIndex >> 14 & 63) - 32 + bOffsetZ;
                    light = listIndex >> 20 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = MvkStatic.ArraOne3d6[iSide];
                        y0 = y + vec.y;
                        if (y0 < 0 || y0 > ChunkBase.COUNT_HEIGHT_BLOCK) continue;
                        x0 = x + vec.x;
                        z0 = z + vec.z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - chBeginX;
                        zco = (z0 >> 4) - chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[indexBlock] & 0xFFF];
                        else lo = 0;
                        lightNew = chunkStorage.lightBlock[indexBlock];
                        // Определяем яркость, какая должна
                        lightCheck = light - (lo >> 4) - 1;
                        if (lightCheck < 0) lightCheck = 0;
                        if (lightNew >= lightCheck) continue;
                        // Если тикущая темнее, осветляем её
                        lightB = (byte)lightCheck;
                        // Нужна проверка, по созданию новых псевдо чанков освещения
                        // Причём нужна проверка по всей высоте, чтоб не было промежутков
                        if (yco > chunkCache.Light.MaxSkyChunk)
                        {
                            ycMaxSky = chunkCache.Light.MaxSkyChunk + 1;
                            for (i = ycMaxSky; i <= yco; i++)
                            {
                                chunkCache.StorageArrays[i].CheckBrightenBlockSky();
                            }
                            chunkCache.Light.MaxSkyChunk = yco;
                        }
                        chunkStorage.lightBlock[indexBlock] = lightB;
                        countBlock++;

                        if (x0 < axisX0) axisX0 = x0; else if (x0 > axisX1) axisX1 = x0;
                        if (y0 < axisY0) axisY0 = y0; else if (y0 > axisY1) axisY1 = y0;
                        if (z0 < axisZ0) axisZ0 = z0; else if (z0 > axisZ1) axisZ1 = z0;
                        arCache[indexActive++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                    }

                }

                indexEnd = indexActive;
                if (indexBegin == 0)
                {
                    indexBegin = 388096;
                    indexActive = 0;
                }
                else
                {
                    indexBegin = 0;
                    indexActive = 388096;
                }
            }
        }

        /// <summary>
        /// Затемнить блочный массив
        /// </summary>
        private void DarkenLightBlock()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList;
            // вектор стороны
            vec3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            indexActive = indexBegin == 0 ? 388096 : 0;
            // Индекс для массива осветления
            int indexBrighter = 776192;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (indexEnd - indexBegin > 0)
            {
                for (iList = indexBegin; iList < indexEnd; iList++)
                {
                    listIndex = arCache[iList];
                    x = (listIndex & 63) - 32 + bOffsetX;
                    y = (listIndex >> 6 & 255);
                    z = (listIndex >> 14 & 63) - 32 + bOffsetZ;
                    light = listIndex >> 20 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = MvkStatic.ArraOne3d6[iSide];
                        y0 = y + vec.y;
                        if (y0 < 0 || y0 > ChunkBase.COUNT_HEIGHT_BLOCK) continue;
                        x0 = x + vec.x;
                        z0 = z + vec.z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - chBeginX;
                        zco = (z0 >> 4) - chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[indexBlock] & 0xFFF];
                        else lo = 0;
                        lightNew = chunkStorage.lightBlock[indexBlock];
                        lightCheck = lo & 15;
                        // Если фактическая яркость больше уровня прохода,
                        // значит зацепили соседний источник света, 
                        // прерываем с будущей пометкой на проход освещения
                        if (lightNew >= light && lightNew > 1 || lightCheck > 0)
                        {
                            if (lightCheck > 0) lightNew = lightCheck;
                            lightB = (byte)lightNew;
                            arCache[indexBrighter++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                        }
                        // Проход затемнения без else от прошлого, из-за плафонов, они в обоих случаях могут быть
                        if (lightNew < light && lightNew > 0)
                        {
                            lightNew = light - 1;
                            if (lightNew > 0)
                            {
                                lightB = (byte)lightNew;
                                chunkStorage.lightBlock[indexBlock] = (byte)lightCheck;
                                countBlock++;
                                if (x0 < axisX0) axisX0 = x0; else if (x0 > axisX1) axisX1 = x0;
                                if (y0 < axisY0) axisY0 = y0; else if (y0 > axisY1) axisY1 = y0;
                                if (z0 < axisZ0) axisZ0 = z0; else if (z0 > axisZ1) axisZ1 = z0;
                                arCache[indexActive++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                            }
                        }
                    }
                }
                indexEnd = indexActive;
                if (indexBegin == 0)
                {
                    indexBegin = 388096;
                    indexActive = 0;
                }
                else
                {
                    indexBegin = 0;
                    indexActive = 388096;
                }
            }
            indexEnd = indexBrighter;
            indexActive = 0;
            indexBegin = 776192;
        }

        #endregion

        #region Sky

        /// <summary>
        /// Осветляем небесный массив
        /// </summary>
        private void BrighterLightSky()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew, lightCheck;
            int iSide, iList, i;
            // вектор стороны
            vec3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // Максимальный псевдо чанк осветленный небом + 1
            int ycMaxSky;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            indexActive = indexBegin == 0 ? 388096 : 0;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (indexEnd - indexBegin > 0)
            {
                for (iList = indexBegin; iList < indexEnd; iList++)
                {
                    listIndex = arCache[iList];
                    x = (listIndex & 63) - 32 + bOffsetX;
                    y = (listIndex >> 6 & 255);
                    z = (listIndex >> 14 & 63) - 32 + bOffsetZ;
                    light = listIndex >> 20 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = MvkStatic.ArraOne3d6[iSide];
                        y0 = y + vec.y;
                        if (y0 < 0 || y0 > ChunkBase.COUNT_HEIGHT_BLOCK) continue;
                        x0 = x + vec.x;
                        z0 = z + vec.z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - chBeginX;
                        zco = (z0 >> 4) - chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        if (chunkStorage.countData != 0) lo = Blocks.blocksLightOpacity[chunkStorage.data[indexBlock] & 0xFFF];
                        else lo = 0;
                        lightNew = chunkStorage.lightSky[indexBlock];
                        // Определяем яркость, какая должна
                        lightCheck = light - (lo >> 4) - 1;
                        if (lightCheck < 0) lightCheck = 0;
                        if (lightNew >= lightCheck) continue;
                        // Если тикущая темнее, осветляем её
                        lightB = (byte)lightCheck;
                        // Нужна проверка, по созданию новых псевдо чанков освещения
                        // Причём нужна проверка по всей высоте, чтоб не было промежутков
                        if (yco > chunkCache.Light.MaxSkyChunk)
                        {
                            ycMaxSky = chunkCache.Light.MaxSkyChunk + 1;
                            for (i = ycMaxSky; i <= yco; i++)
                            {
                                chunkCache.StorageArrays[i].CheckBrightenBlockSky();
                            }
                            chunkCache.Light.MaxSkyChunk = yco;
                        }
                        chunkStorage.lightSky[indexBlock] = lightB;
                        countBlock++;

                        if (x0 < axisX0) axisX0 = x0; else if (x0 > axisX1) axisX1 = x0;
                        if (y0 < axisY0) axisY0 = y0; else if (y0 > axisY1) axisY1 = y0;
                        if (z0 < axisZ0) axisZ0 = z0; else if (z0 > axisZ1) axisZ1 = z0;
                        arCache[indexActive++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                    }
                }

                indexEnd = indexActive;
                if (indexBegin == 0)
                {
                    indexBegin = 388096;
                    indexActive = 0;
                }
                else
                {
                    indexBegin = 0;
                    indexActive = 388096;
                }
            }
        }

        /// <summary>
        /// Затемнить небесный массив
        /// </summary>
        private void DarkenLightSky()
        {
            ChunkBase chunkCache;
            ChunkStorage chunkStorage;
            byte lightB;
            int lightNew;
            int iSide, iList;
            // вектор стороны
            vec3i vec;
            // координаты с листа
            int x, y, z;
            // свет от листа
            int light;
            // значение индекса
            int listIndex;
            // координата блока
            int x0, y0, z0, indexBlock;
            // смещение координат чанка от стартового
            int xco, zco;
            // псевдочанк
            int yco;
            // значения LightValue и LightOpacity
            byte lo;
            indexActive = indexBegin == 0 ? 388096 : 0;
            // Индекс для массива осветления
            int indexBrighter = 776192;
            bool isAgainstSky;
            // Цикл обхода по древу, уровневым метод (он же ширину (breadth-first search, BFS))
            while (indexEnd - indexBegin > 0)
            {
                for (iList = indexBegin; iList < indexEnd; iList++)
                {
                    listIndex = arCache[iList];
                    x = (listIndex & 63) - 32 + bOffsetX;
                    y = (listIndex >> 6 & 255);
                    z = (listIndex >> 14 & 63) - 32 + bOffsetZ;
                    light = listIndex >> 20 & 15;
                    // соседние блоки
                    for (iSide = 0; iSide < 6; iSide++) // Цикл сторон
                    {
                        vec = MvkStatic.ArraOne3d6[iSide];
                        y0 = y + vec.y;
                        if (y0 < 0 || y0 > ChunkBase.COUNT_HEIGHT_BLOCK) continue;
                        x0 = x + vec.x;
                        z0 = z + vec.z;
                        yco = y0 >> 4;
                        xco = (x0 >> 4) - chBeginX;
                        zco = (z0 >> 4) - chBeginY;
                        chunkCache = (xco == 0 && zco == 0) ? chunk : chunks[MvkStatic.GetAreaOne8(xco, zco)];
                        chunkStorage = chunkCache.StorageArrays[yco];
                        indexBlock = (y0 & 15) << 8 | (z0 & 15) << 4 | (x0 & 15);
                        lightNew = chunkStorage.lightSky[indexBlock];
                        // Если фактическая яркость больше уровня прохода,
                        // значит зацепили соседний источник света, 
                        // прерываем с будущей пометкой на проход освещения
                        isAgainstSky = chunkCache.Light.IsAgainstSky(x0, y0, z0);
                        if ((lightNew >= light && lightNew > 1) || isAgainstSky)
                        {
                            if (isAgainstSky) lightNew = 15;
                            lightB = (byte)lightNew;
                            arCache[indexBrighter++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                        }
                        // Проход затемнения без else от прошлого, из-за плафонов, они в обоих случаях могут быть
                        if (lightNew < light && lightNew > 0)
                        {
                            lightNew = light - 1;
                            if (lightNew > 0)
                            {
                                lightB = (byte)lightNew;
                                chunkStorage.lightSky[indexBlock] = (byte)(isAgainstSky ? 15 : 0);
                                countBlock++;
                                if (x0 < axisX0) axisX0 = x0; else if (x0 > axisX1) axisX1 = x0;
                                if (y0 < axisY0) axisY0 = y0; else if (y0 > axisY1) axisY1 = y0;
                                if (z0 < axisZ0) axisZ0 = z0; else if (z0 > axisZ1) axisZ1 = z0;
                                arCache[indexActive++] = (x0 - bOffsetX + 32 | y0 << 6 | z0 - bOffsetZ + 32 << 14 | lightB << 20);
                            }
                        }
                    }
                }
                indexEnd = indexActive;
                if (indexBegin == 0)
                {
                    indexBegin = 388096;
                    indexActive = 0;
                }
                else
                {
                    indexBegin = 0;
                    indexActive = 388096;
                }
            }
            indexEnd = indexBrighter;
            indexActive = 0;
            indexBegin = 776192;
        }

        /// <summary>
        /// Осветление блока неба
        /// </summary>
        private void BrighterLightSky(int x, int y, int z, byte lo)
        {
            // яркость от соседних блоков
            byte lightBeside = GetLevelBrightSky(x, y, z, lo);
            indexBegin = indexEnd = 0;
            chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] = lightBeside;
            arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | lightBeside << 20);
            BrighterLightSky();
        }

        /// <summary>
        /// Затемнение блок неба
        /// </summary>
        private void DarkenLightSky(int x, int y, int z)
        {
            indexBegin = indexEnd = 0;
            byte light = chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
            chunk.StorageArrays[y >> 4].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)] = 0;
            arCache[indexEnd++] = (x - bOffsetX + 32 | y << 6 | z - bOffsetZ + 32 << 14 | light << 20);
            DarkenLightSky();
            BrighterLightSky();
        }

        #endregion

        /// <summary>
        /// Запустить запрос рендера
        /// </summary>
        private void ModifiedRender()
        {
            if (countBlock <= 1) World.MarkBlockForUpdate(axisX0, axisY0, axisZ0);
            else World.MarkBlockRangeForRenderUpdate(axisX0, axisY0, axisZ0, axisX1, axisY1, axisZ1);
        }

        #region private Get

        /// <summary>
        /// Получить уровень яркости блока, глобальные  координаты
        /// </summary>
        private byte GetLightBlock(int x, int y, int z)
        {
            int yc = y >> 4;
            int xc = (x >> 4) - chBeginX;
            int zc = (z >> 4) - chBeginY;
            if (xc == 0 && zc == 0)
            {
                return chunk.StorageArrays[yc].lightBlock[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
            }
            return chunks[MvkStatic.GetAreaOne8(xc, zc)].StorageArrays[yc].lightBlock[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
        }

        /// <summary>
        /// Получить уровень яркости блока, глобальные  координаты
        /// </summary>
        private byte GetLightSky(int x, int y, int z)
        {
            int yc = y >> 4;
            int xc = (x >> 4) - chBeginX;
            int zc = (z >> 4) - chBeginY;
            if (xc == 0 && zc == 0)
            {
                return chunk.StorageArrays[yc].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
            }
            return chunks[MvkStatic.GetAreaOne8(xc, zc)].StorageArrays[yc].lightSky[(y & 15) << 8 | (z & 15) << 4 | (x & 15)];
        }

        /// <summary>
        /// Возвращает уровень яркости блока, анализируя соседние блоки, глобальные  координаты
        /// </summary>
        private byte GetLevelBrightBlock(int x, int y, int z, byte lo)
        {
            // Количество излучаемого света
            int light = (lo & 0xF);
            // Сколько света вычитается для прохождения этого блока
            int opacity = lo >> 4;
            if (opacity >= 15 && light > 0) opacity = 1;
            if (opacity < 1) opacity = 1;

            // Если блок не проводит свет, значит темно
            if (opacity >= 15) return 0;
            // Если блок яркий выводим значение
            if (light >= 14) return (byte)light;

            vec3i vec;
            int lightNew, y2;
            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            for (int i = 0; i < 6; i++)
            {
                vec = MvkStatic.ArraOne3d6[i];
                y2 = y + vec.y;
                if (y2 >= 0 && y2 <= ChunkBase.COUNT_HEIGHT_BLOCK)
                {
                    lightNew = GetLightBlock(x + vec.x, y2, z + vec.z) - opacity;
                    // Если соседний блок ярче текущего блока
                    if (lightNew > light) light = lightNew;
                    // Если блок яркий выводим значение
                    if (light >= 14) return (byte)light;
                }
            }
            return (byte)light;
        }

        /// <summary>
        /// Возвращает уровень яркости блока, анализируя соседние блоки, глобальные  координаты
        /// </summary>
        public byte GetLevelBrightSky(int x, int y, int z, byte lo)
        {
            // Количество излучаемого света
            int light = 0;
            // Сколько света вычитается для прохождения этого блока
            int opacity = lo >> 4;
            if (opacity >= 15 && light > 0) opacity = 1;
            if (opacity < 1) opacity = 1;

            // Если блок не проводит свет, значит темно
            if (opacity >= 15) return 0;
            // Если блок яркий выводим значение
            if (light >= 14) return (byte)light;

            vec3i vec;
            int lightNew, y2;
            // обрабатываем соседние блоки, вдруг рядом плафон ярче, чтоб не затемнить
            for (int i = 0; i < 6; i++)
            {
                vec = MvkStatic.ArraOne3d6[i];
                y2 = y + vec.y;
                if (y2 >= 0 && y2 <= ChunkBase.COUNT_HEIGHT_BLOCK)
                {
                    lightNew = GetLightSky(x + vec.x, y2, z + vec.z) - opacity;
                    // Если соседний блок ярче текущего блока
                    if (lightNew > light) light = lightNew;
                    // Если блок яркий выводим значение
                    if (light >= 14) return (byte)light;
                }
            }
            return (byte)light;
        }

        #endregion
    }
}
