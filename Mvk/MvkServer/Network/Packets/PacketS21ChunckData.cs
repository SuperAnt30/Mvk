using MvkServer.Glm;
using MvkServer.World.Chunk;

namespace MvkServer.Network.Packets
{
    public struct PacketS21ChunkData : IPacket
    {
        private vec2i pos;
        private byte[] buffer;
        private byte y0;
        private int height;
        private EnumChunk status;

        public PacketS21ChunkData(ChunkBase chunk, int hy)
        {
            pos = chunk.Position;
            status = EnumChunk.One;
            y0 = (byte)hy;
            height = 0;
            // 16 * 16 * 16 * 3 * 16
            buffer = new byte[12288];
            int i = 0;
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        ushort data = chunk.StorageArrays[hy].GetData(x, y, z);
                        buffer[i++] = (byte)(data & 0xFF);
                        buffer[i++] = (byte)(data >> 8);
                        buffer[i++] = chunk.StorageArrays[hy].GetLightsFor(x, y, z);
                    }
                }
            }
        }

        public PacketS21ChunkData(ChunkBase chunk)
        {
            pos = chunk.Position;
            status = EnumChunk.All;
            y0 = 0;
            //TODO:: Надо додумать передовать не весь чанк, где только небо не брать в работу
            //и по параметру псевдо чанков
            // height определяем максимальную высоту
            height = 6;
            // 16 * 16 * 16 * 3 * 16
            buffer = new byte[height * 12288];
            int i = 0;
            for (int sy = 0; sy < height; sy++)
            {
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            ushort data = chunk.StorageArrays[sy].GetData(x, y, z);
                            buffer[i++] = (byte)(data & 0xFF);
                            buffer[i++] = (byte)(data >> 8);
                            buffer[i++] = chunk.StorageArrays[sy].GetLightsFor(x, y, z);
                        }
                    }
                }
            }
        }

        public PacketS21ChunkData(vec2i pos)
        {
            this.pos = pos;
            status = EnumChunk.Remove;
            height = 0;
            y0 = 0;
            buffer = new byte[0];
        }

        /// <summary>
        /// Буффер псевдо чанка
        /// </summary>
        public byte[] GetBuffer() => buffer;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i GetPos() => pos;
        /// <summary>
        /// Высота
        /// </summary>
        public int GetHeight() => height;
        public int GetY() => y0;
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public EnumChunk Status() => status;

        public void ReadPacket(StreamBase stream)
        {
            status = (EnumChunk)stream.ReadByte();
            pos = new vec2i(stream.ReadInt(), stream.ReadInt());
            if (status == EnumChunk.All)
            {
                height = stream.ReadByte();
                buffer = stream.ReadBytes(height * 12288);
            }
            else if (status == EnumChunk.One)
            {
                y0 = stream.ReadByte();
                buffer = stream.ReadBytes(12288);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)status);
            stream.WriteInt(pos.x);
            stream.WriteInt(pos.y);
            if (status == EnumChunk.All)
            {
                stream.WriteByte((byte)height);
                stream.WriteBytes(buffer);
            }
            else if (status == EnumChunk.One)
            {
                stream.WriteByte(y0);
                stream.WriteBytes(buffer);
            }
        }

        public enum EnumChunk
        {
            /// <summary>
            /// Удалить чанк
            /// </summary>
            Remove = 1,
            /// <summary>
            /// Загрузить весь чанк
            /// </summary>
            All = 2,
            /// <summary>
            /// Загрузить псевдо чанк
            /// </summary>
            One = 3
        }
    }
}
