namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет соединения с сервером
    /// </summary>
    public struct PacketS02JoinGame : IPacket
    {
        private ushort id;
        private string uuid;

        public ushort GetId() => id;
        public string GetUuid() => uuid;

        public PacketS02JoinGame(ushort id, string uuid)
        {
            this.id = id;
            this.uuid = uuid;
        }

        public void ReadPacket(StreamBase stream)
        {
            uuid = stream.ReadString();
            id = stream.ReadUShort();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteUShort(id);
        }
    }
}
