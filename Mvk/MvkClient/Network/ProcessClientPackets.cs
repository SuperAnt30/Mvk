using MvkClient.Setitings;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.World.Chunk;
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
                switch (GetId(packet))
                {
                    case 0x10: Packet10((PacketS10Connection)packet); break;
                    case 0x12: Packet12((PacketS12Success)packet); break;
                    case 0x21: Packet21((PacketS21ChunckData)packet); break;
                    case 0xFF:
                        PacketTFFTest p1 = (PacketTFFTest)packet;
                        Debug.DStr = p1.Name;
                        break;
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
                
                ClientMain.TrancivePacket(new PacketC11LoginStart(
                    Setting.Nickname + (ClientMain.IsServerLocalRun() ? "" : "2"),
                    Setting.OverviewChunk));
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
            string uuid = packet.GetUuid();

            ClientMain.World.Player.HitBox.SetPos(packet.Pos);// + new MvkServer.Glm.vec3(16, 0, 0));
            ClientMain.World.Player.SetRotation(packet.Yaw, packet.Pitch);

            // Отправляем пакет местоположения игрока, для загрузки клиентских чанков
            ClientMain.TrancivePacket(new PacketC20Player(ClientMain.World.Player.HitBox.Position));

            ClientMain.GameMode(packet.Timer);
        }

        protected void Packet21(PacketS21ChunckData packet)
        {
            if (ClientMain.World != null)
            {
                ChunkBase chunk = ClientMain.World.ChunkPr.LoadNewChunk(packet.GetPos());
                if (packet.IsRemoved())
                {
                    ClientMain.World.ChunkPr.UnloadChunk(packet.GetPos());
                }
                else
                {
                    chunk.SetBinary(packet.GetBuffer(), packet.GetHeight());
                }
            }
        }
    }
}
