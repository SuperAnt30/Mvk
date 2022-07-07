using MvkServer.World.Block;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// </summary>
    public struct ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка, нижнего блока, т.е. кратно 16. Глобальная координата Y, не чанка
        /// </summary>
        private readonly int yBase;
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        private ushort[,,] data;
        /// <summary>
        /// Освещение блока
        /// 4 bit свет блока и 4 bit свет от неба
        /// </summary>
        private byte[,,] light;
        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        private int countData;
        /// <summary>
        /// было ли заполнения неба
        /// </summary>
        private bool sky;

        public ChunkStorage(int y)
        {
            yBase = y;
            data = null;
            countData = 0;
            light = new byte[16, 16, 16];
            sky = false;
        }

        /// <summary>
        /// Пустой, все блоки воздуха
        /// </summary>
        public bool IsEmptyData() => countData == 0;

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            data = null;
            light = new byte[16, 16, 16];
            countData = 0;
        }

        #region Get

        /// <summary>
        /// Уровень псевдочанка
        /// </summary>
        public int GetYLocation() => yBase;

        /// <summary>
        /// Получить данные всего блока, XYZ 0..15 
        /// </summary>
        public ushort GetData(int x, int y, int z) => data[y, x, z];

        /// <summary>
        /// Получить тип блок по координатам XYZ 0..15 
        /// </summary>
        public int GetEBlock(int x, int y, int z) => data[y, x, z] & 0xFFF;

        /// <summary>
        /// Получить дополнительный параметр блока в 4 бита, XYZ 0..15 
        /// </summary>
        public int GetMetadata(int x, int y, int z) => data[y, x, z] >> 12;

        /// <summary>
        /// Получить блок данных, XYZ 0..15 
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z) 
            => new BlockState(data[y, x, z], light[y, x, z]);

        /// <summary>
        /// Получить байт освещения неба и блока, XYZ 0..15 
        /// </summary>
        public byte GetLightsFor(int x, int y, int z) => light[y, x, z];

        /// <summary>
        /// Получить яркость блока от неба, XYZ 0..15 
        /// </summary>
        public int GetLightSky(int x, int y, int z) => light[y, x, z] & 0xF;
        /// <summary>
        /// Получить яркость блока от блочного освещения, XYZ 0..15 
        /// </summary>
        public int GetLightBlock(int x, int y, int z) => (light[y, x, z] & 0xF0) >> 4;

        #endregion

        #region Set

        /// <summary>
        /// Задать данные блока, XYZ 0..15 
        /// </summary>
        public void SetData(int x, int y, int z, ushort value)
        {
            if ((value & 0xFFF) == 0)
            {
                // воздух, проверка на чистку
                if (countData > 0 && (data[y, x, z] & 0xFFF) != 0)
                {
                    countData--;
                    if (countData == 0) data = null;
                    else data[y, x, z] = value;
                }
            }
            else
            {
                //CheckEmpty();
                if (countData == 0) data = new ushort[16, 16, 16];
                if ((data[y, x, z] & 0xFFF) == 0) countData++;
                data[y, x, z] = value;
            }
        }

        /// <summary>
        /// Задать байт освещения неба и блока
        /// </summary>
        public void SetLightsFor(int x, int y, int z, byte value) => light[y, x, z] = value;
        /// <summary>
        /// Задать яркость неба
        /// </summary>
        public void SetLightSky(int x, int y, int z, byte value) => light[y, x, z] = (byte)(light[y, x, z] & 0xF0 | value);
        /// <summary>
        /// Задать яркость блока
        /// </summary>
        public void SetLightBlock(int x, int y, int z, byte value) => light[y, x, z] = (byte)(value << 4 | light[y, x, z] & 0xF);

        #endregion

        /// <summary>
        /// Пометка обработанного псевдо чанка на небесное освещение
        /// </summary>
        public void Sky() => sky = true;
        /// <summary>
        /// Была ли обработка небесного освещения
        /// </summary>
        public bool IsSky() => sky;

        /// <summary>
        /// Проверка на осветление блоков неба
        /// </summary>
        public void CheckBrightenBlockSky()
        {
            if (!sky)
            {
                sky = true;
                for (int x = 0; x < 16; x++)
                {
                    for (int y = 0; y < 16; y++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            light[y, x, z] = 0xF;
                        }
                    }
                }
            }
        }

        public override string ToString() => "yB:" + yBase + " body:" + countData + " ";
    }
}
