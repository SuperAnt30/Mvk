using MvkServer.Network.Packets;
using System;
using System.IO;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Обработка сетевых пакетов
    /// </summary>
    public class ProcessPackets
    {
        /// <summary>
        /// Объявление всех объектов пакетов
        /// </summary>
        /// <param name="id">ключ пакета</param>
        protected IPacket Init(byte id)
        {
            switch (id)
            {
                case 0x10: return new PacketS10Connection();
                case 0x11: return new PacketC11LoginStart();
                case 0x12: return new PacketS12Success();
                case 0x13: return new PacketC13ClientSetting();
                case 0x14: return new PacketS14TimeUpdate();
                case 0x15: return new PacketS15Disconnect();
                case 0x20: return new PacketB20Player();
                case 0x21: return new PacketS21ChunckData();
                //case 0x22: return new PacketC22EntityUse();
                //case 0x23: return new PacketS23EntityUse();
                case 0xFF: return new PacketTFFTest();
            }
            return null;
        }

        /// <summary>
        /// Получить id по имени объекта
        /// </summary>
        public static byte GetId(IPacket packet)
        {
            string hex = packet.ToString().Substring(33, 2);
            return Convert.ToByte(hex, 16);
        }

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBufferClient(byte[] buffer) => ReceivePacket(true, null, buffer);
        /// <summary>
        /// Передача данных для сервера
        /// </summary>
        public void ReceiveBufferServer(Socket socket, byte[] buffer) => ReceivePacket(false, socket, buffer);

        protected void ReceivePacket(bool isClient, Socket socket, byte[] buffer)
        {
            IPacket packet = Init(buffer[0]);
            if (packet == null) return;
            try
            {
                using (MemoryStream readStream = new MemoryStream(buffer, 1, buffer.Length - 1))
                {
                    using (StreamBase stream = new StreamBase(readStream))
                    {
                        packet.ReadPacket(stream);
                        if (isClient)
                        {
                            ReceivePacketClient(packet);
                        }
                        else
                        {
                            ReceivePacketServer(socket, packet);
                        }
                    }
                }
            } catch (Exception e)
            {
                Console.WriteLine($"Generic Exception Handler: {e}");
            }
        }

        /// <summary>
        /// Пакет получения для сервера
        /// </summary>
        /// <param name="socket">клиент</param>
        /// <param name="packet">данные пакета</param>
        protected virtual void ReceivePacketServer(Socket socket, IPacket packet) { }
        /// <summary>
        /// Пакет получения для клиента
        /// </summary>
        /// <param name="packet">данные пакета</param>
        protected virtual void ReceivePacketClient(IPacket packet) { }
    }
}
