using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using System.Net.Sockets;
using System.Threading.Tasks;

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

        protected override void ReceivePacketServer(Socket socket, IPacket packet)
        {
            Task.Factory.StartNew(() =>
            {
                switch (GetId(packet))
                {
                    case 0x11: Packet11(socket, (PacketC11LoginStart)packet); break;
                    case 0x13: Packet13(socket, (PacketC13ClientSetting)packet); break;
                    case 0x20: Packet20(socket, (PacketB20Player)packet); break;
                    case 0x22: Packet22(socket, (PacketC22Input)packet); break;
                    case 0xFF:
                        ServerMain.ResponsePacket(socket, new PacketTFFTest("Получил тест: " + ((PacketTFFTest)packet).Name));
                        break;
                }
            });
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        protected void Packet11(Socket socket, PacketC11LoginStart packet)
        {
            ServerMain.World.Players.LoginStart(new EntityPlayerServer(ServerMain, socket, packet.GetName(), ServerMain.World));
        }

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        protected void Packet13(Socket socket, PacketC13ClientSetting packet)
        {
            ServerMain.World.Players.ClientSetting(socket, packet);
        }

        /// <summary>
        /// Пакет положения игрока
        /// </summary>
        protected void Packet20(Socket socket, PacketB20Player packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null)
            {
                switch(packet.Type())
                {
                    case 0: entityPlayer.SetPosition(packet.GetPos()); break;
                    case 1: entityPlayer.SetRotation(packet.GetYaw(), packet.GetPitch()); break;
                }
            }
        }

        /// <summary>
        /// Нажатие клавиш игрока
        /// </summary>
        protected void Packet22(Socket socket, PacketC22Input packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null)
            {
                entityPlayer.Mov.Key(packet.GetKey());
            }
        }
    }
}
