using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Entity;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Network.Packets.Client;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;

namespace MvkClient.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public ChunkProviderClient ChunkPrClient => ChunkPr as ChunkProviderClient;
        /// <summary>
        /// Мир для рендера и прорисовки
        /// </summary>
        public WorldRenderer WorldRender { get; protected set; }
        /// <summary>
        /// Менеджер прорисовки сущностей
        /// </summary>
        public RenderManager RenderEntityManager { get; protected set; }
        /// <summary>
        /// Список все объекты для этого клиента, как порожденные, так и непорожденные
        /// </summary>
        public MapListEntity EntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Содержит все объекты для этого клиента, которые не были созданы из-за отсутствия фрагмента. 
        /// Игра будет пытаться создать до 10 ожидающих объектов с каждым последующим тиком, 
        /// пока очередь появления не опустеет. 
        /// </summary>
        public MapListEntity EntitySpawnQueue { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список сущностей игроков
        /// </summary>
      //  public Hashtable PlayerEntities { get; protected set; } = new Hashtable();
        /// <summary>
        /// Объект нажатия клавиатуры
        /// </summary>
        public Keyboard Key { get; protected set; }

        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
       // protected InterpolationTime interpolation = new InterpolationTime();
        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;
        /// <summary>
        /// Количество прорисованных сущностей, для отладки
        /// </summary>
        protected int entitiesCountShow = 0;
        /// <summary>
        /// Объект заглушка
        /// </summary>
        private readonly object locker = new object();

        public WorldClient(Client client) : base()
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
           // interpolation.Start();
            WorldRender = new WorldRenderer(this);
            RenderEntityManager = new RenderManager(this);
            ClientMain.PlayerCreate(this);
            ClientMain.Player.SetOverviewChunk(Setting.OverviewChunk);
            Key = new Keyboard(this);
        }

        /// <summary>
        /// Игровое время
        /// </summary>
        public override uint GetTotalWorldTime() => ClientMain.TickCounter;

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            try
            {
               // interpolation.Restart();
                uint time = ClientMain.TickCounter;

                base.Tick();
                // Добавляем спавн новых сущностей
                while (EntitySpawnQueue.Count > 0) // count < 10 сделать до 10 сущностей в такт
                {
                    EntityBase entity = EntitySpawnQueue.FirstRemove();

                    if (!LoadedEntityList.ContainsValue(entity))
                    {
                        SpawnEntityInWorld(entity);
                    }
                }


                // Дополнительная чистка, если какие-то чанки не почистились!
                //if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
                //{
                //    previousTotalWorldTime = time;
                //    ChunkPrClient.FixOverviewChunk(ClientMain.Player);
                //}

                // Выгрузка чанков в тике
                ChunkPrClient.ChunksTickUnloadLoad();
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
                throw;
            }
        }

        /// <summary>
        /// Проверить загружены ли все ближ лижащие чанки кроме центра
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        public bool IsChunksSquareLoaded(vec2i pos)
        {
            for (int i = 0; i < MvkStatic.AreaOne8.Length; i++)
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(pos + MvkStatic.AreaOne8[i]);
                if (chunk == null || !chunk.IsChunkLoaded) return false;
            }
            return true; 
        }

        /// <summary>
        /// Остановка мира, удаляем все элементы
        /// </summary>
        public void StopWorldDelete()
        {
            ChunkPrClient.ClearAllChunks(false);
        }

        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// где 0 это начало, 1 финиш
        /// </summary>
        public float Interpolation() => ClientMain.Interpolation();

        /// <summary>
        /// Получить объект игрока по сети, по имени
        /// </summary>
        //public EntityPlayerMP GetPlayerMP(ushort id) => PlayerEntities.Get(id) as EntityPlayerMP;

        /// <summary>
        /// Возвращает сущностьь с заданным идентификатором или null, если он не существует в этом мире.
        /// </summary>
        public EntityBase GetEntityByID(ushort id)
        {
            if (id == ClientMain.Player.Id) return ClientMain.Player;
            return LoadedEntityList.Get(id);
        }

        /// <summary>
        /// Возвращает сущностьь с заданным идентификатором или null, если он не существует в этом мире.
        /// </summary>
        public EntityLiving GetEntityLivingByID(ushort id)
        {
            EntityBase entity = GetEntityByID(id);
            if (entity != null && entity is EntityLiving)
            {
                return (EntityLiving)entity;
            }
            return null;
        }

        /// <summary>
        /// Добавить сопоставление идентификатора сущности с entityHashSet
        /// </summary>
        public void AddEntityToWorld(ushort id, EntityBase entity)
        {
            EntityBase entityId = GetEntityByID(id);

            if (entityId == null)
            {
                SpawnEntityInWorld(entity);
            }

            //if (entityId != null) RemoveEntity(entityId);

            ////EntityList.Add(entity);
            ////entity.SetEntityId(id);

            //SpawnEntityInWorld(entity);
            //if (!SpawnEntityInWorld(entity))
            //{
            //    EntitySpawnQueue.Add(entity);
            //}
            //LoadedEntityList.Add(entity);
        }

        public EntityBase RemoveEntityFromWorld(ushort id)
        {
            EntityBase entity = GetEntityByID(id);
            if (entity != null)
            {
                EntityList.Remove(entity);
                RemoveEntity(entity);
            }

            return entity;
        }

        public void MouseDown(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                ClientMain.Player.HandAction();
            }
            else if(button == MouseButton.Right)
            {
                ClientMain.Player.HandActionTwo();
            }
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button)
        {
            ClientMain.Player.UndoHandAction();
        }

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="eBlock">тип блока</param>
        /// <returns>true смена была</returns>
        public override bool SetBlockState(BlockPos blockPos, BlockState blockState)
        {
            //ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
            //if (chunk != null)
            //{
            //    int bx = blockPos.X & 15;
            //    int by = blockPos.Y;
            //    int bz = blockPos.Z & 15;

            //    chunk.StorageArrays[by >> 4].Set(bx, by & 15, bz, blockState);

            //    MarkBlockForUpdate(blockPos);
            //}

            // Это если обновлять сразу!
            base.SetBlockState(blockPos, blockState);

            //if (base.SetBlockState(blockPos, blockState))
            //{
            //    // Для рендера, проверка соседнего чанка, если блок крайний,
            //    //// то будет доп рендер чанков рядом
            //    //vec3i min = blockPos.Position - 1;
            //    //vec3i max = blockPos.Position + 1;

            //    //vec3i c0 = new vec3i(min.x >> 4, min.y >> 4, min.z >> 4);
            //    //vec3i c1 = new vec3i(max.x >> 4, max.y >> 4, max.z >> 4);

            //    //AreaModifiedToRender(c0, c1);
            return true;
            //}
            //return false;
        }

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        public override void MarkBlockForUpdate(BlockPos blockPos)
        {
            vec3i min = blockPos.ToVec3i() - 1;
            vec3i max = blockPos.ToVec3i() + 1;

            vec3i c0 = new vec3i(min.x >> 4, min.y >> 4, min.z >> 4);
            vec3i c1 = new vec3i(max.x >> 4, max.y >> 4, max.z >> 4);

            AreaModifiedToRender(c0, c1);
        }

        /// <summary>
        /// Отметить блоки для обновления
        /// </summary>
        public override void MarkBlockRangeForRenderUpdate(int x0, int y0, int z0, int x1, int y1, int z1)
        {
            vec3i min = new vec3i(x0 - 1, y0 - 1, z0 - 1);
            vec3i max = new vec3i(x0 + 1, y0 + 1, z0 + 1);

            vec3i c0 = new vec3i(min.x >> 4, min.y >> 4, min.z >> 4);
            vec3i c1 = new vec3i(max.x >> 4, max.y >> 4, max.z >> 4);

            AreaModifiedToRender(c0, c1);
        }

        /// <summary>
        /// Сделать запрос перерендера выбранной облости псевдочанков
        /// </summary>
        public void AreaModifiedToRender(vec3i c0, vec3i c1)
        {
            for (int x = c0.x; x <= c1.x; x++)
            {
                for (int z = c0.z; z <= c1.z; z++)
                {
                    ChunkRender chunk = ChunkPrClient.GetChunkRender(new vec2i(x, z));
                    if (chunk != null && c0.y >= -1 && c1.y <= ChunkRender.COUNT_HEIGHT)
                    {
                        for (int y = c0.y; y <= c1.y; y++)
                        {
                            chunk.ModifiedToRender(y);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Пометить псевдо чанка на перегенерацию
        /// </summary>
        //public void ModifiedToRender(vec3i pos)
        //{
        //    if (pos.y >= 0 && pos.y < ChunkRender.COUNT_HEIGHT)
        //    {
        //        ChunkRender chunk = ChunkPrClient.GetChunkRender(new vec2i(pos.x, pos.z));
        //        if (chunk != null) chunk.ModifiedToRender(pos.y);
        //    }
        //}

        /// <summary>
        /// Получить попадает ли в луч сущность, выбрать самую близкую
        /// </summary>
        //public MovingObjectPosition RayCastEntity()
        //{
        //    float timeIndex = Interpolation();
        //    MovingObjectPosition moving = new MovingObjectPosition();
        //    if (ClientMain.Player.EntitiesLook.Length > 0)
        //    {
        //        EntityPlayerMP[] entities = ClientMain.Player.EntitiesLook.Clone() as EntityPlayerMP[];
        //        vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
        //        float dis = 1000f;
        //        foreach (EntityPlayerMP entity in entities)
        //        {
        //            float disR = glm.distance(pos, entity.GetPositionFrame(timeIndex));
        //            if (dis > disR)
        //            {
        //                dis = disR;
        //                moving = new MovingObjectPosition(entity);
        //            }
        //        }
        //    }
        //    return moving;
        //}

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public override void SendBlockBreakProgress(int breakerId, BlockPos blockPos, int progress)
        {
            ChunkRender chunk = ChunkPrClient.GetChunkRender(blockPos.GetPositionChunk());
            if (chunk != null)
            {
                if (progress >= 0 && progress < 10)
                {
                    chunk.DestroyBlockSet(breakerId, blockPos, progress);
                    ParticleDiggingBlock(blockPos, 1);
                }
                else
                {
                    if (progress == -2)
                    {
                        // Блок сломан
                        ParticleDiggingBlock(blockPos, 50);
                        //SpawnEntityInWorld(new EntityItem(this, blockPos.ToVec3(), new MvkServer.Item.ItemStack()
                    }
                    chunk.DestroyBlockRemove(breakerId);
                }
                chunk.ModifiedToRender(blockPos.GetPositionChunkY());
            }
        }

        /// <summary>
        /// Частички блока
        /// </summary>
        /// <param name="blockPos">позиция где рассыпаются частички</param>
        /// <param name="count">количество частичек</param>
        public void ParticleDiggingBlock(BlockPos blockPos, int count)
        {
            BlockBase block = GetBlock(blockPos);
            if (block != null && block.IsParticle)
            {
                vec3 pos = blockPos.ToVec3() + new vec3(.5f);
                for (int i = 0; i < count; i++)
                {
                    SpawnParticle(EnumParticle.Digging,
                        pos + new vec3((Rand.Next(16) - 8) / 16f, (Rand.Next(12) - 6) / 16f, (Rand.Next(16) - 8) / 16f),
                        new vec3(0),
                        (int)GetEBlock(blockPos));
                }
            }
        }

        /// <summary>
        /// Помечаем на перерендер всех псевдочанков обзора
        /// </summary>
        public void RerenderAllChunks()
        {
            if (ClientMain.Player.DistSqrt != null)
            {
                for (int i = 0; i < ClientMain.Player.DistSqrt.Length; i++)
                {
                    vec2i coord = new vec2i(Mth.Floor(ClientMain.Player.Position.x) >> 4, Mth.Floor(ClientMain.Player.Position.z) >> 4)
                        + ClientMain.Player.DistSqrt[i];
                    ChunkRender chunk = ChunkPrClient.GetChunkRender(coord);
                    if (chunk != null)
                    {
                        for (int y = 0; y < chunk.StorageArrays.Length; y++)
                        {
                            if (!chunk.StorageArrays[y].IsEmpty())
                            {
                                chunk.ModifiedToRender(y);
                            }
                        }
                    }
                }
            }
        }

        #region Entity

        protected override void OnEntityAdded(EntityBase entity)
        {
            base.OnEntityAdded(entity);
            EntitySpawnQueue.Remove(entity);
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected override void OnEntityRemoved(EntityBase entity)
        {
            base.OnEntityRemoved(entity);

            if (EntityList.ContainsValue(entity))
            {
                if (!entity.IsDead)
                {
                    EntitySpawnQueue.Add(entity);
                    LoadedEntityList.Remove(entity);
                }
                else
                {
                    EntityList.Remove(entity);
                }
            }
        }

        /// <summary>
        /// Запланировать удаление сущности в следующем тике
        /// </summary>
        public override void RemoveEntity(EntityBase entity)
        {
            base.RemoveEntity(entity);
            EntityList.Remove(entity);
        }

        /// <summary>
        /// Удалить всех сущностей
        /// </summary>
        private void RemoveAllEntities()
        {
            LoadedEntityList.RemoveRange(UnloadedEntityList);
            while (UnloadedEntityList.Count > 0)
            {
                EntityBase entity = UnloadedEntityList.FirstRemove();
                if (entity.AddedToChunk)
                {
                    ChunkBase chunk = ChunkPrClient.GetChunk(entity.PositionChunk);
                    if (chunk != null) chunk.RemoveEntity(entity);
                }
                OnEntityRemoved(entity);
            }
            while(LoadedEntityList.Count > 0)
            {
                EntityBase entity = LoadedEntityList.FirstRemove();
                if (entity.IsDead && entity.AddedToChunk)
                {
                    ChunkBase chunk = ChunkPrClient.GetChunk(entity.PositionChunk);
                    if (chunk != null) chunk.RemoveEntity(entity);
                }
                OnEntityRemoved(entity);
            }
        }

        /// <summary>
        /// Респавн игрока
        /// </summary>
        public void Respawn()
        {
            RemoveAllEntities();
            ClientMain.TrancivePacket(new PacketC16ClientStatus(PacketC16ClientStatus.EnumState.Respawn));
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        public override void SpawnEntityInWorld(EntityBase entity)
        {
            base.SpawnEntityInWorld(entity);
            EntityList.Add(entity);
            //EntitySpawnQueue.Add(entity);
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public override void SpawnParticle(EnumParticle particle, vec3 pos, vec3 motion, params int[] items) 
            => ClientMain.EffectRender.SpawnParticle(particle, pos, motion, items);

        protected override void UpdateEntity(EntityBase entity)
        {
            entity.UpdateClient();
            // Проверка толчка
            if (entity is EntityPlayerClient entityPlayer) entityPlayer.CheckPush();

            base.UpdateEntity(entity);
        }

        #endregion

        #region Debug

        public void CountEntitiesShowBegin() => entitiesCountShow = 0;
        public void CountEntitiesShowAdd() => entitiesCountShow++;

        #endregion

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("t {2} {0} E:{4}/{5}\r\n{1}\r\n@!{6}/{7}\r\nParticles: {8}\r\n{3}",
                ChunkPrClient.ToString(), // 0
                ClientMain.Player,  // 1
                ClientMain.TickCounter / 20,  // 2
                ClientMain.Player.Inventory, // 3
                PlayerEntities.Count + 1, // 4
                entitiesCountShow, // 5
                EntityList.Count, // 6
                base.ToStringDebug(), // 7
                ClientMain.EffectRender.CountParticles() // 8
            );
        }
    }
}
