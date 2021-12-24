using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Setitings;
using MvkServer;
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
        public EntityPlayerClient Player { get; protected set; } = new EntityPlayerClient();

        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public ChunkProviderClient ChunkPrClient => ChunkPr as ChunkProviderClient;

        /// <summary>
        /// Мир для рендера и прорисовки
        /// </summary>
        public WorldRenderer WorldRender { get; protected set; }

        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;

        public WorldClient(Client client)
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
            WorldRender = new WorldRenderer(this);
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            uint time = ClientMain.TickCounter;

            if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
            {
                previousTotalWorldTime = time;
                // + 2 к обзору для кэша из-за клона обработки, разных потоков
                ChunkPrClient.FixOverviewChunk(Player, Setting.OverviewChunk + 2); 
            }
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
