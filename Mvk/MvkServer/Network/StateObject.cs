using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Объект для работы с пакетами сокета
    /// </summary>
    public class StateObject : SocketHeir
    {
        public StateObject(Socket workSocket) : base(workSocket) { }

        /// <summary>
        /// Размер получаемого буфера
        /// </summary>
        public const int BufferSize = 1024;

        /// <summary>
        /// Получить или задать получаемый буфер
        /// </summary>
        public byte[] Buffer { get; set; } = new byte[BufferSize];
    }
}
