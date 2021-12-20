using MvkServer.Management;

namespace MvkServer.World
{
    /// <summary>
    /// Серверный объект мира
    /// </summary>
    public class WorldServer : WorldBase
    {
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }
        /// <summary>
        /// Объект клиентов
        /// </summary>
        public PlayerManager Players { get; protected set; }



        public WorldServer(Server server) : base()
        {
            ServerMain = server;
            Players = new PlayerManager(this);
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            base.Tick();

            //ChunkPr.UnloadQueuedChunks();
        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("Ch {0}-{2} Pl {1}", ChunkPr.Count, Players.PlayerCount, Players.chunkCoordPlayers.Count);
        }
    }
}
