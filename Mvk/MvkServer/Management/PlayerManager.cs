using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// </summary>
    public class PlayerManager
    {
        /// <summary>
        /// Для фильтрации чанков игрока
        /// </summary>
        //private readonly int[][] xzDirectionsConst = { new int[] { 1, 0 }, new int[] { 0, 1 }, new int[] { -1, 0 }, new int[] { 0, -1 } };
        /// <summary>
        /// Массив игроков
        /// </summary>
        protected Hashtable players = new Hashtable();
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; protected set; }
        /// <summary>
        /// Чанки игроков
        /// </summary>
        public Hashtable chunkCoordPlayers = new Hashtable();

        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;

        public PlayerManager(WorldServer worldServer) => World = worldServer;

        #region Player

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket(EntityPlayerServer entityPlayer, IPacket packet) 
            => World.ServerMain.ResponsePacket(entityPlayer.SocketClient, packet);

        /// <summary>
        /// Отправить пакеты всех игрокам
        /// </summary>
        public void ResponsePacketAll(IPacket packet)
        {
            Hashtable ht = players.Clone() as Hashtable;
            foreach (EntityPlayerServer player in ht.Values)
            {
                ResponsePacket(player, packet);
            }
        }

        /// <summary>
        /// Добавить игрока
        /// </summary>
        protected bool PlayerAdd(EntityPlayerServer entityPlayer)
        {
            if (!players.ContainsKey(entityPlayer.UUID))
            {
                // TODO::Тут проверяем место положение персонажа, и заносим при запуске
                Random random = new Random();
                entityPlayer.SetRotation(-0.9f, -.8f);
                entityPlayer.SetPosition(new vec3(random.Next(-16, 16) + 80, 30, random.Next(-16, 16)));
                entityPlayer.SetChunkPosManaged(entityPlayer.GetChunkPos());
                AddMountedMovingPlayer(entityPlayer);
                players.Add(entityPlayer.UUID, entityPlayer);
                FilterChunkLoadQueue(entityPlayer);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Удалить игрока
        /// </summary>
        public void PlayerRemove(Socket socket) => PlayerRemove(GetPlayer(socket));

        /// <summary>
        /// Очистить всех игроков
        /// </summary>
        public void PlayerClear()
        {
            Hashtable ht = players.Clone() as Hashtable;
            foreach (EntityPlayerServer player in ht.Values)
            {
                PlayerRemove(player);
            }
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        public void PlayerRemove(EntityPlayerServer entityPlayer)
        {
            if (entityPlayer != null && players.ContainsKey(entityPlayer.UUID))
            {
                World.ServerMain.Log.Log("server.player.entry.repeat {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                RemoveMountedMovingPlayer(entityPlayer);
                players.Remove(entityPlayer.UUID);
            }
        }

        /// <summary>
        /// По сокету найти игрока
        /// </summary>
        public EntityPlayerServer GetPlayer(Socket socket)
        {
            foreach(EntityPlayerServer player in players.Values)
            {
                if (player.SocketClient == socket)
                {
                    return player;
                }
            }
            return null;
        }

        /// <summary>
        /// Количество игроков
        /// </summary>
        public int PlayerCount => players.Count;

        /// <summary>
        /// Получить основного игрока который создал сервер
        /// </summary>
        public EntityPlayerServer GetEntityPlayerMain()
        {
            foreach (EntityPlayerServer player in players.Values)
            {
                if (player.SocketClient == null) return player;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Получить объект связи чанков с игроками
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isNew">если null создать</param>
        protected ChunkCoordPlayers GetChunkCoordPlayers(vec2i pos, bool isNew)
        {
            ChunkCoordPlayers ccp = chunkCoordPlayers.ContainsKey(pos) ?
                ccp = chunkCoordPlayers[pos] as ChunkCoordPlayers : null;

            if (isNew && ccp == null)
            {
                ccp = new ChunkCoordPlayers(pos);
                chunkCoordPlayers.Add(pos, ccp);
            }

            return ccp;
        }

        /// <summary>
        /// Проверка запуска игрока
        /// </summary>
        public void LoginStart(EntityPlayerServer entityPlayer)
        {
            if (PlayerAdd(entityPlayer))
            {
                // норм!
                World.ServerMain.Log.Log("server.player.entry {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                if (entityPlayer.SocketClient == null)
                {
                    // основной игрок, запуск сервера!!!
                    // После того как загрузиться запуститься метод LoginStart() для запуска пакета PacketS12Success
                    World.ServerMain.StartServer();
                }
                else
                {
                    // Сетевой игрок подключён, сразу отправляем пакет PacketS12Success
                    GameBegin(entityPlayer);
                }
            } else
            {
                World.ServerMain.Log.Log("server.player.entry.duplicate {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                // Игрок с таким именем в игре!
                ResponsePacket(entityPlayer, new PacketS10Connection("world.player.duplicate"));
            }
            return;
        }

        /// <summary>
        /// Запуск основного игрока который создал сервер
        /// </summary>
        public void LoginStart()
        {
            EntityPlayerServer player = GetEntityPlayerMain();
            if (player != null) GameBegin(player);
        }

        protected void ResponsePacketS12Success(EntityPlayerServer player)
        {
            PacketS12Success packet = new PacketS12Success(player.UUID)
            {
                Pos = player.Position,
                Yaw = player.RotationYaw,
                Pitch = player.RotationPitch
            };
            ResponsePacket(player, packet);
            ResponsePacket(player, new PacketS14TimeUpdate(World.ServerMain.TickCounter));
        }

        /// <summary>
        /// Начало игры, прошли проверку на игрока, теперь в игре
        /// </summary>
        protected void GameBegin(EntityPlayerServer player)
        {
            // отправить игроку его местоположение и время сервера
            ResponsePacketS12Success(player);
            
        }

        /// <summary>
        /// Пакет настроек клиента
        /// </summary>
        public void ClientSetting(Socket socket, PacketC13ClientSetting packet)
        {
            EntityPlayerServer player = GetPlayer(socket);
            if (player != null)
            {
                RemoveMountedMovingPlayer(player);
                player.SetOverviewChunk(packet.GetOverviewChunk(), 1);
                AddMountedMovingPlayer(player);
                FilterChunkLoadQueue(player);
            }
        }

        #region Фрагменты чанков вокруг игрока

        /// <summary>
        /// Добавить фрагменты чанков вокруг игрока
        /// </summary>
        protected void AddMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            int radius = entityPlayer.OverviewChunk + 1;
            int chx = entityPlayer.ChunkPosManaged.x;
            int chz = entityPlayer.ChunkPosManaged.y;
            for (int x = chx - radius; x <= chx + radius; x++)
            {
                for (int z = chz - radius; z <= chz + radius; z++)
                {
                    GetChunkCoordPlayers(new vec2i(x, z), true).AddPlayer(entityPlayer);
                }
            }
        }
        /// <summary>
        /// Удалить фрагменты чанков вокруг игрока
        /// </summary>
        protected void RemoveMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            int radius = entityPlayer.OverviewChunk + 1;
            int chx = entityPlayer.ChunkPosManaged.x;
            int chz = entityPlayer.ChunkPosManaged.y;
            for (int x = chx - radius; x <= chx + radius; x++)
            {
                for (int z = chz - radius; z <= chz + radius; z++)
                {
                    ChunkCoordPlayers ccp = GetChunkCoordPlayers(new vec2i(x, z), false);
                    if (ccp != null)
                    {
                        ccp.RemovePlayer(entityPlayer);
                    }
                }
            }
        }
        /// <summary>
        /// Определите, перекрываются ли два прямоугольника с центрами в заданных точках 
        /// для заданного радиуса.
        /// </summary>
        protected bool Overlaps(int x1, int z1, int x2, int z2, int radius)
        {
            int xb = x1 - x2;
            int zb = z1 - z2;
            return xb >= -radius && xb <= radius ? zb >= -radius && zb <= radius : false;
        }

        /// <summary>
        /// обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
        /// </summary>
        protected void UpdateMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
            if (entityPlayer.CheckPosManaged(2))
            {
                int radius = entityPlayer.OverviewChunk + 1;
                vec2i chunkCoor = entityPlayer.GetChunkPos(); 
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
                            GetChunkCoordPlayers(new vec2i(x, z), true).AddPlayer(entityPlayer);
                        }

                        if (!Overlaps(x - dx, z - dz, chx, chz, radius))
                        {
                            ChunkCoordPlayers ccp = GetChunkCoordPlayers(new vec2i(x - dx, z - dz), false);
                            if (ccp != null)
                            {
                                if (ccp.RemovePlayer(entityPlayer))
                                {
                                    // отправить игроку что чанк удалить
                                    vec2i posch = new vec2i(ccp.Position);
                                    //System.Threading.Tasks.Task.Factory.StartNew(() =>
                                    {
                                        World.ServerMain.ResponsePacket(entityPlayer.SocketClient, new PacketS21ChunckData(posch));
                                    }//);
                                }
                            }
                        }
                    }
                }

                entityPlayer.SetChunkPosManaged(new vec2i(chmx + dx, chmz + dz));
                FilterChunkLoadQueue(entityPlayer);
            }
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков и сортирует их с центра в даль для игрока LoadedChunks
        /// </summary>
        protected void FilterChunkLoadQueue(EntityPlayerServer entityPlayer)
        {
            Hashtable map = entityPlayer.LoadedChunks.CloneMap();
            entityPlayer.LoadedChunks.Clear();

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

        /// <summary>
        /// Обновляет все экземпляры игроков, которые необходимо обновить 
        /// </summary>
        public void UpdatePlayerInstances()
        {

            Hashtable playersClone = players.Clone() as Hashtable;
            foreach (EntityPlayerServer player in playersClone.Values)
            {
                try
                {
                    // Проверяем смещение чанка на выбранный параметр, если есть начинаем обработку
                    World.Players.UpdateMountedMovingPlayer(player);
                    player.Update();
                }
                catch
                {
                    World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.EntityPlayerServer");
                    throw;
                }
            }
            
            uint time = World.ServerMain.TickCounter;

            if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
            {
                previousTotalWorldTime = time;
                try
                {
                    // Занести 
                    Hashtable htCh = chunkCoordPlayers.Clone() as Hashtable;
                    foreach (ChunkCoordPlayers ccp in htCh.Values)
                    {
                        ccp.FixOverviewChunk();
                        if (ccp.Count == 0)
                        {
                            chunkCoordPlayers.Remove(ccp.Position);
                            World.ChunkPrServ.DroppedChunks.Add(ccp.Position);
                        }
                    }
                    // Добавить в список удаляющих чанков которые не полного статуса
                    World.ChunkPrServ.DroopedChunkStatusMin(players.Clone() as Hashtable);
                }
                catch
                {
                    World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.ChunkCoordPlayers");
                    throw;
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
            Hashtable ht = chunkCoordPlayers.Clone() as Hashtable;
            foreach (ChunkCoordPlayers ccp in ht.Values)
            {
                if (ccp.Count > 0) list.Add(ccp.Position);
            }
            return list;
        }
    }
}
