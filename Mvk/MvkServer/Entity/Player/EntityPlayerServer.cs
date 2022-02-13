using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока для сервера
    /// </summary>
    public class EntityPlayerServer : EntityPlayer
    {
        /// <summary>
        /// Сетевой сокет клиента
        /// </summary>
        public Socket SocketClient { get; protected set; }
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }

        /// <summary>
        /// Cписок, содержащий все чанки которые нужны клиенту согласно его обзору для загрузки
        /// </summary>
        public MapListVec2i LoadedChunks { get; protected set; } = new MapListVec2i();

        /// <summary>
        /// Пинг клиента в мс
        /// </summary>
        public int Ping { get; protected set; } = -1;

        // protected long pingPrev;

        protected Profiler profiler;

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name, WorldBase world) : base()
        {
            World = world; 
            ServerMain = server;
            SocketClient = socket;
            Name = name;
            UUID = GetHash(name);
            profiler = new Profiler(server.Log);
        }

        /// <summary>
        /// Задать порядковый номер на сервере
        /// </summary>
        public void SetId(ushort id) => Id = id;

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time) => Ping = (Ping * 3 + (int)(ServerMain.Time() - time)) / 4;

        /// <summary>
        /// Задать положение сидя и на земле ли
        /// </summary>
        public void SetSneakOnGround(bool sneaking, bool onGround)
        {
            IsSneaking = sneaking;
            OnGround = onGround;
            isMotionServer = true;
        }

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        protected string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        protected int pingChunk = 0;

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            // Tут base.Update не надо, так-как это обрабатывается на клиенте, 
            // тут отправление перемещение игрокам если оно надо

            if (isMotionServer)
            {
                // TODO:: скорее всего надо вынести в Update уровень выше, чтоб и мобы так же работали
                // было изменение, надо отправить данные всем клиентам кроме тикущего
                ServerMain.World.Players.ResponsePacketAll(new PacketS14EntityMotion(this), Id);
                isMotionServer = false;
            }
        }

        /// <summary>
        /// Вызывается только на сервере у игроков для передачи перемещения
        /// </summary>
        public void UpdatePlayer()
        {
            try
            {
                int i = 0;
                pingChunk--;

                // TODO::2022-02-08 протестировать корректность загрузки чанков по пингу, именно по сети!
                while (LoadedChunks.Count > 0 && i < MvkGlobal.COUNT_PACKET_CHUNK_TPS && pingChunk <= 0)
                {
                    profiler.StartSection("LoadChunk");
                    // передача пакетов чанков по сети
                    vec2i pos = LoadedChunks.FirstRemove();
                    ChunkBase chunk = ServerMain.World.ChunkPrServ.LoadChunk(pos);

                    if (chunk != null)
                    {
                        profiler.EndStartSection("PacketS21ChunckData");
                        PacketS21ChunckData packet = new PacketS21ChunckData(chunk);
                        profiler.EndStartSection("ResponsePacket");
                        ServerMain.ResponsePacket(SocketClient, packet);

                        EntityLiving[] entities = chunk.GetEntities();
                        if (entities.Length > 0)
                        {
                            for(int e = 0; e < entities.Length; e++)
                            {
                                ServerMain.ResponsePacket(SocketClient, new PacketS0CSpawnPlayer((EntityPlayer)entities[e]));
                            }
                        }

                        i++;
                        // TODO:: Нужен алгорит загрузки чанков по пингу
                        pingChunk = Ping > 50 ? 10 : 0;
                    }
                    profiler.EndSection();
                }

                if (isMotionServer)
                {
                    //ServerMain.World.Players.ResponsePacketAll(
                    //    new PacketB20Player().PositionYawPitch(Position, RotationYawHead, RotationYaw, RotationPitch, IsSneaking, OnGround, Id),
                    //    Id
                    //);
                    profiler.StartSection("UpdateMountedMovingPlayer");
                    // обновлять фрагменты чанков вокруг игрока, перемещаемого логикой сервера
                    ((WorldServer)World).Players.UpdateMountedMovingPlayer(this);
                    profiler.EndSection();
                   // isMotionServer = false;
                }

            }
            catch (Exception e)
            {
                ServerMain.Log.Error("EntityPlayerServer.Update {0}", e.Message);
                throw;
            }
        }
    }
}
