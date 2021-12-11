using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Создания делегата для ServerPacket
    /// </summary>
    public delegate void ServerPacketEventHandler(object sender, ServerPacketEventArgs e);

    /// <summary>
    /// Объект для события серверпакета
    /// </summary>
    public class ServerPacketEventArgs
    {
        /// <summary>
        /// Получить объект сервера пинг
        /// </summary>
        public ServerPacket Packet { get; protected set; }
        /// <summary>
        /// Закончен ли пакет
        /// </summary>
        public bool IsFinished => Leght == 0;
        /// <summary>
        /// Длинна пакета
        /// </summary>
        public int Leght { get; protected set; } = 0;
        /// <summary>
        /// Получено
        /// </summary>
        public int Received { get; protected set; } = 0;

        public ServerPacketEventArgs(Socket handler, StatusNet status)
        {
            Packet = new ServerPacket(handler, status);
        }
        public ServerPacketEventArgs(ServerPacket sp)
        {
            Packet = sp;
        }
        public ServerPacketEventArgs(ServerPacket sp, int received, int leght)
        {
            Packet = new ServerPacket(sp);
            Received = received;
            Leght = leght;
        }
    }
}
