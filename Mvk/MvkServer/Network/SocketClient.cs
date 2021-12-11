using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Объект клиента используя сокет
    /// </summary>
    public class SocketClient : SocketBase
    {
        /// <summary>
        /// IP адрес сервера
        /// </summary>
        public IPAddress Ip { get; protected set; }

        /// <summary>
        /// Объект склейки
        /// </summary>
        private ReceivingBytes receivingBytes;

        public SocketClient(IPAddress ip, int port) : base(port) => Ip = ip;

        #region Runing

        /// <summary>
        /// Соединяемся к серверу
        /// </summary>
        public bool Connect()
        {
            if (!IsConnected)
            {
                try
                {
                    WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    WorkSocket.Connect(Ip, Port);
                    SetActiveSocket(WorkSocket);

                    // Соединились
                    ServerPacket sp = new ServerPacket(WorkSocket, StatusNet.Connect);
                    OnReceive(new ServerPacketEventArgs(sp));

                    receivingBytes = new ReceivingBytes(WorkSocket);
                    receivingBytes.Receive += RbReceive;

                    StateObject state = new StateObject(WorkSocket);
                    WorkSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state);

                    return true;
                }
                catch (SocketException e)
                {
                    OnError(new ErrorEventArgs(e));
                }
            }
            return false;
        }

        /// <summary>
        /// Разрываем соединение с сервером
        /// </summary>
        public void Disconnect()
        {
            if (!IsConnected)
            {
                return;
            }
            try
            {
                // Разорвали связь
                ServerPacket sp = new ServerPacket(WorkSocket, StatusNet.Disconnect);
                OnReceive(new ServerPacketEventArgs(sp));

                WorkSocket.Shutdown(SocketShutdown.Both);
                WorkSocket.Close();
                WorkSocket = null;
                receivingBytes = null;
            }
            catch (SocketException e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        #endregion

        /// <summary>
        /// Ответ готовности сообщения
        /// </summary>
        protected new void RbReceive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.Status == StatusNet.Disconnecting)
            {
                Disconnect();
            }
            base.RbReceive(sender, e);
        }

        /// <summary>
        /// Ждём овета от сервера
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar)
        {
            // Получаем состояние объекта и обработчик сокета
            // От асинхронного состояния объекта.
            StateObject state = (StateObject)ar.AsyncState;

            // Проверка если обработчик без связи, прекращаем
            if (!IsConnected)
            {
                return;
            }

            try
            {
                // Чтение данных из клиентского сокета. 
                int bytesRead = WorkSocket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // Если длинны данный больше 0, то обрабатываем данные
                    receivingBytes.Receiving(ReceivingBytes.DivisionAr(state.Buffer, 0, bytesRead));

                    if (WorkSocket == null) return;
                    // Запуск ожидание следующего ответа от клиента
                    if (WorkSocket.Connected)
                    {
                        WorkSocket.BeginReceive(
                            state.Buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReceiveCallback), state
                        );
                    }
                }
                else
                {
                    // Если данные отсутствуют, то разрываем связь
                    OnError(new ErrorEventArgs(new Exception("Если данные отсутствуют, то разрываем связь")));
                    Disconnect();
                }
            }
            catch (Exception e)
            {
                // исключение намекает на разрыв соединения
                Disconnect();
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
            }
        }
    }
}
