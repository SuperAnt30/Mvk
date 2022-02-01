using MvkClient.Entity;
using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
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
                        case 0x20: Packet20((PacketB20Player)packet); break;
                        case 0x21: Packet21((PacketS21ChunckData)packet); break;
                        //case 0x23: Packet23((PacketS23EntityUse)packet); break;
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

        /// <summary>
        /// Пакет положения игрока
        /// </summary>
        protected void Packet20(PacketB20Player packet)
        {
            byte type = packet.Type();
            if (type == 2 || type == 3)
            {
                ushort id = packet.GetId();
                if (id != 0 && ClientMain.World.Player.Id != id)
                {
                    EntityPlayerMP entity = ClientMain.World.GetPlayerMP(id);
                    if (entity != null)
                    {
                        if (type == 2) entity.SetPositionServer(packet.GetPos(), packet.IsSneaking(), packet.OnGround());
                        else entity.SetRotationServer(packet.GetYawHead(), packet.GetYawBody(), packet.GetPitch());
                    }
                }
            }
        }

        protected void Packet21(PacketS21ChunckData packet) 
            => ClientMain.World.ChunkPrClient.PacketChunckData(packet);

        /// <summary>
        /// Взаимодействие с сущностью
        /// </summary>
        //protected void Packet23(PacketS23EntityUse packet)
        //{
        //    if (packet.GetAction() == PacketC22EntityUse.EnumAction.Push)
        //    {
        //        vec3 vec = packet.GetPos();
        //        vec.y = 0;
        //        vec = vec.normalize() * .25f;
        //        ClientMain.World.Player.MotionPush = vec;
        //    }
        //}
    }
}
