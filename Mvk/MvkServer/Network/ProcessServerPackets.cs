using MvkServer.Network.Packets;
using System.IO;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Обработка серверных пакетов для клиента
    /// </summary>
    public class ProcessServerPackets
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        public ProcessServerPackets(Server server) => ServerMain = server;

        public void LocalReceivePacket(Socket socket, byte[] buffer)
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
                    ReceivePacketNext(socket, packet);
                }
            }
        }

        protected void ReceivePacketNext(Socket socket, IPacket packet)
        {
            switch (packet.Id)
            {
                case 1:
                    ServerMain.ResponsePacket(socket, new PacketTest("Получил тест: " + ((PacketTest)packet).Name));
                    break;
            }
        }
    }
}
