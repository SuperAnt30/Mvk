namespace MvkServer.Network.Packets.Client
{
    public struct PacketC16ClientStatus : IPacket
    {
        /// <summary>
        /// Статус от клиента
        /// </summary>
        private EnumState state;

        public EnumState GetState() => state;

        public PacketC16ClientStatus(EnumState state) => this.state = state;

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
