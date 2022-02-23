using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections;

namespace MvkServer.World
{
    /// <summary>
    /// Базовый объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }
        /// <summary>
        /// Объект отладки по задержке в лог
        /// </summary>
        protected Profiler profiler;

        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public CollisionBase Collision { get; protected set; }

        /// <summary>
        /// Список всех сущностей во всех загруженных в данный момент чанках 
        /// </summary>
        public MapListEntity LoadedEntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список сущностей которые надо выгрузить
        /// </summary>
        public MapListEntity UnloadedEntityList { get; protected set; } = new MapListEntity();
        /// <summary>
        /// Список игроков в мире
        /// </summary>
        public MapListEntity PlayerEntities { get; protected set; } = new MapListEntity();

        public Random Rand { get; private set; }

        protected WorldBase()
        {
            Collision = new CollisionBase(this);
            Log = new Logger();
            profiler = new Profiler(Log);
            Rand = new Random();
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public virtual void Tick()
        {

        }

        #region Entity

        /// <summary>
        /// Загрузить коллекцию сущностей
        /// </summary>
        public void LoadEntities(MapListEntity entityCollection)
        {
            LoadedEntityList.AddRange(entityCollection);
            for (int i = 0; i < entityCollection.Count; i++)
            {
                OnEntityAdded((EntityLiving)entityCollection.GetAt(i));
            }
        }

        /// <summary>
        /// Выгрузить коллекцию сущностей
        /// </summary>
        public void UnloadEntities(MapListEntity entities) => UnloadedEntityList.AddRange(entities);

        /// <summary>
        /// Обновляет (и очищает) объекты и объекты чанка 
        /// </summary>
        public virtual void UpdateEntities()
        {
            profiler.StartSection("EntitiesRemove");

            // TODO:: повтор?!!! #140
            LoadedEntityList.RemoveRange(UnloadedEntityList);

            for (int i = 0; i < UnloadedEntityList.Count; i++)
            {
                EntityLiving entity = (EntityLiving)UnloadedEntityList.GetAt(i);
                if (entity.AddedToChunk && ChunkPr.IsChunk(entity.PositionChunk))
                {
                    GetChunk(entity.PositionChunk).RemoveEntity(entity);
                }
                OnEntityRemoved(entity);
            }

            UnloadedEntityList.Clear();

            profiler.EndStartSection("EntityTick");

            // колекция для удаления
            MapListEntity entityRemove = new MapListEntity();
            for (int i = 0; i < LoadedEntityList.Count; i++)
            {
                EntityLiving entity = (EntityLiving)LoadedEntityList.GetAt(i);

                if (!entity.IsDead)
                {
                    try
                    {
                        UpdateEntity(entity);
                    }
                    catch (Exception ex)
                    {
                        Logger.Crach(ex);
                        throw;
                    }
                }
                else 
                {
                    entityRemove.Add(entity);
                }
            }

            profiler.EndStartSection("EntityRemove");

            // Удаляем
            while (entityRemove.Count > 0)
            {
                EntityLiving entity = (EntityLiving)entityRemove.FirstRemove();
                if (entity.AddedToChunk && ChunkPr.IsChunk(entity.PositionChunk))
                {
                    GetChunk(entity.PositionChunk).RemoveEntity(entity);
                }
                LoadedEntityList.Remove(entity);
                OnEntityRemoved(entity);
            }

            profiler.EndSection();
        }

        protected virtual void UpdateEntity(EntityLiving entity)
        {
            vec2i posCh = entity.GetChunkPos();
            //byte var5 = 32;

            // Проверка о наличии соседних чанков
            //if (IsAreaLoaded(x - var5, 0, z - var5, x + var5, 0, z + var5, true))
            {
                if (entity.AddedToChunk)
                {
                    entity.TicksExistedMore();
                    entity.Update();
                }

                //posCh = entity.GetChunkPos();
                float y = entity.GetChunkY(); 

                if (!entity.AddedToChunk || !posCh.Equals(entity.PositionChunk) || y != entity.PositionChunkY)
                {
                    if (entity.AddedToChunk && ChunkPr.IsChunk(entity.PositionChunk))
                    {
                        // Удаляем из старого псевдо чанка
                        GetChunk(entity.PositionChunk).RemoveEntityAtIndex(entity, entity.PositionChunkY);
                    }
                    if (ChunkPr.IsChunk(posCh))
                    {
                        // Добавляем в новый псевдочанк
                        entity.AddedToChunk = true;
                        GetChunk(posCh).AddEntity(entity);
                    }
                    else
                    {
                        // Если нет чанка помечаем что сущность без чанка
                        entity.AddedToChunk = false;
                    }
                }
            }
        }

        protected virtual void OnEntityAdded(EntityLiving entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(var2)).OnEntityAdded(entity);
            //}
        }

