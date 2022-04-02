namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет передачи сущности предмета к сущности игрока или моба
    /// </summary>
    public struct PacketS0DCollectItem : IPacket
    {
        private ushort entityId;
        private ushort itemId;

        public ushort GetEntityId() => entityId;
        public ushort GetItemId() => itemId;

        public PacketS0DCollectItem(ushort entityId, ushort itemId)
        {
            this.entityId = entityId;
            this.itemId = itemId;
        }

        public void ReadPacket(StreamBase stream)
        {
            entityId = stream.ReadUShort();
            itemId = stream.ReadUShort();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(entityId);
            stream.WriteUShort(itemId);
        }
    }
}
