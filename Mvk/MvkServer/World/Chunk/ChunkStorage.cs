using MvkServer.World.Block;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// y << 8 | z << 4 | x
    /// </summary>
    public class ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка, нижнего блока, т.е. кратно 16. Глобальная координата Y, не чанка
        /// </summary>
        private readonly int yBase;
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        public ushort[] data;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte[] lightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte[] lightSky;

        /// <summary>
        /// Количество блоков не воздуха
        /// </summary>
        public int countData;
        /// <summary>
        /// было ли заполнения неба
        /// </summary>
        public bool sky;

        public ChunkStorage(int y)
        {
            yBase = y;
            data = null;
            countData = 0;
            lightBlock = new byte[4096];
            lightSky = new byte[4096];
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
            lightBlock = new byte[4096];
            lightSky = new byte[4096];
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
        //public ushort GetData(int x, int y, int z) => data[y << 8 | z << 4 | x];

        /// <summary>
        /// Получить тип блок по координатам XYZ 0..15 
        /// </summary>
       // public int GetEBlock(int x, int y, int z) => data[y << 8 | z << 4 | x] & 0xFFF;

        /// <summary>
        /// Получить дополнительный параметр блока в 4 бита, XYZ 0..15 
        /// </summary>
        //public int GetMetadata(int x, int y, int z) => data[y << 8 | z << 4 | x] >> 12;

        /// <summary>
        /// Получить блок данных, XYZ 0..15 
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            int index = y << 8 | z << 4 | x;
            return new BlockState(data[index], lightBlock[index], lightSky[index]);
        }

        /// <summary>
        /// Получить байт освещения неба и блока, XYZ 0..15 
        /// </summary>
        //public byte GetLightsFor(int x, int y, int z) => light[y << 8 | z << 4 | x];

        /// <summary>
        /// Получить яркость блока от неба, XYZ 0..15 
        /// </summary>
        //public int GetLightSky(int x, int y, int z) => light[y << 8 | z << 4 | x] & 0xF;
        /// <summary>
        /// Получить яркость блока от блочного освещения, XYZ 0..15 
        /// </summary>
        //public int GetLightBlock(int x, int y, int z) => (light[y << 8 | z << 4 | x] & 0xF0) >> 4;

        #endregion

        #region Set

        /// <summary>
        /// Задать данные блока, XYZ 0..15 
        /// index = y << 8 | z << 4 | x
        /// </summary>
        public void SetData(int index, ushort value)
        {
            if ((value & 0xFFF) == 0)
            {
                // воздух, проверка на чистку
                if (countData > 0 && (data[index] & 0xFFF) != 0)
                {
                    countData--;
                    if (countData == 0) data = null;
                    else data[index] = value;
                }
            }
            else
            {
                //CheckEmpty();
                if (countData == 0) data = new ushort[4096];
                if ((data[index] & 0xFFF) == 0) countData++;
                data[index] = value;
            }
        }

        /// <summary>
        /// Задать байт освещения неба и блока
        /// </summary>
        //public void SetLightsFor(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = value;
        /// <summary>
        /// Задать яркость неба
        /// </summary>
       // public void SetLightSky(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = (byte)(light[y << 8 | z << 4 | x] & 0xF0 | value);
        /// <summary>
        /// Задать яркость блока
        /// </summary>
      //  public void SetLightBlock(int x, int y, int z, byte value) => light[y << 8 | z << 4 | x] = (byte)(value << 4 | light[y << 8 | z << 4 | x] & 0xF);

        #endregion

        /// <summary>
        /// Проверка на осветление блоков неба
        /// </summary>
        public void CheckBrightenBlockSky()
        {
            if (!sky)
            {
                sky = true;
                for (int i = 0; i < 4096; i++)
                {
                    lightSky[i] = 0xF;
                }
            }
        }

        public override string ToString() => "yB:" + yBase + " body:" + countData + " ";
    }
}
