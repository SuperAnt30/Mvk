using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Объект сервера используя сокет
    /// </summary>
    public class SocketServer : SocketBase
    {
        /// <summary>
        /// Получить истину есть ли соединение
        /// </summary>
        public override bool IsConnected => WorkSocket != null;
        /// <summary>
        /// Колекция объектов ReceivingBytes
        /// </summary>
        private ReceivingBytesCollection clients = new ReceivingBytesCollection();

        public SocketServer(int port) : base(port) { }

        #region Runing

        /// <summary>
        /// Начинаем слушать входящие соединения
        /// </summary>
        public bool Run()
        {
            if (IsConnected) return false;

            try
            {
                // очистили список клиентов
                clients.Clear();

                // Создание сокета сервера
                WorkSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Связываем сокет с конечной точкой
                WorkSocket.Bind(new IPEndPoint(IPAddress.Any, Port));
                // Начинаем слушать входящие соединения
                WorkSocket.Listen(10);

                OnRunned();

                // Запуск ожидание клиента
                WorkSocket.BeginAccept(new AsyncCallback(AcceptCallback), WorkSocket);
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Остановить сервер
        /// </summary>
        public bool Stop()
        {
            if (IsConnected)
            {
                try
                {
                    if (clients.Count > 0)
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            DisconnectHandler(clients[i].WorkSocket, StatusNet.Disconnecting);
                        }
                    }
                    WorkSocket.Close();
                    WorkSocket = null;

                    OnStopping();

                    return true;
                }
                catch (Exception e)
                {
                    OnError(new ErrorEventArgs(e));
                    return false;
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Ждём покуда не получим клиента
        /// </summary>
        private void AcceptCallback(IAsyncResult ar)
        {
            // обрабатываемый сокет
            Socket socket;

            try
            {
                socket = WorkSocket.EndAccept(ar);
            }
            catch (Exception e)
            {
                if (!IsConnected)
                {
                    OnStopped();
                }
                else
                {
                    OnError(new ErrorEventArgs(e));
                }
                return;
            }


            // Создаём объекты склейки и присваиваем ему событие
            ReceivingBytes rb = new ReceivingBytes(socket);
            rb.Receive += RbReceive;

            // Добавляем в массив клиентов
            clients.Add(rb);

            // Получили клиента, оповещаем
            ServerPacket sp = new ServerPacket(socket, StatusNet.Connect);
            OnReceive(new ServerPacketEventArgs(sp));

            StateObject state = new StateObject(socket);

            // Запуск ожидание ответа от клиента
            try
            {
                socket.BeginReceive(
                    state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state
                );
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
            }
            // Запуск ожидание следующего клиента
            try
            {
                WorkSocket.BeginAccept(new AsyncCallback(AcceptCallback), WorkSocket);
            }
            catch (Exception e)
            {
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Ждём овета от клиента
        /// </summary>
        private void ReceiveCallback(IAsyncResult ar)
        {
            // Получаем состояние объекта и обработчик сокета
            // От асинхронного состояния объекта.
            StateObject state = (StateObject)ar.AsyncState;// as StateObject;
            Socket socket = state.WorkSocket;

            // Проверка если обработчик без связи, разрываем с ним связь
            if (!socket.Connected)
            {
                DisconnectHandler(socket, StatusNet.Disconnect);
                return;
            }

            try
            {
                // Чтение данных из клиентского сокета. 
                int bytesRead = socket.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // Если длинна данных больше 0, то обрабатываем данные
                    ReceivingBytes rb = clients.Search(socket);
                    if (rb != null)
                    {
                        rb.Receiving(ReceivingBytes.DivisionAr(state.Buffer, 0, bytesRead));
                    }
                    else
                    {
                        OnError(new ErrorEventArgs(new Exception("Отсутствует объект склейки для данного сокета [ServerSocket:ReceiveCallback]")));
                    }

                    // Запуск ожидание следующего ответа от клиента
                    socket.BeginReceive(
                        state.Buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReceiveCallback), state
                    );
                }
                else
                {
                    // Если данные отсутствуют, то разрываем связь
                    DisconnectHandler(socket, StatusNet.Disconnect);
                }
            }
            catch (Exception e)
            {
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
            }
        }

        /// <summary>
        /// Разрываем соединение с текущим обработчиком
        /// </summary>
        private void DisconnectHandler(Socket socket, StatusNet status)
        {
            ServerPacket sp = new ServerPacket(socket, status);
            clients.Remove(socket);
            try { socket.Send(new byte[] { 0 }); } catch { } // защита от вылета сервера
            OnReceive(new ServerPacketEventArgs(sp));
        }

        #region Event

        /// <summary>
        /// Событие, запущен
        /// </summary>
        public event EventHandler Runned;
        /// <summary>
        /// Событие запущен
        /// </summary>
        protected void OnRunned() => Runned?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие, остановливание
        /// </summary>
        public event EventHandler Stopping;
        /// <summary>
        /// Событие остановливание
        /// </summary>
        protected void OnStopping() => Stopping?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие, остановлен
        /// </summary>
        public event EventHandler Stopped;
        /// <summary>
        /// Событие остановлен
        /// </summary>
        protected void OnStopped() => Stopped?.Invoke(this, new EventArgs());

        #endregion
    }
}
