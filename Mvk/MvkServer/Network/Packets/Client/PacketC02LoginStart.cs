namespace MvkServer.Network.Packets.Client
{
    public struct PacketC02LoginStart : IPacket
    {
        /// <summary>
        /// Имя игрока
        /// </summary>
        private string name;

        public string GetName() => name;

        public PacketC02LoginStart(string name) => this.name = name;

        public void ReadPacket(StreamBase stream) => name = stream.ReadString();
        public void WritePacket(StreamBase stream) => stream.WriteString(name);
    }
}