        protected virtual void OnEntityRemoved(EntityLiving entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(i)).OnEntityRemoved(entity);
            //}
        }

        /// <summary>
        /// Запланировать удаление сущности в следующем тике
        /// </summary>
        public virtual void RemoveEntity(EntityLiving entity)
        {
            entity.SetDead();
            if (entity is EntityPlayer)
            {
                PlayerEntities.Remove(entity);
                // Флаг сна всех игроков
                //UpdateAllPlayersSleepingFlag();
                OnEntityRemoved(entity);
            }
        }

        /// <summary>
        /// Вызывается, когда объект появляется в мире. Это включает в себя игроков
        /// </summary>
        public virtual bool SpawnEntityInWorld(EntityLiving entity)
        {
            vec2i posCh = entity.GetChunkPos();
            bool flagSpawn = entity.FlagSpawn;

            if (entity is EntityPlayer) flagSpawn = true;


            //ChunkBase chunk = null;
            bool isChunk = false;
            //if (this is WorldServer)
            //{
            //    chunk = ((WorldServer)this).ChunkPrServ.LoadChunk(posCh);
            //    isChunk = chunk != null;
            //}
            //else
            {
                isChunk = ChunkPr.IsChunk(posCh);
            }
            
            //if (!flagSpawn && !isChunk)
            //{
            //    return false;
            //}
            //else
            {
                if (entity is EntityPlayer entityPlayer)
                {
                    PlayerEntities.Add(entityPlayer);
                    // Флаг сна всех игроков
                    //UpdateAllPlayersSleepingFlag();
                }

                //if (!isChunk)
                ChunkBase chunk = GetChunk(posCh);
                if (chunk != null) chunk.AddEntity(entity);
                else
                {
                    
                   // return false;
                }
                LoadedEntityList.Add(entity);
                OnEntityAdded(entity);
                return true;
            }
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public virtual void SpawnParticle(EnumParticle particle, vec3 pos, vec3 motion, params int[] items) { }
        

        #endregion

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public virtual string ToStringDebug()
        {
            return string.Format("{0}/{1}", LoadedEntityList.Count, UnloadedEntityList.Count);
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(vec2i pos) => ChunkPr.GetChunk(pos);

        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="bpos">глобальная позиция блока</param>
        public BlockBase GetBlock(BlockPos bpos) => GetBlock(bpos.Position);
        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="pos">глобальная позиция блока</param>
        public BlockBase GetBlock(vec3i pos)
        {
            if (pos.y >= 0 && pos.y <= 255)
            {
                ChunkBase chunk = GetChunk(new vec2i(pos.x >> 4, pos.z >> 4));
                if (chunk != null)
                {
                    return chunk.GetBlock0(new vec3i(pos.x & 15, pos.y, pos.z & 15));
                }
            }
            return Blocks.GetAir(pos);
        }

        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью
        /// </summary>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public MovingObjectPosition RayCast(vec3 a, vec3 dir, float maxDist)
        {
            float px = a.x;
            float py = a.y;
            float pz = a.z;

            float dx = dir.x;
            float dy = dir.y;
            float dz = dir.z;

            float t = 0.0f;
            int ix = Mth.Floor(px);
            int iy = Mth.Floor(py);
            int iz = Mth.Floor(pz);

            int stepx = (dx > 0.0f) ? 1 : -1;
            int stepy = (dy > 0.0f) ? 1 : -1;
            int stepz = (dz > 0.0f) ? 1 : -1;

            float infinity = float.MaxValue;

            float txDelta = (dx == 0.0f) ? infinity : Mth.Abs(1.0f / dx);
            float tyDelta = (dy == 0.0f) ? infinity : Mth.Abs(1.0f / dy);
            float tzDelta = (dz == 0.0f) ? infinity : Mth.Abs(1.0f / dz);

            float xdist = (stepx > 0) ? (ix + 1 - px) : (px - ix);
            float ydist = (stepy > 0) ? (iy + 1 - py) : (py - iy);
            float zdist = (stepz > 0) ? (iz + 1 - pz) : (pz - iz);

            float txMax = (txDelta < infinity) ? txDelta * xdist : infinity;
            float tyMax = (tyDelta < infinity) ? tyDelta * ydist : infinity;
            float tzMax = (tzDelta < infinity) ? tzDelta * zdist : infinity;

            int steppedIndex = -1;

            while (t <= maxDist)
            {
                BlockBase block = GetBlock(new vec3i(ix, iy, iz));
                if (block.CollisionRayTrace(a, dir, maxDist))
                {
                    vec3 end;
                    vec3i norm;
                    vec3i iend;

                    end.x = px + t * dx;
                    end.y = py + t * dy;
                    end.z = pz + t * dz;

                    iend.x = ix;
                    iend.y = iy;
                    iend.z = iz;

                    norm.x = norm.y = norm.z = 0;
                    if (steppedIndex == 0) norm.x = -stepx;
                    if (steppedIndex == 1) norm.y = -stepy;
                    if (steppedIndex == 2) norm.z = -stepz;

                    //if (EntityDis != null)
                    //{
                    //    if (t < EntityDis.Distance)
                    //    {
                            return new MovingObjectPosition(block, iend, norm, end);
                    //    }
                    //    return new MovingObjectPosition(EntityDis.Entity);
                    //}
                }
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ix += stepx;
                        t = txMax;
                        txMax += txDelta;
                        steppedIndex = 0;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        iy += stepy;
                        t = tyMax;
                        tyMax += tyDelta;
                        steppedIndex = 1;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
            }
            return new MovingObjectPosition();
        }
    }
}
