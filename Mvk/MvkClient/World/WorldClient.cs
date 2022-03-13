using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Entity;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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


        private Hashtable damagedBlocks = new Hashtable();

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
                    EntityLiving entity = (EntityLiving)EntitySpawnQueue.FirstRemove();

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
        public EntityLiving GetEntityByID(ushort id)
        {
            if (id == ClientMain.Player.Id) return ClientMain.Player;
            return LoadedEntityList.Get(id) as EntityLiving;
        }

        /// <summary>
        /// Добавить сопоставление идентификатора сущности с entityHashSet
        /// </summary>
        public void AddEntityToWorld(ushort id, EntityLiving entity)
        {
            EntityLiving entityId = GetEntityByID(id);

            if (entityId != null) RemoveEntity(entityId);

            EntityList.Add(entity);
            entity.SetEntityId(id);

            if (!SpawnEntityInWorld(entity))
            {
                EntitySpawnQueue.Add(entity);
            }

            LoadedEntityList.Add(entity);
        }

        public EntityLiving RemoveEntityFromWorld(ushort id)
        {
            EntityLiving entity = GetEntityByID(id);
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
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                ClientMain.Player.UndoHandAction();
            }
        }

    /// <summary>
    /// Сменить блок
    /// </summary>
    /// <param name="blockPos">позици блока</param>
    /// <param name="eBlock">тип блока</param>
    /// <returns>true смена была</returns>
    public override bool SetBlockState(BlockPos blockPos, EnumBlock eBlock)
        {
            if (base.SetBlockState(blockPos, eBlock))
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    chunk.ModifiedToRender(blockPos.GetPositionChunkY());
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Получить попадает ли в луч сущность, выбрать самую близкую
        /// </summary>
        public MovingObjectPosition RayCastEntity()
        {
            float timeIndex = Interpolation();
            // TODO::RayCastEntity ЗАМЕНИТЬ!!!
            MovingObjectPosition moving = new MovingObjectPosition();
            if (ClientMain.Player.EntitiesLook.Length > 0)
            {
                EntityPlayerMP[] entities = ClientMain.Player.EntitiesLook.Clone() as EntityPlayerMP[];
                vec3 pos = ClientMain.Player.GetPositionFrame(timeIndex);
                float dis = 1000f;
                foreach (EntityPlayerMP entity in entities)
                {
                    float disR = glm.distance(pos, entity.GetPositionFrame(timeIndex));
                    if (dis > disR)
                    {
                        dis = disR;
                        moving = new MovingObjectPosition(entity);
                    }
                }
            }
            return moving;
        }

        public int GetDamagedBlocksValue(vec3i pos)
        {
            if (damagedBlocks.Count > 0)
            {
                foreach(DestroyBlockProgress destroy in damagedBlocks.Values)
                {
                    if (pos.Equals(destroy.Position.Position))
                    {
                        return destroy.PartialBlockProgress;
                    }
                }
            }
            return -1;
        }


        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public override void SendBlockBreakProgress(int breakerId, BlockPos blockPos, int progress)
        {
            if (progress >= 0 && progress < 10)
            {
                DestroyBlockProgress destroy;
                if (damagedBlocks.ContainsKey(breakerId))
                {
                    destroy = (DestroyBlockProgress)damagedBlocks[breakerId];
                } else
                {
                    destroy = new DestroyBlockProgress(blockPos);
                    damagedBlocks.Add(breakerId, destroy);
                }
                destroy.SetPartialBlockDamage(progress);
                destroy.SetCloudUpdateTick(ClientMain.TickCounter);
                // Частицы
                vec3 pos = blockPos.ToVec3() + new vec3(.5f);
                SpawnParticle(EnumParticle.Digging, pos + new vec3((Rand.Next(16) - 8) / 16f, (Rand.Next(12) - 6) / 16f, (Rand.Next(16) - 8) / 16f), new vec3(0));
            }
            else
            {
                damagedBlocks.Remove(breakerId);
            }
            ChunkRender chunk = ChunkPrClient.GetChunkRender(blockPos.GetPositionChunk());
            chunk.ModifiedToRender(blockPos.GetPositionChunkY());
        }

        #region Entity

        protected override void OnEntityAdded(EntityLiving entity)
        {
            base.OnEntityAdded(entity);
            EntitySpawnQueue.Remove(entity);
        }

        protected override void OnEntityRemoved(EntityLiving entity)
        {
            base.OnEntityRemoved(entity);

            if (EntityList.ContainsValue(entity))
            {
                if (!entity.IsDead)
                {
                    EntitySpawnQueue.Add(entity);
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
        public override void RemoveEntity(EntityLiving entity)
        {
            base.RemoveEntity(entity);
            EntityList.Remove(entity);
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        public override bool SpawnEntityInWorld(EntityLiving entity)
        {
            bool spawn = base.SpawnEntityInWorld(entity);
            EntityList.Add(entity);
            if (!spawn) EntitySpawnQueue.Add(entity);
            return spawn;
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public override void SpawnParticle(EnumParticle particle, vec3 pos, vec3 motion, params int[] items) 
            => ClientMain.EffectRender.SpawnParticle(particle, pos, motion, items);

        protected override void UpdateEntity(EntityLiving entity)
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
            return string.Format("t {2} {0} E:{4}/{5}\r\n{1}\r\n@!{6}/{7}\r\nParticles: {8}",
                ChunkPrClient.ToString(), // 0
                ClientMain.Player,  // 1
                ClientMain.TickCounter / 20,  // 2
                "", // 3
                PlayerEntities.Count + 1, // 4
                entitiesCountShow, // 5
                EntityList.Count, // 6
                base.ToStringDebug(), // 7
                ClientMain.EffectRender.CountParticles() // 8
            );
        }
    }
}
