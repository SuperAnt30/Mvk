using MvkServer.Entity.Player;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Util;
using System.Collections;
using System.Net.Sockets;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект управления пользователями на сервере
    /// </summary>
    public class PlayerManager : ServerHeir
    {
        /// <summary>
        /// Массив игроков
        /// </summary>
        protected static Hashtable players = new Hashtable();

        public PlayerManager(Server server) : base(server) { }

        #region Player

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket(EntityPlayerServer entityPlayer, IPacket packet) 
            => ServerMain.ResponsePacket(entityPlayer.SocketClient, packet);


        /// <summary>
        /// Добавить игрока
        /// </summary>
        protected bool PlayerAdd(EntityPlayerServer entityPlayer)
        {
            if (!players.ContainsKey(entityPlayer.UUID))
            {
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

            //players.Clear();
        }

        /// <summary>
        /// Удалить игрока
        /// </summary>
        public void PlayerRemove(EntityPlayerServer player)
        {
            if (player != null && players.ContainsKey(player.UUID))
            {
                Logger.Log("server.player.entry.repeat {0}  [{1}]", player.Name, player.UUID);
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
        public int PlayerCount() => players.Count;

        #endregion

        /// <summary>
        /// Проверка запуска игрока
        /// </summary>
        public void LoginStart(EntityPlayerServer entityPlayer)
        {
            if (PlayerAdd(entityPlayer))
            {
                // норм!
                Logger.Log("server.player.entry {0} [{1}]", entityPlayer.Name, entityPlayer.UUID);
                ResponsePacket(entityPlayer, new PacketS12Success(entityPlayer.UUID));
            } else
            {
                Logger.Log("server.player.entry.repeat {0}  [{1}]", entityPlayer.Name, entityPlayer.UUID);
                // TODO::local error
                ResponsePacket(entityPlayer, new PacketS10Connection("Игрок с таким именем в игре!"));
            }
            return;
        }

        
        /// <summary>
        /// Обновляет все экземпляры игроков, которые необходимо обновить 
        /// </summary>
        public void UpdatePlayerInstances()
        {
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
