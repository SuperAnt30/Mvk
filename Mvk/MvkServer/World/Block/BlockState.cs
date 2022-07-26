using MvkServer.Network;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Бинарные данные блока
    /// </summary>
    public struct BlockState
    {
        /// <summary>
        /// Данные блока
        /// 12 bit Id блока и 4 bit параметр блока
        /// </summary>
        public ushort data;
        /// <summary>
        /// Освещение блочное, 4 bit используется
        /// </summary>
        public byte lightBlock;
        /// <summary>
        /// Освещение небесное, 4 bit используется
        /// </summary>
        public byte lightSky;

        public BlockState(ushort data, byte lightBlock, byte lightSky)
        {
            this.data = data;
            this.lightBlock = lightBlock;
            this.lightSky = lightSky;
        }
        public BlockState(int id, int met, byte lightBlock, byte lightSky)
        {
            data = (ushort)(id & 0xFFF | met << 12);
            this.lightBlock = lightBlock;
            this.lightSky = lightSky;
        }
        public BlockState(EnumBlock eBlock)
        {
            data = (ushort)eBlock;
            lightBlock = 0;
            lightSky = 0;
        }

        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => data == 4096;
        /// <summary>
        /// Пометить пустой блок
        /// </summary>
        public BlockState Empty()
        {
            // id воздуха, но мет данные 1
            data = 4096;
            return this;
        }

        /// <summary>
        /// Получить id
        /// </summary>
        public int Id() => data & 0xFFF;
        /// <summary>
        /// Получить метданные
        /// </summary>
        public int Met() => data >> 12;

        /// <summary>
        /// Получить тип блок
        /// </summary>
        public EnumBlock GetEBlock() => (EnumBlock)(data & 0xFFF);

        /// <summary>
        /// Получить кэш блока
        /// </summary>
        public BlockBase GetBlock() => Blocks.GetBlockCache(GetEBlock());

        /// <summary>
        /// Записать блок в буффер пакета
        /// </summary>
        public void WriteStream(StreamBase stream)
        {
            stream.WriteUShort(data);
            stream.WriteByte(lightBlock);
            stream.WriteByte(lightSky);
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        public void ReadStream(StreamBase stream)
        {
            data = stream.ReadUShort();
            lightBlock = stream.ReadByte();
            lightSky = stream.ReadByte();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockState))
            {
                var vec = (BlockState)obj;
                if (data == vec.data && lightBlock == vec.lightBlock && lightSky == vec.lightSky) return true;
            }
            return false;
        }

        public override int GetHashCode() => data ^ lightBlock ^ lightSky;

        public override string ToString()
        {
            return string.Format("#{0} M:{1}", Id(), Met());
        }
    }
}
