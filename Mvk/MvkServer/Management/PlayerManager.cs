using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Util;
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
        private readonly int[][] xzDirectionsConst = {
                new int[] { 1, 0 }, new int[] { 0, 1 },
                new int[] { -1, 0 }, new int[] { 0, -1 }
        };
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
        /// Добавить игрока
        /// </summary>
        protected bool PlayerAdd(EntityPlayerServer entityPlayer)
        {
            if (!players.ContainsKey(entityPlayer.UUID))
            {
                // TODO::Тут проверяем место положение персонажа, и заносим при запуске
                Random random = new Random();
                entityPlayer.HitBox.SetPos(new vec3(random.Next(-16, 16), 82, random.Next(-16, 16)));
                entityPlayer.HitBox.UpChunkPosManaged();
                entityPlayer.SetRotation(0.1f, 0f);

                int radius = entityPlayer.OverviewChunk;
                int chx = entityPlayer.HitBox.ChunkPos.x;
                int chz = entityPlayer.HitBox.ChunkPos.y;
                for (int x = chx - radius; x <= chx + radius; x++)
                {
                    for (int z = chz - radius; z <= chz + radius; z++)
                    {
                        GetChunkCoordPlayers(new vec2i(x, z), true).AddPlayer(entityPlayer);
                    }
                }

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

                int radius = entityPlayer.OverviewChunk;
                int chx = entityPlayer.HitBox.ChunkPos.x;
                int chz = entityPlayer.HitBox.ChunkPos.y;
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

        //public void ChunkCoordPlayersNew(vec2i pos)
        //{
        //    chunkCoordPlayers.Add(pos, new ChunkCoordPlayers(pos));
        //}

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
                    ResponsePacketS12Success(entityPlayer);
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

        /// <summary>
        /// Запуск основного игрока который создал сервер
        /// </summary>
        public void LoginStart()
        {
            EntityPlayerServer player = GetEntityPlayerMain();
            if (player != null)
            {
                ResponsePacketS12Success(player);
                GameBegin(player);
            }
        }

        protected void ResponsePacketS12Success(EntityPlayerServer player)
        {
            PacketS12Success packet = new PacketS12Success(player.UUID)
            {
                Pos = player.HitBox.Position,
                Yaw = player.RotationYaw,
                Pitch = player.RotationPitch,
                Timer = World.ServerMain.TickCounter
            };
            ResponsePacket(player, packet);
        }

        /// <summary>
        /// Начало игры
        /// </summary>
        protected void GameBegin(EntityPlayerServer player)
        {
            // Прошли проверку на игрока, теперь в игре
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
        /// обновлять фрагменты вокруг игрока, перемещаемого логикой сервера (например, тележка, лодка) 
        /// Проверяет смещение игрока по чанку, и если есть вызывает FilterChunkLoadQueue
        /// </summary>
        public void UpdateMountedMovingPlayer(EntityPlayerServer entityPlayer)
        {
            int chx = entityPlayer.HitBox.ChunkPos.x;
            int chz = entityPlayer.HitBox.ChunkPos.y;

            if (!entityPlayer.HitBox.ChunkPosManaged.Equals(entityPlayer.HitBox.ChunkPos))
            {
                int radius = entityPlayer.OverviewChunk;
                int chmx = entityPlayer.HitBox.ChunkPosManaged.x;
                int chmz = entityPlayer.HitBox.ChunkPosManaged.y;
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
                                    World.ServerMain.ResponsePacket(entityPlayer.SocketClient, new PacketS21ChunckData(ccp.Position));
                                }
                            }
                        }
                    }
                }

                FilterChunkLoadQueue(entityPlayer);
                entityPlayer.HitBox.UpChunkPosManaged();
            }
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков для игрока LoadedChunks
        /// </summary>
        protected void FilterChunkLoadQueue(EntityPlayerServer entityPlayer)
        {
            List<vec2i> list = entityPlayer.LoadedChunks.GetRange(0, entityPlayer.LoadedChunks.Count);
            int i = 0;

            int radius = entityPlayer.OverviewChunk;
            int chx = entityPlayer.HitBox.ChunkPos.x;
            int chz = entityPlayer.HitBox.ChunkPos.y;
            int x = 0;
            int z = 0;
            vec2i pos = entityPlayer.HitBox.ChunkPos;
            entityPlayer.LoadedChunks.Clear();

            if (list.Contains(pos)) entityPlayer.LoadedChunks.Add(pos);

            int r;
            for (r = 1; r <= radius * 2; ++r)
            {
                for (int var11 = 0; var11 < 2; ++var11)
                {
                    int[] var12 = xzDirectionsConst[i++ % 4];

                    for (int var13 = 0; var13 < r; ++var13)
                    {
                        x += var12[0];
                        z += var12[1];
                        pos = new vec2i(chx + x, chz + z);

                        if (list.Contains(pos)) entityPlayer.LoadedChunks.Add(pos);
                    }
                }
            }

            i %= 4;

            for (r = 0; r < radius * 2; ++r)
            {
                x += xzDirectionsConst[i][0];
                z += xzDirectionsConst[i][1];
                pos = new vec2i(chx + x, chz + z);

                if (list.Contains(pos)) entityPlayer.LoadedChunks.Add(pos);
            }
            return;
        }

        /// <summary>
        /// Обновляет все экземпляры игроков, которые необходимо обновить 
        /// </summary>
        public void UpdatePlayerInstances()
        {

            Hashtable ht = players.Clone() as Hashtable;
            foreach (EntityPlayerServer player in ht.Values)
            {
                try
                {
                    player.Update();
                }
                catch
                {
                    World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.EntityPlayerServer");
                    throw;
                }
            }
            
            uint time = World.ServerMain.TickCounter;
            //long var1 = this.theWorldServer.getTotalWorldTime();
            //int var3;
            //ChunkCoordPlayers ccp
            //ChunkCoordPlayers ccp;

            if (time - previousTotalWorldTime > 100) // 8000
            {
                previousTotalWorldTime = time;
                try
                {
                    ht = chunkCoordPlayers.Clone() as Hashtable;
                    foreach (ChunkCoordPlayers ccp in ht.Values)
                    {
                        if (ccp.Count == 0)
                        {
                            chunkCoordPlayers.Remove(ccp.Position);
                            World.ChunkPr.UnloadChunk(ccp.Position);
                        }
                    }
                }
                catch
                {
                    World.ServerMain.Log.Error("PlayerManager.UpdatePlayerInstances.ChunkCoordPlayers");
                    throw;
                }

                //chunkCoordPlayers

                //for (int i = 0; i < this.playerInstanceList.size(); i++)
                //{
                //    ccp = (PlayerManager.PlayerInstance)this.playerInstanceList.get(i);
                //    ccp.onUpdate();
                //    ccp.processChunk();
                //}
            }
            //else
            //{
            //    for (int i = 0; i < this.playerInstancesToUpdate.size(); i++)
            //    {
            //        ccp = (PlayerManager.PlayerInstance)this.playerInstancesToUpdate.get(i);
            //        ccp.onUpdate();
            //    }
            //}

            //this.playerInstancesToUpdate.clear();

            //if (this.players.isEmpty())
            //{
            //    WorldProvider var5 = this.theWorldServer.provider;

            //    if (!var5.canRespawnHere())
            //    {
            //        this.theWorldServer.theChunkProviderServer.unloadAllChunks();
            //    }
            //}
        }

        /// <summary>
        /// Для дебага
        /// </summary>
        public List<vec2i> GetList()
        {
            // TODO::отладка чанков
            List<vec2i> list = new List<vec2i>();
            Hashtable ht = chunkCoordPlayers.Clone() as Hashtable;
            foreach (ChunkCoordPlayers ccp in ht.Values)
            {
                if (ccp.Count > 0)
                {
                    list.Add(ccp.Position);
                }
            }
            return list;
        }
    }
}
