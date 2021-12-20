namespace MvkServer.Network.Packets
{
    public struct PacketC11LoginStart : IPacket
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        private string name;
        /// <summary>
        /// Обзор чанков
        /// </summary>
        private int overviewChunk;

        public PacketC11LoginStart(string name, int overviewChunk)
        {
            this.name = name;
            this.overviewChunk = overviewChunk;
        }

        public string GetName() => name;
        public int GetOverviewChunk() => overviewChunk;

        public void ReadPacket(StreamBase stream)
        {
            name = stream.ReadString();
            overviewChunk = stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(name);
            stream.WriteByte((byte)overviewChunk);
        }
    }
}
