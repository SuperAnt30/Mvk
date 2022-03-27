using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Item.List;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Пакет спавна вещи
    /// </summary>
    public struct PacketS0ESpawnItem : IPacket
    {
        private ushort id;
        private vec3 pos;
        private bool isBlock;
        private ushort itemId;
        private int amount;

        public ushort GetEntityId() => id;
        public vec3 GetPos() => pos;
        public bool IsBlock() => isBlock;
        public int GetAmount() => amount;
        public ushort GetItemId() => itemId;

        public PacketS0ESpawnItem(EntityItem entity)
        {
            id = entity.Id;
            pos = entity.Position;
            amount = entity.Stack.Amount;
            if (entity.Stack.Item is ItemBlock itemBlock)
            {
                isBlock = true;
                itemId = (ushort)itemBlock.Block.EBlock;
            } else
            {
                isBlock = false;
                itemId = 0;
            }
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            amount = stream.ReadByte();
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            isBlock = stream.ReadBool();
            itemId = stream.ReadUShort();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteByte((byte)amount);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteBool(isBlock);
            stream.WriteUShort(itemId);
        }
    }
}
