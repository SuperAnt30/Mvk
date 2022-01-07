namespace MvkServer.Network.Packets
{
    public struct PacketS14TimeUpdate : IPacket
    {
        private uint time;

        public PacketS14TimeUpdate(uint time)
        {
            this.time = time;
        }

        /// <summary>
        /// Время сервера
        /// </summary>
        public uint GetTime() => time;

        public void ReadPacket(StreamBase stream)
        {
            time = stream.ReadUInt();
        }

        public void WritePacket(StreamBase stream)
        {
            stream.WriteUInt(time);
        }
    }
}
