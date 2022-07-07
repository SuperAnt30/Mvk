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
        /// Освещение блока
        /// 4 bit свет блока и 4 bit свет от неба
        /// </summary>
        public byte light;

        public BlockState(ushort data, byte light)
        {
            this.data = data;
            this.light = light;
        }
        public BlockState(int id, int met, byte light)
        {
            data = (ushort)(id & 0xFFF | met << 12);
            this.light = light;
        }
        public BlockState(EnumBlock eBlock)
        {
            data = (ushort)eBlock;
            light = 0;
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
            stream.WriteByte(light);
        }

        /// <summary>
        /// Прочесть блок из буффер пакета и занести в чанк
        /// </summary>
        public void ReadStream(StreamBase stream)
        {
            data = stream.ReadUShort();
            light = stream.ReadByte();
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockState))
            {
                var vec = (BlockState)obj;
                if (data == vec.data && light == vec.light) return true;
            }
            return false;
        }

        public override int GetHashCode() => data ^ light;

        public override string ToString()
        {
            return string.Format("#{0} M:{1}", Id(), Met());
        }
    }
}
