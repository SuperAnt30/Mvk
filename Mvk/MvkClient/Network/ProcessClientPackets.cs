using MvkClient.Entity;
using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MvkClient.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets : ProcessPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        public ProcessClientPackets(Client client) => ClientMain = client;

        protected override void ReceivePacketClient(IPacket packet)
        {
            Task.Factory.StartNew(() =>
            {
                byte id = GetId(packet);
                if (id == 0x10)
                {
                    Packet10((PacketS10Connection)packet);
                }
                else if (ClientMain.World != null)
                {
                    switch (id)
                    {
                        case 0x12: Packet12((PacketS12Success)packet); break;
                        case 0x14: Packet14((PacketS14TimeUpdate)packet); break;
                        case 0x15: Packet15((PacketS15Disconnect)packet); break;
                        case 0x17: Packet17((PacketS17Health)packet); break;
                        case 0x20: Packet20((PacketB20Player)packet); break;
                        case 0x21: Packet21((PacketS21ChunckData)packet); break;
                        case 0x23: Packet23((PacketS23EntityUse)packet); break;
                        case 0xFF:
                            PacketTFFTest p1 = (PacketTFFTest)packet;
                            Debug.DStr = p1.Name;
                            break;
                    }
                }
            });
        }

        /// <summary>
        /// Пакет связи
        /// </summary>
        protected void Packet10(PacketS10Connection packet)
        {
            if (packet.IsConnect())
            {
                // connect
                ClientMain.TrancivePacket(new PacketC11LoginStart(Setting.Nickname));// + (ClientMain.IsServerLocalRun() ? "" : "2")));
            }
            else
            {
                // disconnect с причиной
                ClientMain.ExitingWorld(packet.GetCause());
            }
        }
        /// <summary>
        /// Пакет успеха связи
        /// </summary>
        protected void Packet12(PacketS12Success packet)
        {
            if (Setting.Nickname == packet.Name)
            {
                // Основной игрок этого клиента
                ClientMain.World.Player.OnPacketS12Success(packet);
                ClientMain.World.Player.UpFrustumCulling();
                ClientMain.GameModeBegin();
                // отправляем настройки
                ClientMain.TrancivePacket(new PacketC13ClientSetting(Setting.OverviewChunk));
            }
            else
            {
                // Удачный вход сетевого игрока, типа приветствие
                // Или после смерти
                EntityPlayerMP entity = new EntityPlayerMP(ClientMain.World);
                entity.OnPacketS12Success(packet);
                ClientMain.World.SetPlayerMP(entity);
            }
        }
        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        protected void Packet14(PacketS14TimeUpdate packet)
        {
            ClientMain.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Дисконект игрока
        /// </summary>
        protected void Packet15(PacketS15Disconnect packet)
        {
            ClientMain.World.RemovePlayerMP(packet.GetId());
        }

        protected void Packet17(PacketS17Health packet)
        {
            EntityLiving entity;
            if (packet.GetId() == ClientMain.World.Player.Id)
            {
                entity = ClientMain.World.Player;
            } else
            {
                entity = ClientMain.World.GetPlayerMP(packet.GetId());
            }
            if (entity != null)
            {
                entity.SetHealth(packet.GetHealth(), true);
            }
        }

        /// <summary>
        /// Пакет положения игрока
        /// </summary>
        protected void Packet20(PacketB20Player packet)
        {
            byte type = packet.Type();
            if (type > 9)
            {
                ushort id = packet.GetId();
                if (id != 0 && ClientMain.World.Player.Id != id)
                {
                    EntityPlayerMP entity = ClientMain.World.GetPlayerMP(id);
                    if (entity != null)
                    {
                        if (type == 10) entity.SetPositionServer(packet.GetPos(), packet.IsSneaking(), packet.OnGround());
                        else if (type == 11) entity.SetRotationServer(packet.GetYawHead(), packet.GetYawBody(), packet.GetPitch());
                        else if (type == 12)
                        {
                            entity.SwingItem(); // анимация руки игрока
                        }
                    }
                }
            } else if (type == 3)
            {
                ClientMain.World.Player.SetPosition(packet.GetPos());
                ClientMain.World.Player.RespawnClient();
            }
        }

        protected void Packet21(PacketS21ChunckData packet) 
            => ClientMain.World.ChunkPrClient.PacketChunckData(packet);

        /// <summary>
        /// Взаимодействие с сущностью
        /// </summary>
        protected void Packet23(PacketS23EntityUse packet)
        {
            if (packet.GetAction() == PacketS23EntityUse.EnumAction.Push)
            {
                ClientMain.World.Player.MotionPush = packet.GetVec();
            }
        }
    }
}
