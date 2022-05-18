using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Management;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
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
        /// <summary>
        /// Флаг для загрузки всех ближайших сущностей в трекере
        /// 0 - старт, 1 - загрузка, 2 - далее, нет загрузки
        /// </summary>
        public byte TrackerBeginFlag { get; private set; } = 0;

        private Profiler profiler;

        /// <summary>
        /// Список сущностей не игрок, которые в ближайшем тике будут удалены
        /// </summary>
        private MapListId destroyedItemsNetCache = new MapListId();

        protected int pingChunk = 0;

        /// <summary>
        /// Почледнее активное время игрока
        /// </summary>
        protected uint playerLastActiveTime = 0;

        //private List<vec2i> LoadingChunks = new List<vec2i>();

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name, WorldServer world) : base(world)
        {
            ServerMain = server;
            SocketClient = socket;
            base.name = name;
            UUID = GetHash(name);
            profiler = new Profiler(server.Log);
            IsCreativeMode = world.IsCreativeMode;
            TheItemInWorldManager = new ItemInWorldManager(world, this);
        }

        /// <summary>
        /// Задать время пинга
        /// </summary>
        public void SetPing(long time) => Ping = (Ping * 3 + (int)(ServerMain.Time() - time)) / 4;

        /// <summary>
        /// Задать положение сидя и бега
        /// </summary>
        public void SetSneakingSprinting(bool sneaking, bool sprinting)
        {
            if (IsSneaking() != sneaking)
            {
                SetSneaking(sneaking);
                if (sneaking) Sitting(); else Standing();
            }
            SetSprinting(sprinting);
            ActionAdd(EnumActionChanged.IsSneaking);
            ActionAdd(EnumActionChanged.IsSprinting);
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


        /// <summary>
        /// Надо сделать респавн в ближайшем тике
        /// </summary>
        public override void Respawn()
        {
            base.Respawn();
            destroyedItemsNetCache.Clear();
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            if (TrackerBeginFlag < 2) TrackerBeginFlag++;

            // Tут base.Update не надо, так-как это обрабатывается на клиенте, 
            // тут отправление перемещение игрокам если оно надо

            EntityUpdateLocation();

            EntityUpdateServer();

            profiler.StartSection("TheItemInWorldManager.UpdateBlock");
            TheItemInWorldManager.UpdateBlock();
            profiler.EndSection();

            // если нет хп обновлям смертельную картинку
            if (Health <= 0f) DeathUpdate();

            // Обновления предметов которые могут видеть игроки, что в руке, броня
            UpdateItems();

            if (Health > 0 && !World.IsRemote)
            {
                AxisAlignedBB axis = BoundingBox.Expand(new vec3(1, .5f, 1));
                MapListEntity map = World.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityItem, axis, Id);
                while (map.Count > 0)
                {
                    EntityBase entity = map.FirstRemove();
                    if (!entity.IsDead && entity is EntityItem entityItem)
                    {
                        entityItem.OnCollideWithPlayer(this);
                    }
                }
            }

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
                // 2022-02-08 протестировать корректность загрузки чанков по пингу, именно по сети!
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
                        int flag = 0;
                        int heightMax = chunk.GetTopFilledSegment() >> 4;
                        for (int y = 0; y <= heightMax; y++) { flag |= 1 << y; }
                        
                        PacketS21ChunkData packet = new PacketS21ChunkData(chunk, true, flag);
                        profiler.EndStartSection("ResponsePacket");
                        SendPacket(packet);

                        i++;
                        // Нужен алгорит загрузки чанков по пингу
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
        public void SendRemoveEntity(EntityBase entity)
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

        /// <summary>
        /// Пометка активности игрока
        /// </summary>
        public void MarkPlayerActive() => playerLastActiveTime = ServerMain.TickCounter;

        /// <summary>
        /// Обновить инвентарь игрока
        /// </summary>
        public void SendUpdateInventory() => SendPacket(new PacketS30WindowItems(Inventory.GetMainAndArmor()));

        public override string ToString()
        {
            return "#" + Id + " " + name + "\r\n" + base.ToString();
        }
    }
}
