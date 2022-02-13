namespace MvkServer.Network.Packets.Client
{
    public struct PacketC00Ping : IPacket
    {
        private long clientTime;

        public long GetClientTime() => clientTime;

        public PacketC00Ping(long time) => clientTime = time;

        public void ReadPacket(StreamBase stream) => clientTime = stream.ReadLong();
        public void WritePacket(StreamBase stream) => stream.WriteLong(clientTime);
    }
}
