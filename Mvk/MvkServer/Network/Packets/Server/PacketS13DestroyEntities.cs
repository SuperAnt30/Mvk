namespace MvkServer.Network.Packets.Server
{
    /// <summary>
    /// Уничтожение сущностей
    /// </summary>
    public struct PacketS13DestroyEntities : IPacket
    {
        private ushort[] ids;

        public ushort[] GetIds() => ids;

        public PacketS13DestroyEntities(ushort[] ids) => this.ids = ids;

        public void ReadPacket(StreamBase stream)
        {
            int count = stream.ReadInt();
            ids = new ushort[count];
            for (int i = 0; i < count; i++)
            {
                ids[i] = stream.ReadUShort();
            }
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteInt(ids.Length);
            for (int i = 0; i < ids.Length; i++)
            {
                stream.WriteUShort(ids[i]);
            }
        }
    }
}
