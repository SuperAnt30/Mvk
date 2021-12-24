namespace MvkServer.Network.Packets
{
    public struct PacketC13ClientSetting : IPacket
    {
        /// <summary>
        /// Обзор чанков
        /// </summary>
        private int overviewChunk;

        public PacketC13ClientSetting(int overviewChunk)
        {
            this.overviewChunk = overviewChunk;
        }

        public int GetOverviewChunk() => overviewChunk;

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
