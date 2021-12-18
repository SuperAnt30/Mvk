using MvkServer.Entity.Player;
using MvkServer.World;

namespace MvkClient.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        /// <summary>
        /// Объект клиента
        /// </summary>
        public EntityPlayer Player { get; protected set; } = new EntityPlayer();

        public WorldClient(Client client) : base()
        {
            ClientMain = client;
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            base.Tick();
        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("t {2} Ch {0}\r\nXYZ {1}", ChunkPr.Count, Player.HitBox.Position, ClientMain.TickCounter / 20);
        }
    }
}
