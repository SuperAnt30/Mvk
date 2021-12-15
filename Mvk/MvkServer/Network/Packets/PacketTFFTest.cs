namespace MvkServer.Network.Packets
{
    public struct PacketTFFTest : IPacket
    {
        public string Name;

        public PacketTFFTest(string name) => Name = name;

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
