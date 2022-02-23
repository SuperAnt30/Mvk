using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using System;
using System.IO;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Обработка сетевых пакетов
    /// </summary>
    public abstract class ProcessPackets
    {
        private readonly bool isClient;

        protected ProcessPackets(bool client) => isClient = client;

        /// <summary>
        /// Объявление всех объектов пакетов
        /// </summary>
        /// <param name="id">Ключ пакета</param>
        /// <param name="isClient">Пакет от клиента</param>
        protected IPacket Init(byte id)
        {
            if (!isClient)
            {
                // Пакеты от клиента
                switch (id)
                {
                    case 0x00: return new PacketC00Ping();
                    case 0x01: return new PacketC01KeepAlive();

                    case 0x02: return new PacketC02LoginStart();
                    case 0x03: return new PacketC03UseEntity();
                    case 0x04: return new PacketC04PlayerPosition();
                    case 0x05: return new PacketC05PlayerLook();
                    case 0x06: return new PacketC06PlayerPosLook();
                    case 0x0A: return new PacketC0AAnimation();
                    case 0x0C: return new PacketC0CPlayerAction();
                    case 0x15: return new PacketC15ClientSetting();
                    case 0x16: return new PacketC16ClientStatus();
                }
            }
            else
            {
                // Пакеты от сервера
                switch (id)
                {
                    case 0x00: return new PacketS00Pong();
                    case 0x01: return new PacketS01KeepAlive();

                    case 0x02: return new PacketS02JoinGame();
                    case 0x03: return new PacketS03TimeUpdate();
                    case 0x06: return new PacketS06UpdateHealth();
                    case 0x07: return new PacketS07Respawn();
                    case 0x08: return new PacketS08PlayerPosLook();
                    case 0x0B: return new PacketS0BAnimation();
                    case 0x0C: return new PacketS0CSpawnPlayer();
                    case 0x0F: return new PacketS0FSpawnMob();
                    case 0x12: return new PacketS12EntityVelocity();
                    case 0x13: return new PacketS13DestroyEntities();
                    case 0x14: return new PacketS14EntityMotion();
                    case 0x19: return new PacketS19EntityStatus();

                    case 0xF0: return new PacketSF0Connection();
                    case 0xF1: return new PacketSF1Disconnect();
                }
            }

            // Старые
            switch (id)
            {
                case 0x21: return new PacketS21ChunckData();
            }
            return null;
        }

        /// <summary>
        /// Получить id по имени объекта
        /// </summary>
        protected bool IsKey(IPacket packet, string check)
        {
            try
            {
                string hex = packet.ToString().Substring(packet.ToString().Substring(26, 1) == "P" ? 32 : 39, 1);
                return hex == check;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Получить id по имени объекта
        /// </summary>
        public static byte GetId(IPacket packet)
        {
            try
            {
                string hex = packet.ToString().Substring(packet.ToString().Substring(26, 1) == "P" ? 33 : 40, 2);
                return Convert.ToByte(hex, 16);
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        //public void ReceiveBufferClient(byte[] buffer) => ReceivePacket(null, buffer);
        ///// <summary>
        ///// Передача данных для сервера
        ///// </summary>
        //public void ReceiveBufferServer(Socket socket, byte[] buffer) => ReceivePacket(socket, buffer);

        protected void ReceivePacket(Socket socket, byte[] buffer)
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
