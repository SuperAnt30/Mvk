namespace MvkServer.Network.Packets.Server
{
    public struct PacketSF1Disconnect : IPacket
    {
        private ushort id;

        public ushort GetId() => id;

        public PacketSF1Disconnect(ushort id)
        {
            this.id = id;
        }

        public void ReadPacket(StreamBase stream)
        {
            id = stream.ReadUShort();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUShort(id);
        }
    }
}
