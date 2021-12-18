using MvkServer.Glm;
using MvkServer.World.Chunk;

namespace MvkServer.Network.Packets
{
    public struct PacketS21ChunckData : IPacket
    {
        private vec2i pos;
        private byte[] buffer;

        public PacketS21ChunckData(ChunkBase chunk)
        {
            pos = chunk.Position;

            //TODO:: Надо додумать передовать не весь чанк, где только небо не брать в работу
            //и по параметру псевдо чанков

            // 16 * 16 * 16 * 3 * 16
            buffer = new byte[196608];
            int i = 0;
            for (int sy = 0; sy < 16; sy++)
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

        /// <summary>
        /// Буффер псевдо чанка
        /// </summary>
        public byte[] GetBuffer() => buffer;
        /// <summary>
        /// Высота
        /// </summary>
        public vec2i GetPos() => pos;

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec2i(stream.ReadInt(), stream.ReadInt());
            buffer = stream.ReadBytes(196608);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(pos.x);
            stream.WriteInt(pos.y);
            stream.WriteBytes(buffer);
        }
    }
}
