namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет соединения с сервером
    /// </summary>
    public struct PacketS02JoinGame : IPacket
    {
        private ushort id;
        private string uuid;
        private bool isCreativeMode;

        public ushort GetId() => id;
        public string GetUuid() => uuid;
        public bool IsCreativeMode() => isCreativeMode;

        public PacketS02JoinGame(ushort id, string uuid, bool isCreativeMode)
        {
            this.id = id;
            this.uuid = uuid;
            this.isCreativeMode = isCreativeMode;
        }

        public void ReadPacket(StreamBase stream)
        {
            uuid = stream.ReadString();
            id = stream.ReadUShort();
            isCreativeMode = stream.ReadBool();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteString(uuid);
            stream.WriteUShort(id);
            stream.WriteBool(isCreativeMode);
        }
    }
}
