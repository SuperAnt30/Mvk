namespace MvkServer.Network.Packets.Client
{
    public struct PacketC15ClientSetting : IPacket
    {
        /// <summary>
        /// Обзор чанков
        /// </summary>
        private int overviewChunk;

        public int GetOverviewChunk() => overviewChunk;

        public PacketC15ClientSetting(int overviewChunk)
        {
            this.overviewChunk = overviewChunk;
        }

        public void ReadPacket(StreamBase stream)
        {
            overviewChunk = stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteByte((byte)overviewChunk);
        }
    }
}
