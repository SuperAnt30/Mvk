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
        /// <summary>
        /// Массив игроков
        /// </summary>
        protected static Hashtable players = new Hashtable();
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; protected set; }

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
                entityPlayer.SetRotation(0.1f, 0f);

                players.Add(entityPlayer.UUID, entityPlayer);
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
        public void PlayerRemove(EntityPlayerServer player)
        {
            if (player != null && players.ContainsKey(player.UUID))
            {
                World.ServerMain.Log.Log("server.player.entry.repeat {0} [{1}]", player.Name, player.UUID);
                players.Remove(player.UUID);
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
                    World.ServerMain.StartServerThread();
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
                // TODO::local error Игрок с таким именем в игре!
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
        /// обновлять фрагменты вокруг игрока, перемещаемого логикой сервера (например, тележка, лодка) 
        /// Проверяет смещение игрока по чанку, и если есть вызывает FilterChunkLoadQueue
        /// </summary>
        public void UpdateMountedMovingPlayer(EntityPlayerServer player)
        {
            FilterChunkLoadQueue(player);
        }

        /// <summary>
        /// Удаляет все фрагменты из очереди загрузки фрагментов данного проигрывателя, которые не находятся в зоне видимости проигрывателя. 
        /// Обновляет список координат чанков для игрока LoadedChunks
        /// </summary>
        protected void FilterChunkLoadQueue(EntityPlayerServer player)
        {
            int[][] xzDirectionsConst = {
                 new int[] { 1, 0 }, new int[] { 0, 1 },
                 new int[] { -1, 0 }, new int[] { 0, -1 }
            };

            List<vec2i> list = new List<vec2i>(player.LoadedChunks);
            int i = 0;
            // TODO:: брать с конфига игрока, но для сети не более 12
            int radius = 8;
            int chx = player.HitBox.ChunkPos.x;
            int chz = player.HitBox.ChunkPos.y;
            int x = 0;
            int z = 0;
            vec2i pos = player.HitBox.ChunkPos;
            player.LoadedChunks.Clear();

            //if (list.Contains(pos))
                player.LoadedChunks.Add(pos);

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

                        //if (list.Contains(pos))
                            player.LoadedChunks.Add(pos);
                    }
                }
            }

            i %= 4;

            for (r = 0; r < radius * 2; ++r)
            {
                x += xzDirectionsConst[i][0];
                z += xzDirectionsConst[i][1];
                pos = new vec2i(chx + x, chz + z);

                //if (list.Contains(pos))
                    player.LoadedChunks.Add(pos);
            }

            //foreach (vec2i pos2 in player.LoadedChunks)
            //{
            //    ChunkBase chunk = World.ChunkPr.LoadGenChunk(pos2);
            //    if (chunk != null)
            //    {
            //        World.ServerMain.ResponsePacket(player.SocketClient, new PacketS21ChunckData(chunk));
            //    }
            //}
            //player.LoadedChunks.Clear();
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
                player.Update();
            }

            //long var1 = this.theWorldServer.getTotalWorldTime();
            //int var3;
            //PlayerManager.PlayerInstance var4;

            //if (var1 - this.previousTotalWorldTime > 8000L)
            //{
            //    this.previousTotalWorldTime = var1;

            //    for (var3 = 0; var3 < this.playerInstanceList.size(); ++var3)
            //    {
            //        var4 = (PlayerManager.PlayerInstance)this.playerInstanceList.get(var3);
            //        var4.onUpdate();
            //        var4.processChunk();
            //    }
            //}
            //else
            //{
            //    for (var3 = 0; var3 < this.playerInstancesToUpdate.size(); ++var3)
            //    {
            //        var4 = (PlayerManager.PlayerInstance)this.playerInstancesToUpdate.get(var3);
            //        var4.onUpdate();
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
    }
}
