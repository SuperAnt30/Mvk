using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
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

        private long networkTickCount;
        private long lastPingTime;
        private long lastSentPingPacket;
        private uint pingKeySend;

        public ProcessServerPackets(Server server) : base(false) => ServerMain = server;

        /// <summary>
        /// Передача данных для сервера
        /// </summary>
        public void ReceiveBuffer(Socket socket, byte[] buffer) => ReceivePacket(socket, buffer);

        protected override void ReceivePacketServer(Socket socket, IPacket packet)
        {
            Task.Factory.StartNew(() =>
            {
                switch (GetId(packet))
                {
                    case 0x00: Handle00Ping(socket, (PacketC00Ping)packet); break;
                    case 0x01: Handle01KeepAlive(socket, (PacketC01KeepAlive)packet); break;
                    case 0x02: Handle02LoginStart(socket, (PacketC02LoginStart)packet); break;
                    case 0x03: Handle03UseEntity(socket, (PacketC03UseEntity)packet); break;
                    case 0x04: Handle04PlayerPosition(socket, (PacketC04PlayerPosition)packet); break;
                    case 0x05: Handle05PlayerLook(socket, (PacketC05PlayerLook)packet); break;
                    case 0x06: Handle06PlayerPosLook(socket, (PacketC06PlayerPosLook)packet); break;
                    case 0x0A: Handle0AAnimation(socket, (PacketC0AAnimation)packet); break;
                    case 0x0C: Handle0CPlayerAction(socket, (PacketC0CPlayerAction)packet); break;
                    case 0x15: Handle15ClientSetting(socket, (PacketC15ClientSetting)packet); break;
                    case 0x16: Handle16ClientStatus(socket, (PacketC16ClientStatus)packet); break;
                }
            });
        }

        /// <summary>
        /// Такт сервера
        /// </summary>
        public void Update()
        {
            networkTickCount++;
            if (networkTickCount - lastSentPingPacket > 40)
            {
                lastSentPingPacket = networkTickCount;
                lastPingTime = ServerMain.Time();
                pingKeySend = (uint)lastPingTime;
                ServerMain.ResponsePacketAll(new PacketS01KeepAlive(pingKeySend));
            }
        }

        /// <summary>
        /// Ping-pong
        /// </summary>
        private void Handle00Ping(Socket socket, PacketC00Ping packet) => ServerMain.ResponsePacket2(socket, new PacketS00Pong(packet.GetClientTime()));

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(Socket socket, PacketC01KeepAlive packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (packet.GetTime() == pingKeySend && entityPlayer != null)
            {
                entityPlayer.SetPing(lastPingTime);
            }
        }

        /// <summary>
        /// Пакет проверки логина
        /// </summary>
        private void Handle02LoginStart(Socket socket, PacketC02LoginStart packet)
        {
            ServerMain.World.Players.LoginStart(new EntityPlayerServer(ServerMain, socket, packet.GetName(), ServerMain.World));
        }

        /// <summary>
        /// Взаимодействие с сущностью
        /// </summary>
        private void Handle03UseEntity(Socket socket, PacketC03UseEntity packet)
        {
            EntityLiving entity = ServerMain.World.LoadedEntityList.Get(packet.GetId());
            //EntityPlayerServer entity = ServerMain.World.Players.GetPlayer(packet.GetId());

            if (entity != null)
            {
                // Урон
                if (packet.GetAction() == PacketC03UseEntity.EnumAction.Attack)
                {
                    float damage = 1f;
                    entity.SetHealth(entity.Health - damage);
                    vec3 vec = packet.GetVec() * .5f;
                    vec.y = .84f;
                    if (entity is EntityPlayerServer)
                    {
                        ((EntityPlayerServer)entity).SendPacket(new PacketS12EntityVelocity(entity.Id, vec));
                    } else
                    {
                        entity.MotionPush = vec;
                    }
                    ResponseHealth(entity);
                }
            }
        }

        /// <summary>
        /// Пакет позиции игрока
        /// </summary>
        private void Handle04PlayerPosition(Socket socket, PacketC04PlayerPosition packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetPosition(packet.GetPos());
                entityPlayer.SetSneakOnGround(packet.IsSneaking(), entityPlayer.OnGround);
            }
        }

        /// <summary>
        /// Пакет камеры игрока
        /// </summary>
        private void Handle05PlayerLook(Socket socket, PacketC05PlayerLook packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetRotationHead(packet.GetYaw(), entityPlayer.RotationYaw, packet.GetPitch());
                entityPlayer.SetSneakOnGround(packet.IsSneaking(), entityPlayer.OnGround);
            }
        }

        /// <summary>
        /// Пакет позиции и камеры игрока
        /// </summary>
        private void Handle06PlayerPosLook(Socket socket, PacketC06PlayerPosLook packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null)
            {
                entityPlayer.SetPosition(packet.GetPos());
                entityPlayer.SetRotationHead(packet.GetYaw(), entityPlayer.RotationYaw, packet.GetPitch());
                entityPlayer.SetSneakOnGround(packet.IsSneaking(), entityPlayer.OnGround);
            }
        }

        /// <summary>
        /// Пакет анимации игрока
        /// </summary>
        private void Handle0AAnimation(Socket socket, PacketC0AAnimation packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null) entityPlayer.SwingItem();
        }

        /// <summary>
        /// Пакет действия игрока
        /// </summary>
        private void Handle0CPlayerAction(Socket socket, PacketC0CPlayerAction packet)
        {
            EntityPlayerServer entityPlayer = ServerMain.World.Players.GetPlayer(socket);
            if (entityPlayer != null && packet.GetAction() == PacketC0CPlayerAction.EnumAction.Fall)
            {
                // Падение с высоты
                if (packet.GetParam() >= 5)
                {
                    float damage = packet.GetParam() - 5;
                    entityPlayer.SetHealth(entityPlayer.Health - damage);
                    ResponseHealth(entityPlayer);
                }
            }
        }

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        private void Handle15ClientSetting(Socket socket, PacketC15ClientSetting packet)
        {
            ServerMain.World.Players.ClientSetting(socket, packet);
        }

        /// <summary>
        /// Пакет статуса клиента
        /// </summary>
        private void Handle16ClientStatus(Socket socket, PacketC16ClientStatus packet)
        {
            ServerMain.World.Players.ClientStatus(socket, packet.GetState());
        }

        /// <summary>
        /// Отправить изменение по здоровью
        /// </summary>
        private void ResponseHealth(EntityLiving entity)
        {
            if (entity is EntityPlayerServer)
            {
                ((EntityPlayerServer)entity).SendPacket(new PacketS06UpdateHealth(entity.Health));
            }

            if (entity.Health > 0)
            {
                // Анимация урона
                ServerMain.World.Tracker.SendToAllTrackingEntity(entity, new PacketS0BAnimation(entity.Id,
                    PacketS0BAnimation.EnumAnimation.Hurt));
            } else
            {
                // Начала смерти
                ServerMain.World.Tracker.SendToAllTrackingEntity(entity, new PacketS19EntityStatus(entity.Id,
                    PacketS19EntityStatus.EnumStatus.Die));
            }
        }

        
    }
}
