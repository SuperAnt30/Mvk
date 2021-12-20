using MvkServer.Glm;
using MvkServer.World.Chunk;

namespace MvkServer.Network.Packets
{
    public struct PacketS21ChunckData : IPacket
    {
        private vec2i pos;
        private byte[] buffer;
        private int height;
        private bool remove;

        public PacketS21ChunckData(ChunkBase chunk)
        {
            pos = chunk.Position;
            remove = false;
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

        public PacketS21ChunckData(vec2i pos)
        {
            this.pos = pos;
            remove = true;
            height = 0;
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
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => remove;

        public void ReadPacket(StreamBase stream)
        {
            remove = stream.ReadBool();
            pos = new vec2i(stream.ReadInt(), stream.ReadInt());
            if (!remove)
            {
                height = stream.ReadByte();
                buffer = stream.ReadBytes(height * 12288);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteBool(remove);
            stream.WriteInt(pos.x);
            stream.WriteInt(pos.y);
            if (!remove)
            {
                stream.WriteByte((byte)height);
                stream.WriteBytes(buffer);
            }
        }
    }
}
