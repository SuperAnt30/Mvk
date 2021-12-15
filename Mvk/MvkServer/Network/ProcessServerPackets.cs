using MvkServer.Entity.Player;
using MvkServer.Network.Packets;
using System.Net.Sockets;

namespace MvkServer.Network
{
    /// <summary>
    /// Обработка серверных пакетов для клиента
    /// </summary>
    public class ProcessServerPackets : ProcessPackets
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        public ProcessServerPackets(Server server) => ServerMain = server;

        protected Socket socketCache;

        protected override void ReceivePacketServer(Socket socket, IPacket packet)
        {
            socketCache = socket;
            switch (GetId(packet))
            {
                case 0x11: Packet11((PacketC11LoginStart)packet); break;
                case 0xFF:
                    ServerMain.ResponsePacket(socket, new PacketTFFTest("Получил тест: " + ((PacketTFFTest)packet).Name));
                    break;
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        protected void Packet11(PacketC11LoginStart packet)
        {
            ServerMain.PlayersManager.LoginStart(new EntityPlayerServer(ServerMain, socketCache, packet.GetName()));
        }
    }
}
