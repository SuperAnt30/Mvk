namespace MvkServer.Network.Packets.Client
{
    /// <summary>
    /// Отправляем на сервер анимацию
    /// </summary>
    public struct PacketC0AAnimation : IPacket
    {
        public void ReadPacket(StreamBase stream) { }
        public void WritePacket(StreamBase stream) { }
    }
}
