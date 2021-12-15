namespace MvkServer.Network.Packets
{
    public struct PacketC11LoginStart : IPacket
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        private string name;

        public PacketC11LoginStart(string name) => this.name = name;

        public string GetName() => name;

        public void ReadPacket(StreamBase stream)
        {
            name = stream.ReadString();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(name);
        }
    }
}
