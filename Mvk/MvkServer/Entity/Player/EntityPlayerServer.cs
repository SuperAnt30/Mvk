using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока для сервера
    /// </summary>
    public class EntityPlayerServer : EntityPlayer
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Сетевой сокет клиента
        /// </summary>
        public Socket SocketClient { get; protected set; }

        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        public EntityPlayerServer(Server server, Socket socket, string name)
        {
            ServerMain = server;
            SocketClient = socket;
            Name = name;
            UUID = GetHash(name);
        }

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        protected string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }
    }
}
