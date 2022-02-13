namespace MvkServer.Network.Packets.Server
{
    public struct PacketS03TimeUpdate : IPacket
    {
        private uint time;

        public PacketS03TimeUpdate(uint time)
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
