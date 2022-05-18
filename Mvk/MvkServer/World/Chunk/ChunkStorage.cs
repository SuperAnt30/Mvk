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
        public ushort[,,] data;
        /// <summary>
        /// Освещение блока
        /// 4 bit свет блока и 4 bit свет от неба
        /// </summary>
        public byte[,,] light;
        /// <summary>
        /// Есть ли тело
        /// </summary>
        private bool body;

        public int countVoxel;

        public ChunkStorage(int y)
        {
            yBase = y;
            data = null;
            light = null;
            body = false;
            countVoxel = 0;
        }

        private void Set()
        {
            if (data == null)// IsEmpty())
            {
                data = new ushort[16, 16, 16];
                light = new byte[16, 16, 16];
                body = false;
                countVoxel = 0;
                //body = true;
            }
        }

        private void Plus()
        {
            countVoxel++;
            body = true;
        }

        private bool Minus()
        {
            countVoxel--;
            if (countVoxel == 0)
            {
                Clear();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !body;// countVoxel == 0;// !body;

        /// <summary>
        /// Уровень псевдочанка
        /// </summary>
        public int GetYLocation() => yBase;

        #region Прямой доступ к данным для бинарника

        /// <summary>
        /// Внести данные блока
        /// </summary>
        public void Set(int x, int y, int z, BlockState blockState)
        {
            SetData(x, y, z, blockState.data);
            SetLightsFor(x, y, z, blockState.light);
        }

        /// <summary>
        /// Получить данные всего блока
        /// </summary>
        public ushort GetData(int x, int y, int z) => data[y, x, z];
        /// <summary>
        /// Задать данные блока
        /// </summary>
        public void SetData(int x, int y, int z, ushort value)
        {
            if (value == 0)
            {
                // воздух, проверка на чистку
                if (!IsEmpty() && data[y, x, z] != 0)
                {
                    if (!Minus()) data[y, x, z] = value;
                }
            }
            else
            {
                Set();
                if (data[y, x, z] == 0) Plus();
                data[y, x, z] = value;
            }
        }

        /// <summary>
        /// Получить байт освещения неба и блока
        /// </summary>
        public byte GetLightsFor(int x, int y, int z) => light[y, x, z];

        /// <summary>
        /// Задать байт  освещения неба и блока
        /// </summary>
        public void SetLightsFor(int x, int y, int z, byte light)
        {
            Set();
            this.light[y, x, z] = light;
        }

        #endregion

        /// <summary>
        /// Получить тип блок по координатам XYZ 0..15 
        /// </summary>
        public EnumBlock GetEBlock(int x, int y, int z) => (EnumBlock)(data[y, x, z] & 0xFFF);

        /// <summary>
        /// Задать тип блок
        /// </summary>
        public void SetEBlock(int x, int y, int z, EnumBlock eBlock)
        {
            if (eBlock == EnumBlock.Air)
            {
                if (!IsEmpty() && data[y, x, z] != 0)
                {
                    if (!Minus()) data[y, x, z] = (ushort)((byte)(data[y, x, z] >> 12) << 12 | (ushort)eBlock);
                }
            }
            else
            {
                Set();
                if (data[y, x, z] == 0) Plus();
                data[y, x, z] = (ushort)((byte)(data[y, x, z] >> 12) << 12 | (ushort)eBlock);
            }
        }

        /// <summary>
        /// Получить дополнительный параметр блока в 4 бита
        /// </summary>
        public byte GetMetadata(int x, int y, int z) => (byte)(data[y, x, z] >> 12);

        /// <summary>
        /// Задать дополнительный параметр блока в 4 бита
        /// </summary>
        public void SetMetadata(int x, int y, int z, byte value)
        {
            Set();
            data[y, x, z] = (ushort)((byte)(value & 0xF) << 12 | (ushort)(data[y, x, z] & 0xFFF));
        }

        /// <summary>
        /// Получить яркость блока
        /// </summary>
        public byte GetLightFor(int x, int y, int z, EnumSkyBlock type) => GetLightFor(light[y, x, z], type);

        /// <summary>
        /// Получить из байта конкретное освещение
        /// </summary>
        /// <param name="light">байт освещения неба и блока</param>
        public static byte GetLightFor(byte light, EnumSkyBlock type)
            => type == EnumSkyBlock.Sky ? (byte)(light & 0xF) : (byte)((light & 0xF0) >> 4);

        /// <summary>
        /// Задать яркость блока
        /// </summary>
        public void SetLightFor(int x, int y, int z, EnumSkyBlock type, byte light)
        {
            Set();
            byte s, b;

            if (type == EnumSkyBlock.Sky)
            {
                s = light;
                b = (byte)((this.light[y, x, z] & 0xF0) >> 4);
            }
            else
            {
                s = (byte)(this.light[y, x, z] & 0xF);
                b = light;
            }

            this.light[y, x, z] = (byte)(b << 4 | s);
        }

        /// <summary>
        /// Склеить яркость блока
        /// </summary>
        public static byte GlueLightFor(byte lightSky, byte lightBlok) => (byte)(lightBlok << 4 | lightSky);

        /// <summary>
        /// Удалить данные
        /// </summary>
        //public void Delete()
        //{
        //    data = null;
        //    light = null;
        //}
        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            data = null;
            light = null;
            countVoxel = 0;
            body = false;
        }

        /// <summary>
        /// Заполнить весь чанк блоками неба
        /// </summary>
        public void LightSky()
        {
            Set();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        light[y, x, z] = 15;
                    }
                }
            }
        }

        public override string ToString() => yBase + " " + countVoxel;
    }
}
