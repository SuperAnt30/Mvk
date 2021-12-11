using System.Net.Sockets;

namespace MvkServer.Network
{
    public class SocketHeir
    {
        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public virtual bool IsConnected => WorkSocket != null ? WorkSocket.Connected : false;

        /// <summary>
        /// Получить рабочий сокет
        /// </summary>
        public Socket WorkSocket { get; protected set; } = null;

        protected SocketHeir() { }
        public SocketHeir(Socket workSocket) => WorkSocket = workSocket;

        public override string ToString()
        {
            return WorkSocket.RemoteEndPoint.ToString();
        }
    }
}
