using MvkServer.Entity;
using System.Collections;

namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Дополнительные данные сущности
    /// </summary>
    public struct PacketS1CEntityMetadata : IPacket
    {
        private ushort id;
        private ArrayList list;

        public ushort GetId() => id;
        public ArrayList GetList() => list;

        public PacketS1CEntityMetadata(ushort id, DataWatcher dataWatcher, bool isAll)
        {
            this.id = id;
            list = isAll ? dataWatcher.GetAllWatched() : dataWatcher.GetChanged();
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
            list = DataWatcher.ReadWatchedListFromPacketBuffer(stream);
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
            DataWatcher.WriteWatchedListToPacketBuffer(list, stream);
        }
    }
}
