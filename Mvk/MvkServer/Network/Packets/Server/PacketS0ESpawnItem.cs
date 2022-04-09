using MvkServer.Entity;
using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Item.List;
using System.Collections;

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
        private ArrayList list;

        public ushort GetEntityId() => id;
        public vec3 GetPos() => pos;
        public bool IsBlock() => isBlock;
        public ushort GetItemId() => itemId;
        public ArrayList GetList() => list;

        public PacketS0ESpawnItem(EntityItem entity)
        {
            id = entity.Id;
            pos = entity.Position;
            list = entity.MetaData.GetAllWatched();
            if (entity.GetEntityItemStack().Item is ItemBlock itemBlock)
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
            pos = new vec3(stream.ReadFloat(), stream.ReadFloat(), stream.ReadFloat());
            isBlock = stream.ReadBool();
            itemId = stream.ReadUShort();
            list = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            stream.WriteFloat(pos.x);
            stream.WriteFloat(pos.y);
            stream.WriteFloat(pos.z);
            stream.WriteBool(isBlock);
            stream.WriteUShort(itemId);
            DataWatcher.WriteWatchedListToPacketBuffer(list, stream);
        }
    }
}
