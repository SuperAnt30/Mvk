using MvkClient.Setitings;
using MvkServer.Network;
using MvkServer.Network.Packets;

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
            switch(GetId(packet))
            {
                case 0x10: Packet10((PacketS10Connection)packet); break;
                case 0x12: Packet12((PacketS12Success)packet); break;
                case 0xFF:
                    PacketTFFTest p1 = (PacketTFFTest)packet;
                    Debug.DStr = p1.Name;
                    break;
            }
        }

        /// <summary>
        /// Пакет связи
        /// </summary>
        protected void Packet10(PacketS10Connection packet)
        {
            if (packet.IsConnect())
            {
                // connect
                ClientMain.TrancivePacket(new PacketC11LoginStart(Setting.Nickname));
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
            ClientMain.GameMode();
        }
    }
}
