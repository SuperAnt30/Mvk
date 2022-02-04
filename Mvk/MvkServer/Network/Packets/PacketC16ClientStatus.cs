namespace MvkServer.Network.Packets
{
    public struct PacketC16ClientStatus : IPacket
    {
        /// <summary>
        /// Статус от клиента
        /// </summary>
        private EnumState state;

        public PacketC16ClientStatus(EnumState state) => this.state = state;

        public EnumState GetState() => state;

        public void ReadPacket(StreamBase stream) => state = (EnumState)stream.ReadByte();

        public void WritePacket(StreamBase stream) => stream.WriteByte((byte)state);

        /// <summary>
        /// Варианты действия
        /// </summary>
        public enum EnumState
        {
            Respawn = 0,
            RequestStats = 1
        }
    }
}
