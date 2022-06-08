using MvkServer.Glm;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Отправляем клиенту изменённые псевдо чанки
    /// </summary>
    public struct PacketS21ChunkData : IPacket
    {
        private vec2i pos;
        private byte[] buffer;
        private int flagsYAreas;
        private bool biom;

        /// <summary>
        /// Буффер псевдо чанка
        /// </summary>
        public byte[] GetBuffer() => buffer;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i GetPos() => pos;
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public bool IsRemoved() => flagsYAreas == 0;
        /// <summary>
        /// Данные столбца биома, как правило при первой загрузке
        /// </summary>
        public bool IsBiom() => biom;
        /// <summary>
        /// Флаг псевдо чанков
        /// </summary>
        public int GetFlagsYAreas() => flagsYAreas;

        public PacketS21ChunkData(ChunkBase chunk, bool biom, int flagsYAreas)
        {
            pos = chunk.Position;
            this.biom = biom;
            buffer = new byte[0];

            this.flagsYAreas = 0;
            if (flagsYAreas > 0)
            {
                List<ChunkStorage> storages = new List<ChunkStorage>();

                for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
                {
                    if ((!biom || !chunk.StorageArrays[y].IsEmptyData()) && (flagsYAreas & 1 << y) != 0)
                    {
                        this.flagsYAreas |= 1 << y;
                        storages.Add(chunk.StorageArrays[y]);
                    }
                }
                buffer = new byte[CountBuffer() * storages.Count];
                int count = 0;
                while (storages.Count > 0)
                {
                    bool emptyData = storages[0].IsEmptyData();
                   // bool emptyLight = storages[0].IsEmptyLight();
                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                if (emptyData)
                                {
                                    buffer[count++] = 0;
                                    buffer[count++] = 0;
                                   // buffer[count++] = 0;
                                }
                                else
                                {
                                    ushort data = storages[0].GetData(x, y, z);
                                    buffer[count++] = (byte)(data & 0xFF);
                                    buffer[count++] = (byte)(data >> 8);
                                   // buffer[count++] = storages[0].GetLightsFor(x, y, z);
                                }
                                buffer[count++] = storages[0].GetLightsFor(x, y, z);
                            }
                        }
                    }
                    storages.RemoveAt(0);
                }
                if (biom)
                {
                    // добавляем данные биома
                }
            }
        }

        /// <summary>
        /// Получить количество данных в псевдо чанке
        /// </summary>
        private int CountBuffer()
        {
            // количество буфер данных
            int countBuf = 12288; // 16 * 16 * 16 * 3
            int countHeight = biom ? 256 : 0; // 16 * 16 
            return countBuf + countHeight;
        }

        /// <summary>
        /// Количество псевдо чанков по флагу
        /// </summary>
        private int CountChunk()
        {
            int countChunk = 0;
            for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
            {
                if ((flagsYAreas & 1 << y) != 0) countChunk++;
            }
            return countChunk;
        }

        public void ReadPacket(StreamBase stream)
        {
            pos = new vec2i(stream.ReadInt(), stream.ReadInt());
            biom = stream.ReadBool();
            flagsYAreas = stream.ReadUShort();
            if (flagsYAreas > 0)
            {
                buffer = stream.ReadBytes(CountChunk() * CountBuffer());
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(pos.x);
            stream.WriteInt(pos.y);
            stream.WriteBool(biom);
            stream.WriteUShort((ushort)flagsYAreas);
            if (flagsYAreas > 0)
            {
                stream.WriteBytes(buffer);
            }
        }
    }
}
