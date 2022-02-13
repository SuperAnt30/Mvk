namespace MvkServer.Network.Packets.Client
{
    public struct PacketC01KeepAlive : IPacket
    {
        private uint time;

        public uint GetTime() => time;

        public PacketC01KeepAlive(uint time) => this.time = time;

        public void ReadPacket(StreamBase stream) => time = stream.ReadUInt();
        public void WritePacket(StreamBase stream) => stream.WriteUInt(time);
    }
}
