using MvkServer.Network;
using MvkServer.Network.Packets;
using System.IO;

namespace MvkClient.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        public ProcessClientPackets(Client client) => ClientMain = client;

        public void LocalReceivePacket(byte[] buffer)
        {
            int id = buffer[0];
            IPacket packet = null;
            switch (id)
            {
                case 1: packet = new PacketTest(); break;
            }
            if (packet == null) return;
            using (MemoryStream readStream = new MemoryStream(buffer, 1, buffer.Length - 1))
            {
                using (StreamBase stream = new StreamBase(readStream))
                {
                    packet.ReadPacket(stream);
                    ReceivePacketNext(packet);
                }
            }
        }

        protected void ReceivePacketNext(IPacket packet)
        {
            switch (packet.Id)
            {
                case 1:
                    PacketTest p1 = (PacketTest)packet;
                    Debug.DStr = p1.Name;
                    break;
            }
        }
    }
}
