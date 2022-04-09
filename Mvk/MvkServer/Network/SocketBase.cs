using System;
using System.IO;
using System.Net.Sockets;

namespace MvkServer.Network
{
    public class SocketBase : SocketHeir, ITransfer
    {
        /// <summary>
        /// Порт сервера
        /// </summary>
        public int Port { get; protected set; }
        /// <summary>
        /// Получить активный сокет, он же выбранный для сервера
        /// </summary>
        public Socket ActiveSocket { get; protected set; } = null;
        /// <summary>
        /// Запрос завершонный
        /// </summary>
        public bool SendCompleted { get; protected set; } = true;

        protected SocketBase() { }
        public SocketBase(int port) => Port = port;

        /// <summary>
        /// Задать активынй сокет
        /// </summary>
        public void SetActiveSocket(Socket socket) => ActiveSocket = socket;

        /// <summary>
        /// Метод отправки пакетов запроса
        /// </summary>
        /// <param name="bytes">данные в массиве байт</param>
        /// <returns>результат отправки</returns>
        //protected bool SenderOld(Socket socket, byte[] bytes)
        //{
        //    if (!IsConnected || bytes.Length == 0)
        //    {
        //        return false;
        //    }
        //    try
        //    {
        //        // Отправляем пакет
        //        socket.Send(ReceivingBytes.BytesSender(bytes));
        //        return true;
        //    }
        //    catch (Exception e)
        //    {
        //        // Возвращаем ошибку
        //        OnError(new ErrorEventArgs(e));
        //        return false;
        //    }
        //}

        /// <summary>
        /// Метод отправки пакетов запроса
        /// </summary>
        /// <param name="bytes">данные в массиве байт</param>
        /// <returns>результат отправки</returns>
        protected bool Sender(Socket socket, byte[] bytes)
        {
            if (!IsConnected || bytes.Length == 0)
            {
                return false;
            }
            try
            {
                byte[] buffer = ReceivingBytes.BytesSender(bytes);
                SocketAsyncEventArgs e = new SocketAsyncEventArgs();
                e.SetBuffer(buffer, 0, buffer.Length);
                // UNDONE:: SendAsync продумать 
                e.Completed += new EventHandler<SocketAsyncEventArgs>(SendCallback);

                SendCompleted = false;
                // Отправляем асихронный пакет
                socket.SendAsync(e);
                return true;
            }
            catch (Exception e)
            {
                // Возвращаем ошибку
                OnError(new ErrorEventArgs(e));
                return false;
            }
        }

        private void SendCallback(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                OnError(new ErrorEventArgs(new Exception(string.Format("Socket Error: {0}", e.SocketError))));
            }
            SendCompleted = true;
        }

        /// <summary>
        /// Отправить пакет
        /// </summary>
        public virtual void SendPacket(byte[] bytes) => Sender(ActiveSocket, bytes);

        /// <summary>
        /// Ответ готовности сообщения
        /// </summary>
        protected void RbReceive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.Status == StatusNet.Receive)
            {
                OnReceivePacket(e);
            }
            else
            {
                OnReceive(e);
            }
        }

        #region Event

        /// <summary>
        /// Событие, ошибка
        /// </summary>
        public event ErrorEventHandler Error;
        /// <summary>
        /// Событие ошибки
        /// </summary>
        protected void OnError(ErrorEventArgs e) => Error?.Invoke(this, e);

        /// <summary>
        /// Событие, получать
        /// </summary>
        public event ServerPacketEventHandler Receive;
        /// <summary>
        /// Событие получать
        /// </summary>
        protected void OnReceive(ServerPacketEventArgs e) => Receive?.Invoke(this, e);

        /// <summary>
        /// Событие, получать пакет
        /// </summary>
        public event ServerPacketEventHandler ReceivePacket;
        /// <summary>
        /// Событие получать пакет
        /// </summary>
        protected void OnReceivePacket(ServerPacketEventArgs e) => ReceivePacket?.Invoke(this, e);

        #endregion
    }
}
