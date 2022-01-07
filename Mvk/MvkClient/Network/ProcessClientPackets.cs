using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer.Network;
using MvkServer.Network.Packets;
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
                        case 0x20: Packet20((PacketB20Player)packet); break;
                        case 0x21: Packet21((PacketS21ChunckData)packet); break;
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
                ClientMain.TrancivePacket(new PacketC11LoginStart(Setting.Nickname + (ClientMain.IsServerLocalRun() ? "" : "2")));
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
            // отправляем настройки
            ClientMain.TrancivePacket(new PacketC13ClientSetting(Setting.OverviewChunk));

            ClientMain.World.Player.SetUUID(Setting.Nickname, packet.GetUuid());
            ClientMain.World.Player.SetPosBegin(packet.Pos, packet.Yaw, packet.Pitch);

            ClientMain.GameModeBegin();
        }
        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        protected void Packet14(PacketS14TimeUpdate packet)
        {
            ClientMain.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Пакет положения игрока
        /// </summary>
        protected void Packet20(PacketB20Player packet)
        {
            if (!packet.GetRotating())
            {
                ClientMain.World.Player.SetPositionServer(packet.GetPos());
            }
        }

        protected void Packet21(PacketS21ChunckData packet)
        {
            if (packet.IsRemoved())
            {
                ClientMain.World.ChunkPrClient.UnloadChunk(packet.GetPos());
            }
            else
            {
                ChunkRender chunk = ClientMain.World.ChunkPrClient.GetChunkRender(packet.GetPos(), true);
                chunk.SetBinary(packet.GetBuffer(), packet.GetHeight());
                chunk.ModifiedToRender();
            }
        }
    }
}
