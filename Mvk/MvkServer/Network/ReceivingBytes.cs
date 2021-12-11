using System;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Объект склейки и возрат при получении полного пакета
    /// </summary>
    internal class ReceivingBytes : SocketHeir
    {
        /// <summary>
        /// Массив байт сборки пакета
        /// </summary>
        protected byte[] BytesCache { get; set; } = new byte[0];
        /// <summary>
        /// Длинна пакета 
        /// </summary>
        protected int BodyLength { get; set; } = 0;
        /// <summary>
        /// Длинна фактическая пакета 
        /// </summary>
        protected int BodyFactLength { get; set; } = 0;

        public ReceivingBytes(Socket workSocket) : base(workSocket) { }

        /// <summary>
        /// Добавить пакет данных
        /// </summary>
        public void Receiving(byte[] dataPacket)
        {
            try
            {
                // с какого индекса начинаем
                int indexRun = 0;

                if (BodyLength == 0)
                {
                    if (dataPacket[0] == 1)
                    {
                        // Начало пакета

                        // длинна пакета
                        BodyLength = BitConverter.ToInt32(dataPacket, 1);

                        // устанавливаем индекс
                        indexRun = 5;

                        BytesCache = new byte[BodyLength];
                        BodyFactLength = 0;
                    }
                    else if (dataPacket[0] == 0)
                    {
                        // Пришел пакет от сервера, надо разорвать связь
                        ServerPacket sp2 = new ServerPacket(WorkSocket, StatusNet.Disconnecting);
                        OnReceive(new ServerPacketEventArgs(sp2));
                        return;
                    }
                    else
                    {
                        throw new Exception("Ошибка в склейке данных [ReceivingBytes:Receiving]");
                    }
                }
                // Определяем тикущую длинну пакета
                int lengthPacket = dataPacket.Length - indexRun;
                // Общая длинна с фактическим паетом
                int length = lengthPacket + BodyFactLength;
                // Если общая длинна больше нужной
                if (length > BodyLength)
                {
                    // Меняем длину пакета с учётом нужного
                    lengthPacket -= (length - BodyLength);
                    length = BodyLength;
                }
                // Объеденяем массив байт пакетов
                if (indexRun == 0)
                {
                    int begin = BodyFactLength;
                    for (int i = 0; i < lengthPacket; i++)
                    {
                        BytesCache[i + begin] = dataPacket[i];
                    }
                }
                else
                {
                    byte[] vs = DivisionAr(dataPacket, indexRun, lengthPacket);
                    for (int i = 0; i < vs.Length; i++)
                    {
                        BytesCache[i] = vs[i];
                    }
                }
                // Добавляем длинну
                BodyFactLength += lengthPacket;

                // Закончен основной пакет
                ServerPacket sp = new ServerPacket(WorkSocket, BytesCache, BodyFactLength);

                // Финиш сборки паета
                if (BodyFactLength == BodyLength)
                {
                    // Отправляем событие получения
                    OnReceive(new ServerPacketEventArgs(sp));

                    // Обнуляем глобальные переменные
                    BytesCache = new byte[0];
                    BodyLength = 0;
                }
                else
                {
                    sp = new ServerPacket(WorkSocket, StatusNet.Loading);
                    // Отправляем запрос сколько отправлено
                    OnReceive(new ServerPacketEventArgs(sp, BodyFactLength, BodyLength));
                }
            }
            catch (Exception e)
            {
                throw e;
                // исключение намекает на разрыв соединения
            }
        }

        /// <summary>
        /// Вернуть массив байт для отправки сообщения
        /// </summary>
        public static byte[] BytesSender(byte[] bytes)
        {
            byte[] ret = new byte[bytes.Length + 5];
            Buffer.BlockCopy(new byte[] { 1 }, 0, ret, 0, 1);
            Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, ret, 1, 4);
            Buffer.BlockCopy(bytes, 0, ret, 5, bytes.Length);
            return ret;
        }

        /// <summary>
        /// Событие, получать
        /// </summary>
        public event ServerPacketEventHandler Receive;
        /// <summary>
        /// Событие получать
        /// </summary>
        protected void OnReceive(ServerPacketEventArgs e) => Receive?.Invoke(this, e);

        /// <summary>
        /// Разделить часть массива
        /// </summary>
        public static byte[] DivisionAr(byte[] first, int indexStart, int indexLength)
        {
            byte[] ret = new byte[indexLength];
            Array.Copy(first, indexStart, ret, 0, indexLength);
            return ret;
        }
    }
}
