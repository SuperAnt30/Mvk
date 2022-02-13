namespace MvkServer.Network.Packets.Server
{
    public struct PacketS01KeepAlive : IPacket
    {
        private uint time;

        public uint GetTime() => time;

        public PacketS01KeepAlive(uint time) => this.time = time;

        public void ReadPacket(StreamBase stream) => time = stream.ReadUInt();

        public void WritePacket(StreamBase stream) => stream.WriteUInt(time);
    }
}
