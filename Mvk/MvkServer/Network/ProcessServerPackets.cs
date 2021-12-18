using MvkServer.Entity.Player;
using MvkServer.Glm;
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
                case 0x20: Packet20((PacketC20Player)packet); break;
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
            ServerMain.World.Players.LoginStart(new EntityPlayerServer(ServerMain, socketCache, packet.GetName()));
        }

        /// <summary>
        /// Пакет положения игрока
        /// </summary>
        protected void Packet20(PacketC20Player packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socketCache);
            if (entityPlayer != null)
            {
                vec2i ch = entityPlayer.HitBox.ChunkPos;
                entityPlayer.HitBox.SetPos(packet.GetPos());

                if (!ch.Equals(entityPlayer.HitBox.ChunkPos))
                {
                    ServerMain.World.Players.UpdateMountedMovingPlayer(entityPlayer);
                }
            }
        }
    }
}
