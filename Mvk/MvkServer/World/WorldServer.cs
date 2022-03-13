using MvkServer.Entity;
using MvkServer.Management;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections;

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
        /// <summary>
        /// Трекер сущностей
        /// </summary>
        public EntityTracker Tracker { get; protected set; }

        /// <summary>
        /// Счётчик для обновления сущностей
        /// </summary>
        private int updateEntityTick = 0;

        /// <summary>
        /// Посредник серверного чанка
        /// </summary>
        public ChunkProviderServer ChunkPrServ => ChunkPr as ChunkProviderServer;

        public WorldServer(Server server) : base()
        {
            ServerMain = server;
            ChunkPr = new ChunkProviderServer(this);
            Players = new PlayerManager(this);
            Tracker = new EntityTracker(this);
            Log = ServerMain.Log;
            profiler = new Profiler(Log);
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            profiler.StartSection("PlayersTick");
            Players.Update();

            profiler.EndStartSection("WorldTick");
            base.Tick();

            profiler.EndStartSection("MobSpawner");

            profiler.EndStartSection("ChunkSource");
            ChunkPrServ.UnloadQueuedChunks();
            profiler.EndStartSection("TickBlocks");
            //this.func_147456_g();
            profiler.EndStartSection("ChunkMap");
            Players.UpdatePlayerInstances();
            profiler.EndStartSection("Village");
            //this.villageCollectionObj.tick();
            //this.villageSiege.tick();
            profiler.EndSection();
        }

        /// <summary>
        /// Отметить блок для обновления 
        /// </summary>
        protected override void MarkBlockForUpdate(BlockPos blockPos) => Players.FlagChunkForUpdate(blockPos);

        #region Entity

        /// <summary>
        /// Обновляет (и очищает) объекты и объекты чанка 
        /// </summary>
        public override void UpdateEntities()
        {
            // Для мира где нет игроков или в перспективе если только сервер, 
            // чтоб не запускать обработчик после минуты
            if (Players.IsEmpty())
            {
                if (updateEntityTick++ >= 1200) return;
            }
            else
            {
                updateEntityTick = 0;
            }

            base.UpdateEntities();
        }

        /// <summary>
        /// Обновить трекер сущностей
        /// </summary>
        public void UpdateTrackedEntities()
        {
            profiler.StartSection("Tracker");
            Tracker.UpdateTrackedEntities();
            profiler.EndSection();
        }

        protected override void OnEntityAdded(EntityLiving entity)
        {
            base.OnEntityAdded(entity);
            Tracker.EntityAdd(entity);
        }

        protected override void OnEntityRemoved(EntityLiving entity)
        {
            base.OnEntityRemoved(entity);
            Tracker.EntityRemove(entity);
        }

        #endregion

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public override void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress) 
            => Players.SendBlockBreakProgress(breakerId, pos, progress);


        public int countGetChunck = 0;

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            try
            {
                return string.Format("Ch {0}-{2} Pl {1} @!{4}\r\n{3} ||| E: {5}, Ch: {6}\r\n{7}",
                    ChunkPr.Count, Players.PlayerCount, Players.CountPlayerInstances(), Players.ToStringDebug() // 0 - 3
                    , base.ToStringDebug(), ChunkPr.GetCountEntityDebug(), countGetChunck, Tracker); // 4 - 7
            }
            catch(Exception e)
            {
                return "error: " + e.Message;
            }
        }
    }
}
