using MvkAssets;
using MvkClient.Util;
using MvkServer;
using MvkServer.Network;
using System.IO;
using System.Threading.Tasks;

namespace MvkClient
{
    /// <summary>
    /// Объект локального сервера который может открыт быть для сети
    /// </summary>
    public class LocalServer
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
        /// Открыть сеть
        /// </summary>
        public void OpenNet()
        {
            if (IsLoacl && IsStartWorld)
            {
                server.RunNet();
            }
        }
        /// <summary>
        /// Открыта ли сеть
        /// </summary>
        public bool IsOpenNet() => (IsLoacl && IsStartWorld) ? server.IsRunNet() : false;

        /// <summary>
        /// Запуск по сети
        /// </summary>
        public void StartServerNet(string ip)
        {
            IsStartWorld = true;
            IsLoacl = false;

            // По сети сервер
            socket = new SocketClient(System.Net.IPAddress.Parse(ip), 32021);
            socket.ReceivePacket += (sender, e) => OnRecievePacket(e);
            socket.Receive += Socket_Receive;
            socket.Error += (sender, e) => OnObjectKeyTick(new ObjectKeyEventArgs(ObjectKey.Error, e.GetException().Message));

            //Task.Factory.StartNew(() =>
            //{
            //    Thread.Sleep(500);
            //    socket.Connect();
            //});
            Task.Factory.StartNew(socket.Connect);
        }

        private void Socket_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.Status == StatusNet.Disconnect)
            {
                //Logger.Log("gui.error.clint.disconnect player={0}", server.PlayersManager.GetPlayer(e.Packet.WorkSocket).Name);
                OnObjectKeyTick(new ObjectKeyEventArgs(ObjectKey.Error, Language.T("gui.error.server.disconnect")));
            }
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
            server.LoadingTick += (sender, e) => OnObjectKeyTick(new ObjectKeyEventArgs(ObjectKey.LoadStep));
            server.LoadStepCount += (sender, e) => OnObjectKeyTick(new ObjectKeyEventArgs(ObjectKey.LoadCountWorld, e.Number));
            server.Stoped += (sender, e) => ThreadServerStoped("");
            server.RecievePacket += (sender, e) => OnRecievePacket(e);
            server.LogDebug += (sender, e) => Debug.strServer = e.Text;
            server.LogDebugCh += (sender, e) =>
            {
                // TODO::отладка чанков
                MvkServer.Util.DebugChunk list = (MvkServer.Util.DebugChunk)e.Tag;
                list.listChunkPlayer = Debug.ListChunks.listChunkPlayer;
                Debug.ListChunks = list;
            };
            server.Initialize(slot);
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
                        writeStream.WriteByte(ProcessPackets.GetId(packet));
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
        public void ExitingWorld(string errorNet)
        {
            if (IsLoacl)
            {
                // останавливаем мир
                server.ServerStop();
            } else
            {
                // Игра по сети
                socket.Disconnect();
                // TODO:: Тут разрыв сокета если надо, в лог инфу клиента
                // Но это без доп потока, по этому надо быстро делать, без задержек

                // отправляем событие остановки
                ThreadServerStoped(errorNet);
            }
        }

        /// <summary>
        /// Остановка 
        /// </summary>
        protected void ThreadServerStoped(string errorNet)
        {
            IsStartWorld = false;
            OnObjectKeyTick(new ObjectKeyEventArgs(ObjectKey.ServerStoped, errorNet));
        }

        #region Event

        /// <summary>
        /// Событие такта для объекта с ключом
        /// </summary>
        public event ObjectKeyEventHandler ObjectKeyTick;
        protected virtual void OnObjectKeyTick(ObjectKeyEventArgs e) => ObjectKeyTick?.Invoke(this, e);

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        protected void OnRecievePacket(ServerPacketEventArgs e) => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
