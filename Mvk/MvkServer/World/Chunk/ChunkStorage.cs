using MvkServer.World.Block;
using System;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Псевдо чанк с данными вокселей
    /// 16 * 16 * 16
    /// </summary>
    public class ChunkStorage
    {
        /// <summary>
        /// Уровень псевдочанка
        /// </summary>
        public int YBase { get; protected set; }
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        protected ushort[,,] data = new ushort[16, 16, 16];
        /// <summary>
        /// Освещение блока
        /// 4 bit свет блока и 4 bit свет от неба
        /// </summary>
        protected byte[,,] light = new byte[16, 16, 16];

        public ChunkStorage(int y) => YBase = y;

        #region Прямой доступ к данным для бинарника

        /// <summary>
        /// Получить данные всего блока
        /// </summary>
        public ushort GetData(int x, int y, int z) => data[y, x, z];
        /// <summary>
        /// Задать данные блока
        /// </summary>
        public void SetData(int x, int y, int z, ushort value) => data[y, x, z] = value;

        /// <summary>
        /// Получить байт освещения неба и блока
        /// </summary>
        public byte GetLightsFor(int x, int y, int z) => light[y, x, z];

        /// <summary>
        /// Задать байт  освещения неба и блока
        /// </summary>
        public void SetLightsFor(int x, int y, int z, byte light) => this.light[y, x, z] = light;

        #endregion

        /// <summary>
        /// Получить тип блок по координатам XYZ 0..15 
        /// </summary>
        public EnumBlock GetEBlock(int x, int y, int z) => (EnumBlock)(data[y, x, z] & 0xFFF);

        /// <summary>
        /// Задать тип блок
        /// </summary>
        public void SetEBlock(int x, int y, int z, EnumBlock eBlock) 
            => data[y, x, z] = (ushort)((byte)(data[y, x, z] >> 12) << 12 | (ushort)eBlock);

        /// <summary>
        /// Получить дополнительный параметр блока в 4 бита
        /// </summary>
        public byte GetMetadata(int x, int y, int z) => (byte)(data[y, x, z] >> 12);

        /// <summary>
        /// Задать дополнительный параметр блока в 4 бита
        /// </summary>
        public void SetMetadata(int x, int y, int z, byte value) 
            => data[y, x, z] = (ushort)((byte)(value & 0xF) << 12 | (ushort)(data[y, x, z] & 0xFFF));

        /// <summary>
        /// Получить яркость блока
        /// </summary>
        public byte GetLightFor(int x, int y, int z, EnumSkyBlock type) 
            => type == EnumSkyBlock.Sky ? (byte)(light[y, x, z] & 0xF) : (byte)((light[y, x, z] & 0xF0) >> 4);

        /// <summary>
        /// Задать яркость блока
        /// </summary>
        public void SetLightFor(int x, int y, int z, EnumSkyBlock type, byte light)
        {
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
        /// Удалить данные
        /// </summary>
        public void Delete()
        {
            data = null;
            light = null;
        }
        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            data = new ushort[16, 16, 16];
            light = new byte[16, 16, 16];
        }
    }
}
