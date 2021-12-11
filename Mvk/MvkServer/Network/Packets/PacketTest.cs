namespace MvkServer.Network.Packets
{
    public struct PacketTest : IPacket
    {
        public byte Id { get { return 1; } }
        public string Name;

        public PacketTest(string name) => Name = name;

        public void ReadPacket(StreamBase stream)
        {
            Name = stream.ReadString();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(Name);
        }
    }
}
