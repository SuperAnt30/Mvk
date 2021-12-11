using MvkClient.Util;
using MvkServer;
using MvkServer.Network;
using System.IO;
using System.Threading;

namespace MvkClient
{
    /// <summary>
    /// Объект отдельного потока сервера или сокета к серверу
    /// </summary>
    public class ThreadServer
    {
        /// <summary>
        /// Объект сервера если по сети
        /// </summary>
        protected Server server;
        /// <summary>
        /// Объект сокета если по сети
        /// </summary>
        protected SocketClient socket;
        /// <summary>
        /// Локальный ли сервер
        /// </summary>
        public bool IsLoacl { get; protected set; } = true;
        /// <summary>
        /// Был ли запуск мира
        /// </summary>
        public bool IsStartWorld { get; protected set; } = false;

        /// <summary>
        /// Запуск по сети
        /// </summary>
        public void StartServerNet(string ip)
        {
            IsStartWorld = true;
            IsLoacl = false;
            OnObjectKeyTick(new ObjectEventArgs(ObjectKey.LoadingStopWorld));

            // По сети сервер
            socket = new SocketClient(System.Net.IPAddress.Parse(ip), 32021);
            socket.ReceivePacket += (sender, e) => OnRecievePacket(e);
            //TODO:: Сделать для ошибки отдельное окно в GUI
            socket.Error += (sender, e) => OnObjectKeyTick(new ObjectEventArgs(ObjectKey.Error, e.GetException().Message));
            socket.Connect();
        }
        /// <summary>
        /// Запуск локального сервера
        /// </summary>
        public void StartServer(int slot) 
        {
            // Локальный сервер
            IsStartWorld = true;
            server = new Server();
            IsLoacl = true;
            server.LoadingTick += (sender, e) => OnObjectKeyTick(new ObjectEventArgs(ObjectKey.LoadStep));
            server.LoadingEnd += (sender, e) => OnObjectKeyTick(new ObjectEventArgs(ObjectKey.LoadingStopWorld));
            server.Stoped += (sender, e) => ThreadServerStoped();
            server.RecievePacket += (sender, e) => OnRecievePacket(e);
            server.LogDebug += (sender, e) => Debug.strServer = e.Text;
            int count = server.Initialize();
            OnObjectKeyTick(new ObjectEventArgs(ObjectKey.LoadingCountWorld, count));
            Thread myThread = new Thread(server.ServerLoop);
            myThread.Start();
        }

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public void TrancivePacket(IPacket packet)
        {
            if (IsStartWorld)
            {
                using (MemoryStream writeStream = new MemoryStream())
                {
                    using (StreamBase stream = new StreamBase(writeStream))
                    {
                        writeStream.WriteByte(packet.Id);
                        packet.WritePacket(stream);
                        byte[] buffer = writeStream.ToArray();
                        if (IsLoacl)
                        {
                            server.LocalReceivePacket(null, buffer);
                        }
                        else
                        {
                            socket.SendPacket(buffer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Выходим с мира
        /// </summary>
        public void ExitingWorld()
        {
            if (IsLoacl)
            {
                // останавливаем мир
                server.ServerStop();
            } else
            {
                // Игра по сети
                socket.Disconnect();
                // TODO:: Тут разрыв сокета если надо, в лог инфу
                // Но это без доп потока, по этому надо быстро делать, без задержек

                // отправляем событие остановки
                ThreadServerStoped();
            }
        }

        /// <summary>
        /// Остановка 
        /// </summary>
        protected void ThreadServerStoped()
        {
            //if (server != null) server = null;
            IsStartWorld = false;
            OnObjectKeyTick(new ObjectEventArgs(ObjectKey.ServerStoped));
        }

        #region Event

        /// <summary>
        /// Событие такта для объекта с ключом
        /// </summary>
        public event ObjectEventHandler ObjectKeyTick;
        protected virtual void OnObjectKeyTick(ObjectEventArgs e) => ObjectKeyTick?.Invoke(this, e);

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        protected void OnRecievePacket(ServerPacketEventArgs e) => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
