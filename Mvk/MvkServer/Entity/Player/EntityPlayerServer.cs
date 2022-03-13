using MvkServer.Glm;
using MvkServer.Management;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;
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
        /// Список чанков которые нужно проверить на загрузку или генерацию,
        /// должен формироваться по дистанции от игрока
        /// </summary>
        public MapListVec2i LoadingChunks { get; protected set; } = new MapListVec2i();
        /// <summary>
        /// Пинг клиента в мс
        /// </summary>
        public int Ping { get; protected set; } = -1;

        /// <summary>
        /// ItemInWorldManager, принадлежащий этому игроку
        /// </summary>
        public ItemInWorldManager TheItemInWorldManager { get; private set; }

        private Profiler profiler;

        /// <summary>
        /// Список сущностей не игрок, которые в ближайшем тике будут удалены
        /// </summary>
        private MapListId destroyedItemsNetCache = new MapListId();

        //private List<vec2i> LoadingChunks = new List<vec2i>();

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name, WorldServer world) : base(world)
        {
            ServerMain = server;
            SocketClient = socket;
            Name = name;
            UUID = GetHash(name);
            profiler = new Profiler(server.Log);
            TheItemInWorldManager = new ItemInWorldManager(world, this);
        }

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
            ActionAdd(EnumActionChanged.IsSneaking);
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
            TheItemInWorldManager.UpdateBlockRemoving();


            // если нет хп обновлям смертельную картинку
            if (Health <= 0f) DeathUpdate();

            // Отправляем запрос на удаление сущностей которые не видим
            if (destroyedItemsNetCache.Count > 0)
            {
                List<ushort> ids = new List<ushort>();
                while (destroyedItemsNetCache.Count > 0)
                {
                    ids.Add(destroyedItemsNetCache.FirstRemove());
                }
                SendPacket(new PacketS13DestroyEntities(ids.ToArray()));
            }

            UpdatePlayer();
        }

        /// <summary>
        /// Вызывается только на сервере у игроков для передачи перемещения
        /// </summary>
        private void UpdatePlayer()
        {
            try
            {
                int i = 0;
                pingChunk--;

                List<vec2i> loadedNull = new List<vec2i>();
                // TODO::2022-02-08 протестировать корректность загрузки чанков по пингу, именно по сети!
                while (LoadedChunks.Count > 0 && i < MvkGlobal.COUNT_PACKET_CHUNK_TPS && pingChunk <= 0)
                {
                    profiler.StartSection("LoadChunk");
                    // передача пакетов чанков по сети
                    vec2i pos = LoadedChunks.FirstRemove();
                    ChunkBase chunk = World.GetChunk(pos);
                        //ServerMain.World.ChunkPrServ.LoadChunk(pos);

                    // NULL по сути не должен быть!!!
                    if (chunk != null && chunk.IsChunkLoaded)
                    {
                        profiler.EndStartSection("PacketS21ChunckData");
                        PacketS21ChunkData packet = new PacketS21ChunkData(chunk);
                        profiler.EndStartSection("ResponsePacket");
                        SendPacket(packet);

                        i++;
                        // TODO:: Нужен алгорит загрузки чанков по пингу
                        pingChunk = Ping > 50 ? 10 : 0;
                    } else
                    {
                        loadedNull.Add(pos);
                    }
                    profiler.EndSection();
                }

                // Если чанки не попали, мы возращаем в массив
                int count = loadedNull.Count;
                if (count > 0) {
                    count--;
                    for (int j = count; j >= 0; j--)
                    {
                        LoadedChunks.Insert(loadedNull[j]);
                    }
                }

                if (ActionChanged.HasFlag(EnumActionChanged.Position) || !SameOverviewChunkPrev())
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
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Сущность которую надо удалить у клиента
        /// </summary>
        public void SendRemoveEntity(EntityLiving entity)
        {
            if (entity is EntityPlayer)
            {
                SendPacket(new PacketS13DestroyEntities(new ushort[] { entity.Id }));
            }
            else
            {
                destroyedItemsNetCache.Add(entity.Id);
            }
        }

        /// <summary>
        /// Обновить обзор прошлого такта
        /// </summary>
        public void UpOverviewChunkPrev()
        {
            OverviewChunkPrev = OverviewChunk;
            DistSqrt = MvkStatic.GetSqrt(OverviewChunk + 1);
        }

        /// <summary>
        /// Отправить сетевой пакет этому игроку
        /// </summary>
        public void SendPacket(IPacket packet) => ServerMain.ResponsePacket2(SocketClient, packet);

        

        public override string ToString()
        {
            return "#" + Id + " " + Name + "\r\n" + base.ToString();
        }
    }
}
