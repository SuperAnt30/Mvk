namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет статуса сущности, умирает, урон и прочее
    /// </summary>
    public struct PacketS19EntityStatus : IPacket
    {
        private ushort id;
        private byte status;

        public ushort GetId() => id;
        public EnumStatus GetStatus() => (EnumStatus)status;

        public PacketS19EntityStatus(ushort id, EnumStatus status)
        {
            this.id = id;
            this.status = (byte)status;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            status = stream.ReadByte();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteByte(status);
        }

        public enum EnumStatus
        {
            /// <summary>
            /// Умирает
            /// </summary>
            Die = 1,
            /// <summary>
            /// Нанесён урон
            /// </summary>
            //Damage = 2
        }
    }
}
