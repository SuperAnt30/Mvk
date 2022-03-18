namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер выбранный слот
    /// </summary>
    public struct PacketC09HeldItemChange : IPacket
    {
        private int slotId;
        public int GetSlotId() => slotId;

        public PacketC09HeldItemChange(int slotId) => this.slotId = slotId;

        public void ReadPacket(StreamBase stream) => slotId = stream.ReadByte();
        public void WritePacket(StreamBase stream) => stream.WriteByte((byte)slotId);
    }
}
