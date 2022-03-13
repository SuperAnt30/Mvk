using MvkServer.Entity.Mob;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// КОНКРЕТНО пользователи, с которыми работаем по сети. НЕ ОБРАБАТЫВАТЬ сущности это будет в другом месте!!!
    /// </summary>
    public class PlayerManager
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Объект логотладки
        /// </summary>
        private Profiler profiler;

        /// <summary>
        /// Массив игроков EntityPlayerServer
        /// </summary>
        private List<EntityPlayerServer> players = new List<EntityPlayerServer>();
        /// <summary>
        /// Чанки игроков vec2i => PlayerInstance
        /// </summary>
        private Hashtable playerInstances = new Hashtable();
        /// <summary>
        /// Список PlayerInstance которые надо обновить
        /// </summary>
        private List<PlayerInstance> playerInstancesToUpdate = new List<PlayerInstance>();
        /// <summary>
        /// Этот список используется, когда чанк должен быть обработан (каждые 8000 тиков)
        /// </summary>
        private List<PlayerInstance> playerInstanceList = new List<PlayerInstance>();
        /// <summary>
        /// Список игроков которые надо запустить в ближайшем такте
        /// </summary>
        private List<EntityPlayerServer> playerStartList = new List<EntityPlayerServer>();
        /// <summary>
        /// Список игроков котрые надо выгрузить в ближайшем такте
        /// </summary>
        private List<ushort> playerRemoveList = new List<ushort>();

        /// <summary>
        /// время, которое используется, чтобы проверить, следует ли рассчитывать playerInstanceList 
        /// </summary>
        private long previousTotalWorldTime;

        /// <summary>
        /// Последний порядковый номер игрока с момента запуска
        /// </summary>
        private ushort lastPlayerId = 0;

        public PlayerManager(WorldServer worldServer)
        {
            World = worldServer;
            profiler = new Profiler(worldServer.ServerMain.Log);
        }

        #region Player

        /// <summary>
        /// Отправить всем игрокам пакет
        /// </summary>
        public void SendToAll(IPacket packet)
        {
            for (int i = 0; i < players.Count; i++)
            {
                players[i].SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить всем игрокам пакет, которые видят этот чанк
        /// </summary>
        public void SendToAllPlayersWatchingChunk(IPacket packet, vec2i currentChunk)
        {
            PlayerInstance playerInstance = GetPlayerInstance(currentChunk, false);
            if (playerInstance != null)
            {
                playerInstance.SendToAllPlayersWatchingChunk(packet);
            }
        }

        /// <summary>
        /// Проверка на игрока с таким же именем в игре
        /// </summary>
        /// <returns>false - игрок уже играет</returns>
        private bool CheckPlayerAdd(EntityPlayerServer entityPlayer)
        {
            EntityPlayerServer entityPlayerOld = GetPlayerId(entityPlayer.Id);
            return entityPlayerOld == null;
        }

        /// <summary>
        /// Добавить игрока
        /// </summary>
        private void PlayerAdd(EntityPlayerServer entityPlayer)
        {
            // Лог запуска игрока
            World.ServerMain.Log.Log("server.player.entry {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);

            lastPlayerId++;

            // TODO::Тут проверяем место положение персонажа, и заносим при запуске

            entityPlayer.SetEntityId(lastPlayerId);
            SpawnPositionTest(entityPlayer);
            entityPlayer.SetChunkPosManaged(entityPlayer.GetChunkPos());
            //entityPlayer.SetOverviewChunk(entityPlayer.OverviewChunk, 1);
            vec2i posCh = entityPlayer.GetChunkPos();
            
            // 1
            players.Add(entityPlayer);
            entityPlayer.UpPositionChunk();
            // Добавляем игрок в конкретный чанк
            GetPlayerInstance(entityPlayer.PositionChunk, true).AddPlayer(entityPlayer, true);

            //entityPlayer.FlagSpawn = true;
            World.SpawnEntityInWorld(entityPlayer);
            //entityPlayer.FlagSpawn = false;

            // отладка 5 кур
            for (int i = 0; i < 5; i++)
            {
                lastPlayerId++;
                EntityChicken entityChicken = new EntityChicken(World);
                entityChicken.SetEntityId(lastPlayerId);
                entityChicken.SetPosition(entityPlayer.Position + new vec3(3, World.Rand.Next(0, 10), 0));
                World.SpawnEntityInWorld(entityChicken);
            }
        }

        private void SpawnPositionTest(EntityPlayerServer entityPlayer)
        {
            Random random = new Random();
            entityPlayer.SetPosLook(new vec3(random.Next(-16, 16) + 0040, 30, random.Next(-16, 16)), -0.9f, -.8f);
        }

        /// <summary>
        /// Удалить всех игроков при остановки сервера
        /// </summary>
        public void PlayersRemoveStopingServer()
        {
            for (int i = 0; i < players.Count; i++)
            {
                playerRemoveList.Add(players[i].Id);
            }
            // Надо отработать Update
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        public void PlayerRemove(Socket socket)
        {
            EntityPlayerServer entityPlayer = GetPlayerSocket(socket);
            if (entityPlayer != null)
            {
                playerRemoveList.Add(entityPlayer.Id);
            }
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        private void PlayerRemove(ushort id)
        {
            EntityPlayerServer entityPlayer = GetPlayerId(id);

            if (entityPlayer != null)
            {
                World.ServerMain.Log.Log("server.player.entry.repeat {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                World.Tracker.RemovePlayerFromTrackers(entityPlayer);
                RemoveMountedMovingPlayer(entityPlayer);
                World.RemoveEntity(entityPlayer);
                players.Remove(entityPlayer);
                //ResponsePacketAll(new PacketSF1Disconnect(entityPlayer.Id), entityPlayer.Id);
            }
        }

        /// <summary>
        /// По сокету найти игрока
        /// </summary>
        public EntityPlayerServer GetPlayerSocket(Socket socket)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].SocketClient == socket) return players[i];
            }
            return null;
        }

        /// <summary>
        /// По id найти игрока
        /// </summary>
        public EntityPlayerServer GetPlayerId(ushort id)
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (players[i].Id == id) return players[i];
            }
            return null;
        }

        /// <summary>
        /// Получить основного игрока который создал сервер
        /// </summary>
        public EntityPlayerServer GetEntityPlayerMain() => GetPlayerSocket(null);

        /// <summary>
        /// Количество игроков
        /// </summary>
        public int PlayerCount => players.Count;
        /// <summary>
        /// ПустойЮ нет игроков
        /// </summary>
        public bool IsEmpty() => players.Count == 0;

        /// <summary>
        /// Находится ли игрок в этом чанке
        /// </summary>
        /// <param name="entityPlayer">игрок</param>
        /// <param name="pos">позиция чанка</param>
        public bool IsPlayerWatchingChunk(EntityPlayerServer entityPlayer, vec2i pos)
        {
            PlayerInstance chunkPlayer = GetPlayerInstance(pos, false);
                return chunkPlayer != null && chunkPlayer.Contains(entityPlayer) 
                && entityPlayer.LoadedChunks.Contains(chunkPlayer.CurrentChunk);
        }

        #endregion

        /// <summary>
        /// Получить объект связи чанков с игроками если его нет, создать объект при isNew=true
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isNew">если null создать</param>
        private PlayerInstance GetPlayerInstance(vec2i pos, bool isNew)
        {
            PlayerInstance playerInstance = playerInstances.ContainsKey(pos) ?
                playerInstance = playerInstances[pos] as PlayerInstance : null;

            if (isNew && playerInstance == null)
            {
                playerInstance = new PlayerInstance(this, pos);
                
                playerInstances.Add(pos, playerInstance);
                playerInstanceList.Add(playerInstance);
            }

            return playerInstance;
        }

        /// <summary>
        /// Проверка запуска игрока
        /// </summary>
        public void LoginStart(EntityPlayerServer entityPlayer)
        {
            if (CheckPlayerAdd(entityPlayer))
            {
                // Проверка игрока с повторным именем прошла
                if (entityPlayer.SocketClient == null)
                {
                    // основной игрок, запуск сервера!!!
                    PlayerAdd(entityPlayer);
                    // После того как загрузиться запуститься метод LoginStart() для запуска пакета PacketS12Success
                    World.ServerMain.StartServer();
                    
                } else
                {
                    // Для тактового запуска игрока
                    playerStartList.Add(entityPlayer);
                }

            } else
            {
                World.ServerMain.Log.Log("server.player.entry.duplicate {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                // Игрок с таким именем в игре!
                entityPlayer.SendPacket(new PacketSF0Connection("world.player.duplicate"));
            }
        }

        /// <summary>
        /// Запуск игрока из игрового такта, это сетевые игроки
        /// </summary>
        private void LoginStartTick(EntityPlayerServer entityPlayer)
        {
            if (entityPlayer.SocketClient != null)
            {
                // Сетевой игрок подключён, сразу отправляем пакет PacketJoinGame
                PlayerAdd(entityPlayer);
                ResponsePacketJoinGame(entityPlayer);
            }
        }

        /// <summary>
        /// Запуск основного игрока который создал сервер
        /// </summary>
        public void LoginStart()
        {
            EntityPlayerServer player = GetEntityPlayerMain();
            if (player != null)
            {
                ResponsePacketJoinGame(player);
            }
        }

        /// <summary>
        /// Начало игры, прошли проверку на игрока, теперь в игре
        /// </summary>
        private void ResponsePacketJoinGame(EntityPlayerServer player)
        {
            player.SendPacket(new PacketS02JoinGame(player.Id, player.UUID));
            player.SendPacket(new PacketS08PlayerPosLook(player.Position, player.RotationYawHead, player.RotationPitch));
            player.SendPacket(new PacketS03TimeUpdate(World.ServerMain.TickCounter));
        }
        
        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        public void ClientSetting(Socket socket, PacketC15ClientSetting packet)
        {
            EntityPlayerServer player = GetPlayerSocket(socket);
            if (player != null)
            {
                // Смена обзора
                player.SetOverviewChunk(packet.GetOverviewChunk());
            }
        }

        /// <summary>
        /// Пакет статуса клиента
        /// </summary>
        public void ClientStatus(Socket socket, PacketC16ClientStatus.EnumState state)
        {
            EntityPlayerServer player = GetPlayerSocket(socket);
            if (player != null)
            {
                if (state == PacketC16ClientStatus.EnumState.Respawn)
                {
                    // Респавн игрока
                    SpawnPositionTest(player);
                    player.Respawn();
                    player.SendPacket(new PacketS07Respawn());
                    player.SendPacket(new PacketS08PlayerPosLook(player.Position, player.RotationYawHead, player.RotationPitch));

                    World.SpawnEntityInWorld(player);
                }
            }
        }

        #region Фрагменты чанков вокруг игрока

        /// <summary>
        /// Удалить фрагменты чанков вокруг игрока
        /// </summary>
        private void RemoveMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            int radius = entityPlayer.OverviewChunk + 1;
            int chx = entityPlayer.ChunkPosManaged.x;
            int chz = entityPlayer.ChunkPosManaged.y;
            for (int x = chx - radius; x <= chx + radius; x++)
            {
                for (int z = chz - radius; z <= chz + radius; z++)
                {
                    PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x, z), false);
                    if (playerInstance != null)
                    {
                        playerInstance.RemovePlayer(entityPlayer);
                    }
                }
            }
            entityPlayer.LoadingChunks.Clear();
        }
        /// <summary>
        /// Определите, перекрываются ли два прямоугольника с центрами в заданных точках 
        /// для заданного радиуса.
        /// </summary>
        private bool Overlaps(int x1, int z1, int x2, int z2, int radius)
        {
            int xb = x1 - x2;
            int zb = z1 - z2;
            return xb >= -radius && xb <= radius ? zb >= -radius && zb <= radius : false;
        }

        /// <summary>
        /// обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
        /// </summary>
        public void UpdateMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            bool isFilter = false;

            // Проверяем изменение обзора чанка
            if (!entityPlayer.SameOverviewChunkPrev())
            {
                int radius = entityPlayer.OverviewChunk + 1;
                int radiusPrev = entityPlayer.OverviewChunkPrev;
                int radiusMax = Mth.Max(radius, radiusPrev + 1);
                int chx = entityPlayer.ChunkPosManaged.x;
                int chz = entityPlayer.ChunkPosManaged.y;
                    
                for (int x = chx - radiusMax; x <= chx + radiusMax; x++)
                {
                    for (int z = chz - radiusMax; z <= chz + radiusMax; z++)
                    {
                        if (entityPlayer.OverviewChunk > entityPlayer.OverviewChunkPrev)
                        {
                            if (x < chx - radiusPrev || x > chx + radiusPrev || z < chz - radiusPrev || z > chz + radiusPrev)
                            {
                                // Увеличиваем обзор
                                GetPlayerInstance(new vec2i(x, z), true).AddPlayer(entityPlayer, false);
                            }
                        }
                        else
                        {
                            if (x < chx - radius || x > chx + radius || z < chz - radius || z > chz + radius)
                            {
                                // Уменьшить обзор
                                PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x, z), false);
                                if (playerInstance != null)
                                {
                                    playerInstance.RemovePlayer(entityPlayer);
                                }
                            }
                        }
                    }
                }

                entityPlayer.UpOverviewChunkPrev();
                isFilter = true;
            }

            // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
            vec2i chunkCoor = entityPlayer.GetChunkPos();
            int bias = 2;
            if (Mth.Abs(chunkCoor.x - entityPlayer.ChunkPosManaged.x) >= bias || Mth.Abs(chunkCoor.y - entityPlayer.ChunkPosManaged.y) >= bias)
            {
                // Смещение чанка
                int radius = entityPlayer.OverviewChunk + 1;
                int chx = chunkCoor.x;
                int chz = chunkCoor.y;
                int chmx = entityPlayer.ChunkPosManaged.x;
                int chmz = entityPlayer.ChunkPosManaged.y;
                int dx = chx - chmx;
                int dz = chz - chmz;

                for (int x = chx - radius; x <= chx + radius; x++)
                {
                    for (int z = chz - radius; z <= chz + radius; z++)
                    {
                        if (!Overlaps(x, z, chmx, chmz, radius))
                        {
                            GetPlayerInstance(new vec2i(x, z), true).AddPlayer(entityPlayer, false);
                        }

                        if (!Overlaps(x - dx, z - dz, chx, chz, radius))
                        {
                            PlayerInstance playerInstance = GetPlayerInstance(new vec2i(x - dx, z - dz), false);
                            if (playerInstance != null)
                            {
                                playerInstance.RemovePlayer(entityPlayer);
                            }
                        }
                    }
                }
                
                entityPlayer.SetChunkPosManaged(new vec2i(chmx + dx, chmz + dz));
                isFilter = true;
            }

            if (isFilter)
            {
                FilterChunkLoadQueue(entityPlayer);
            }
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков и сортирует их с центра в даль для игрока entityPlayer
        /// </summary>
        private void FilterChunkLoadQueue(EntityPlayerServer entityPlayer)
        {
            vec2i currentChunk = GetPlayerInstance(entityPlayer.GetChunkPos(), true).CurrentChunk;
            Hashtable map = entityPlayer.LoadedChunks.CloneMap();
            entityPlayer.LoadedChunks.Clear();
            entityPlayer.LoadingChunks.Clear();

            if (entityPlayer.DistSqrt != null)
            {
                vec2i chunkPosManaged = entityPlayer.ChunkPosManaged;
                for (int d = 0; d < entityPlayer.DistSqrt.Length; d++)
                {
                    vec2i pos = entityPlayer.DistSqrt[d] + chunkPosManaged;
                    World.ChunkPrServ.DroppedChunks.Remove(pos);
                    if (map.ContainsKey(pos))
                    {
                        entityPlayer.LoadedChunks.Add(pos);
                        entityPlayer.LoadingChunks.Add(pos);
                    }
                }
            }

            // Методика как в майне, но не кругом, а квадратом
            //Hashtable map = entityPlayer.LoadedChunks.CloneMap();
            //int i = 0;

            //int radius = entityPlayer.OverviewChunk + 1;
            //int chx = entityPlayer.ChunkPos.x;
            //int chz = entityPlayer.ChunkPos.y;
            //int x = 0;
            //int z = 0;
            //vec2i pos = entityPlayer.ChunkPos;
            //entityPlayer.LoadedChunks.Clear();

            //if (map.ContainsKey(pos)) entityPlayer.LoadedChunks.Add(pos);

            //int k = 0;
            //int r;
            //for (r = 1; r <= radius * 2; r++)
            //{
            //    for (int i2 = 0; i2 < 2; i2++)
            //    {
            //        int[] vecxz = xzDirectionsConst[i++ % 4];

            //        for (int i3 = 0; i3 < r; i3++)
            //        {
            //            x += vecxz[0];
            //            z += vecxz[1];
            //            pos = new vec2i(chx + x, chz + z);
            //            k++;
            //            if (map.ContainsKey(pos)) entityPlayer.LoadedChunks.Add(pos);
            //        }
            //    }
            //}

            //i %= 4;

            //for (r = 0; r < radius * 2; ++r)
            //{
            //    x += xzDirectionsConst[i][0];
            //    z += xzDirectionsConst[i][1];
            //    pos = new vec2i(chx + x, chz + z);

            //    if (map.ContainsKey(pos)) entityPlayer.LoadedChunks.Add(pos);
            //}
        }

        #endregion


        public void Update()
        {
            // Удаляем игроков
            while (playerRemoveList.Count > 0)
            {
                ushort id = playerRemoveList[0];
                playerRemoveList.RemoveAt(0);
                PlayerRemove(id);

                for (int i = 0; i < playerStartList.Count; i++)
                {
                    if (playerStartList[i].Id == id)
                    {
                        playerRemoveList.RemoveAt(i);
                        break;
                    }
                }
            }
            // Добавляем игроков
            while (playerStartList.Count > 0)
            {
                EntityPlayerServer entityPlayer = playerStartList[0];
                playerStartList.RemoveAt(0);
                LoginStartTick(entityPlayer);
            }
        }
        /// <summary>
        /// Обновляет все экземпляры игроков, которые необходимо обновить 
        /// </summary>
        public void UpdatePlayerInstances()
        {
            profiler.StartSection("LoadingChunks");
            try
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayerServer player = players[i];
                    // Количество чанков для загрузки или генерации за такт
                    int load = 5;
                    // Всего проверки чанков за такт
                    int all = 100;
                    while (player.LoadingChunks.Count > 0 && load > 0 && all > 0)
                    {
                        vec2i posCh = player.LoadingChunks.FirstRemove();
                        if (!World.ChunkPrServ.IsChunk(posCh))
                        {
                            World.ChunkPrServ.LoadChunk(posCh);
                            load--;
                        }
                        all--;
                    }
                }
            }
            catch (Exception ex)
            {
                World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.LoadingChunks");
                Logger.Crach(ex);
                throw;
            }
            profiler.EndSection();

            
            //Hashtable playersClone = players.Clone() as Hashtable;
            //foreach (EntityPlayerServer player in playersClone.Values)
            //{
            //    try
            //    {
            //        profiler.StartSection("UpdatePlayer");
            //        player.UpdatePlayer();
            //        profiler.EndSection();
            //    }
            //    catch
            //    {
            //        World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.UpdatePlayer");
            //        throw;
            //    }
            //}

            // Чистка чанков по времени
            uint time = World.ServerMain.TickCounter;
            //if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
            //{
            //    previousTotalWorldTime = time;
            //    try
            //    {
            //        profiler.StartSection("ChunkCoordPlayers");
            //        // Занести 
            //        Hashtable htCh = playerInstances.Clone() as Hashtable;
            //        foreach (PlayerInstance ccp in htCh.Values)
            //        {
            //            ccp.FixOverviewChunk();
            //            if (ccp.CountPlayers() == 0)
            //            {
            //                playerInstances.Remove(ccp.Position);
            //                world.ChunkPrServ.DroppedChunks.Add(ccp.Position);
            //            }
            //        }
            //        // Добавить в список удаляющих чанков которые не полного статуса
            //        world.ChunkPrServ.DroopedChunkStatusMin(players);
            //        profiler.EndSection();
            //    }
            //    catch
            //    {
            //        world.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.ChunkCoordPlayers");
            //        throw;
            //    }
            //}

            //int j = 0;
            //while(playerInstanceListLoad.Count > 0 && j < 20)
            //{
            //    World.ChunkPrServ.LoadChunk(playerInstanceListLoad[0]);
            //    playerInstanceListLoad.RemoveAt(0);
            //    j++;
            //}



            if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME) // 8000
            {
                previousTotalWorldTime = time;
                for (int i = 0; i < playerInstanceList.Count; i++)
                {
                    playerInstanceList[i].Update();
                    playerInstanceList[i].ProcessChunk();
                }
                playerInstancesToUpdate.Clear();
            }
            else
            {
                while(playerInstancesToUpdate.Count > 0)
                {
                    playerInstancesToUpdate[0].Update();
                    playerInstancesToUpdate.RemoveAt(0);
                }
            }
        }

        /// <summary>
        /// Удалить игрока с конкретного чанка 
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        /// <param name="player">игрок</param>
        public void PlayerInstancesRemove(vec2i pos, PlayerInstance player)
        {
            playerInstances.Remove(pos);
            playerInstanceList.Remove(player);
        }
        public void PlayerInstancesToUpdateRemove(PlayerInstance player) => playerInstancesToUpdate.Remove(player);
        public void PlayerInstancesToUpdateAdd(PlayerInstance player) => playerInstancesToUpdate.Add(player);

        /// <summary>
        /// Количество чанков у всех игроков
        /// </summary>
        public int CountPlayerInstances() => playerInstances.Count;

        /// <summary>
        /// Флаг блока который был изменён
        /// </summary>
        public void FlagChunkForUpdate(BlockPos blockPos)
        {
            PlayerInstance playerInstance = GetPlayerInstance(blockPos.GetPositionChunk(), false);
            if (playerInstance != null)
            {
                playerInstance.FlagChunkForUpdate(blockPos.GetPosition0());
            }
        }

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress)
        {
            for (int i = 0; i < players.Count; i++)
            {
                EntityPlayerServer entityPlayer = players[i];
                if (entityPlayer != null && entityPlayer.Id != breakerId)
                {
                    if (pos.DistanceNotSqrt(entityPlayer.Position) < 1024f)
                    {
                        // растояние меньше 32 блоков
                        entityPlayer.SendPacket(new PacketS25BlockBreakAnim(breakerId, pos, progress));
                    }
                }
            }
        }


        /// <summary>
        /// Список чанков для отладки
        /// </summary>
        [Obsolete("Список чанков только для отладки")]
        public List<vec2i> GetListDebug()
        {
            List<vec2i> list = new List<vec2i>();
            Hashtable ht = playerInstances.Clone() as Hashtable;
            foreach (PlayerInstance playerInstance in ht.Values)
            {
                if (playerInstance.CountPlayers() > 0) list.Add(playerInstance.CurrentChunk);
            }
            return list;
        }

        public string ToStringDebug()
        {
            string strPlayers = "";
            if (PlayerCount > 0)
            {
                for (int i = 0; i < players.Count; i++)
                {
                    EntityPlayerServer entity = players[i];
                    strPlayers += entity.Name + " [p" + entity.Ping + "|" + (entity.IsDead ? "Dead" : ("h" + entity.Health)) + "]";
                }
            }
            return strPlayers;
        }
    }
}
