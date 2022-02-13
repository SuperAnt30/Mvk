namespace MvkServer.Network.Packets.Server
{
    public struct PacketS00Pong : IPacket
    {
        private long clientTime;

        public long GetClientTime() => clientTime;

        public PacketS00Pong(long time) => clientTime = time;

        public void ReadPacket(StreamBase stream) => clientTime = stream.ReadLong();

        public void WritePacket(StreamBase stream) => stream.WriteLong(clientTime);
    }
}
