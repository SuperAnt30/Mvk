namespace MvkServer.Network
{
    public interface ITransfer
    {
        /// <summary>
        /// Отправить пакет данных
        /// </summary>
        void SendPacket(byte[] bytes);
        /// <summary>
        /// Событие, получать пакет
        /// </summary>
        event ServerPacketEventHandler ReceivePacket;
    }
}
