namespace MvkServer.Network.Packets.Server
{
    public struct PacketSF0Connection : IPacket
    {
        private string cause;

        /// <summary>
        /// Если причина не указана, то клиент соединился, отправляем пинг, если есть причина, то дисконект
        /// </summary>
        public PacketSF0Connection(string cause) => this.cause = cause;

        public bool IsConnect() => cause == "";
        public string GetCause() => cause;

        public void ReadPacket(StreamBase stream) => cause = stream.ReadString();

        public void WritePacket(StreamBase stream) => stream.WriteString(cause);
    }
}
