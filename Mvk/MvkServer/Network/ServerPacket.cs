using System.Net.Sockets;
using System.Text;

namespace MvkServer.Network
{
    /// <summary>
    /// Объект сервера пинга
    /// </summary>
    public class ServerPacket : SocketHeir
    {
        /// <summary>
        /// Получить статус запроса
        /// </summary>
        public StatusNet Status { get; protected set; } = StatusNet.Receive;
        /// <summary>
        /// Получить конечный размер 
        /// </summary>
        public int SizeOriginal => Bytes.Length;
        /// <summary>
        /// Получить сетевой размер 
        /// </summary>
        public int SizeNet { get; protected set; } = 0;
        /// <summary>
        /// Получить массив байтов
        /// </summary>
        public byte[] Bytes { get; protected set; }
        /// <summary>
        /// Получить истину пустого массива байт
        /// </summary>
        public bool BytesIsEmpty => Bytes.Length == 0;

        public ServerPacket(Socket workSocket, StatusNet status) : base(workSocket)
        {
            Status = status;
            Bytes = new byte[0];
        }

        public ServerPacket(Socket workSocket, byte[] bytes, int sizeNet) : base(workSocket)
        {
            Status = StatusNet.Receive;
            Bytes = bytes;
            SizeNet = sizeNet;
        }

        public ServerPacket(ServerPacket sp) : base(sp.WorkSocket)
        {
            Status = StatusNet.Loading;
            Bytes = sp.Bytes;
            SizeNet = sp.SizeNet;
        }

        public ServerPacket(Socket workSocket, byte[] bytes) : this(workSocket, bytes, bytes.Length)
        { }

        /// <summary>
        /// Получить заголовок сетевого потока
        /// </summary>
        public int Handle() => WorkSocket != null ? WorkSocket.Handle.ToInt32() : 0;

        /// <summary>
        /// Получить строку конвектированного массива байт из UTF8
        /// </summary>
        public string BytesToString()
        {
            if (!BytesIsEmpty)
            {
                return Encoding.UTF8.GetString(Bytes);
            }
            return "";
        }

        /// <summary>
        /// Получить статус запроса в виде строки
        /// </summary>
        public string StatusToString() => Status.ToString();
    }
}
