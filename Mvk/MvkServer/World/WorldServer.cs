using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Gen;
using MvkServer.Glm;
using MvkServer.Management;
using MvkServer.Network.Packets.Server;
using MvkServer.Sound;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System;

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
        /// <summary>
        /// Зерно генерации случайных чисел
        /// </summary>
        public int Seed { get; private set; } = 2;
        /// <summary>
        /// Объект шумов
        /// </summary>
        public NoiseStorge Noise { get; private set; }

        /// <summary>
        /// Тестовый параметр, мир весь для креатива
        /// </summary>
        public bool IsCreativeMode { get; private set; }

        public WorldServer(Server server, int seed) : base()
        {
            IsRemote = false;
            ServerMain = server;
            ChunkPr = new ChunkProviderServer(this);
            Players = new PlayerManager(this);
            Tracker = new EntityTracker(this);
            Log = ServerMain.Log;
            profiler = new Profiler(Log);
            Seed = seed;
            IsCreativeMode = seed > 3;
            Rand = new Random(seed);
            Noise = new NoiseStorge(this);
        }

        /// <summary>
        /// Игровое время
        /// </summary>
        public override uint GetTotalWorldTime() => ServerMain.TickCounter;

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
            TickBlocks();
            profiler.EndStartSection("ChunkMap");
            Players.UpdatePlayerInstances();
            profiler.EndStartSection("Village");
            //this.villageCollectionObj.tick();
            //this.villageSiege.tick();
            profiler.EndSection();
        }

        private void TickBlocks()
        {
            SetActivePlayerChunksAndCheckLight();
            // цикл активных чанков
            foreach(vec2i chPos in activeChunkSet)
            {
                profiler.StartSection("GetChunk");
                ChunkBase chunk = GetChunk(chPos);
                //if (chunk == null) ChunkPrServ.LoadChunk(chPos);
                
                if (chunk != null)
                {
                    profiler.EndStartSection("TickChunk");
                    chunk.Update();
                    profiler.EndStartSection("TickBlocks");
                    // WorldServer # 423
                    profiler.EndStartSection("StartRecheckGaps");
                    chunk.Light.StartRecheckGaps();
                }
                profiler.EndSection();
            }

        }

        //private void UpdateActiveChunks()
        //{
        //    for (int i = 0; i < PlayerEntities.Count; i++)
        //    {
        //        EntityBase entity = PlayerEntities.GetAt(i);
        //        vec3i pos = entity.GetBlockPos();
        //        if (IsAreaLoaded(pos.x - 16, pos.y - 16, pos.z - 16, pos.x + 16, pos.y + 16, pos.z + 16))
        //        {
        //            ChunkBase chunk = GetChunk(entity.GetChunkPos());
        //            chunk.Update();
        //        }
                
        //    }
            
        //    //ChunkPrServ.
        //}

        /// <summary>
        /// Отметить блок для обновления 
        /// </summary>
        public override void MarkBlockForUpdate(int x, int y, int z) => Players.FlagBlockForUpdate(x, y, z);
        /// <summary>
        /// Отметить  блоки для обновления
        /// </summary>
        public override void MarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            int c0x = (x0 - 1) >> 4;
            int c0y = (y0 - 1) >> 4;
            if (c0y < 0) c0y = 0;
            int c0z = (z0 - 1) >> 4;
            int c1x = (x1 + 1) >> 4;
            int c1y = (y1 + 1) >> 4;
            if (c1y > ChunkBase.COUNT_HEIGHT15) c1y = ChunkBase.COUNT_HEIGHT15;
            int c1z = (z1 + 1) >> 4;
            vec2i ch;
            int x, y, z;
            for (x = c0x; x <= c1x; x++)
            {
                for (z = c0z; z <= c1z; z++)
                {
                    ch = new vec2i(x, z);
                    for (y = c0y; y <= c1y; y++)
                    {
                        Players.FlagChunkForUpdate(ch, y);
                    }
                }
            }
        }

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

        protected override void OnEntityAdded(EntityBase entity)
        {
            base.OnEntityAdded(entity);
            Tracker.EntityAdd(entity);
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected override void OnEntityRemoved(EntityBase entity)
        {
            base.OnEntityRemoved(entity);
            Tracker.UntrackEntity(entity);
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


        /// <summary>
        /// Отправить изменение по здоровью
        /// </summary>
        public void ResponseHealth(EntityLiving entity)
        {
            if (entity is EntityPlayerServer entityPlayerServer)
            {
                entityPlayerServer.SendPacket(new PacketS06UpdateHealth(entity.Health));
            }

            if (entity.Health > 0)
            {
                // Анимация урона
                Tracker.SendToAllTrackingEntity(entity, new PacketS0BAnimation(entity.Id,
                    PacketS0BAnimation.EnumAnimation.Hurt));
            }
            else
            {
                // Начала смерти
                Tracker.SendToAllTrackingEntity(entity, new PacketS19EntityStatus(entity.Id,
                    PacketS19EntityStatus.EnumStatus.Die));
            }
        }

        /// <summary>
        /// Проиграть звуковой эффект, глобальная координата
        /// </summary>
        public override void PlaySound(EntityLiving entity, AssetsSample key, vec3 pos, float volume, float pitch)
        {
            Tracker.SendToAllTrackingEntity(entity, new PacketS29SoundEffect(key, pos, volume, pitch));
        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            try
            {
                string tracker = "";// Tracker.ToString(); 
                return string.Format("Ch {0}-{2} EPl {1} E: {4}\r\n{3} {5}",
                    ChunkPr.Count, Players.PlayerCount, Players.CountPlayerInstances(), Players.ToStringDebug() // 0 - 3
                    , base.ToStringDebug(), tracker); // 4  - 5
            }
            catch(Exception e)
            {
                return "error: " + e.Message;
            }
        }
    }
}
