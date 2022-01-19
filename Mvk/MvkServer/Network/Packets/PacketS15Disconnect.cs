namespace MvkServer.Network.Packets
{
    public struct PacketS15Disconnect : IPacket
    {
        private ushort id;

        public PacketS15Disconnect(ushort id)
        {
            this.id = id;
        }

        /// <summary>
        /// id игрока
        /// </summary>
        public ushort GetId() => id;

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
