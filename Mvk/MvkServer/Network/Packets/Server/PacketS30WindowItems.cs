using MvkServer.Item;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет окна списка предметов
    /// </summary>
    public struct PacketS30WindowItems : IPacket
    {

        //private int windowId;  (byte)
        private ItemStack[] stacks;

        //public int GetWindowId() => windowId;
        public ItemStack[] GetStacks() => stacks;

        public PacketS30WindowItems(ItemStack[] stacks)
        {
            //this.windowId = windowId;
            this.stacks = stacks;
        }

        public void ReadPacket(StreamBase stream)
        {
            int count = stream.ReadByte();
            stacks = new ItemStack[count];
            for (int i = 0; i < count; i++)
            {
                stacks[i] = ItemStack.ReadStream(stream);
            }
        }

        public void WritePacket(StreamBase stream)
        {
            int count = stacks.Length;
            stream.WriteByte((byte)count);
            for (int i = 0; i < count; i++)
            {
                ItemStack.WriteStream(stacks[i], stream);
            }
        }
    }
}
