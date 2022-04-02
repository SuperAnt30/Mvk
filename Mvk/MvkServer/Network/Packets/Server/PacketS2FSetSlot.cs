using MvkServer.Item;

namespace MvkServer.Network.Packets.Server
{
    public struct PacketS2FSetSlot : IPacket
    {
        //private int windowId;  (byte)
        private int slot;
        private ItemStack itemStack;

        //public int GetWindowId() => windowId;
        public int GetSlot() => slot;
        public ItemStack GetItemStack() => itemStack;

        public PacketS2FSetSlot(int slot, ItemStack itemStack)
        {
            //this.windowId = windowId;
            this.slot = slot;
            this.itemStack = itemStack;
        }

        public void ReadPacket(StreamBase stream)
        {
            //windowId = stream.ReadByte();
            slot = stream.ReadByte();
            itemStack = ItemStack.ReadStream(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            //stream.WriteByte((byte)windowId);
            stream.WriteByte((byte)slot);
            ItemStack.WriteStream(itemStack, stream);
        }
    }
}
