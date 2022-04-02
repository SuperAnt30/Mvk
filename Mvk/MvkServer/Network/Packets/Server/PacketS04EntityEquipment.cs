using MvkServer.Item;

namespace MvkServer.Network.Packets.Server
{
    public struct PacketS04EntityEquipment : IPacket
    {
        private ushort id;
        private int slot;
        private ItemStack itemStack;

        public ushort GetId() => id;
        public int GetSlot() => slot;
        public ItemStack GetItemStack() => itemStack;

        public PacketS04EntityEquipment(ushort id, int slot, ItemStack itemStack)
        {
            this.id = id;
            this.slot = slot;
            this.itemStack = itemStack;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            slot = stream.ReadByte();
            itemStack = ItemStack.ReadStream(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteByte((byte)slot);
            ItemStack.WriteStream(itemStack, stream);
        }
    }
}
