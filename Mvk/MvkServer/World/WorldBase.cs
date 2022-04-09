using MvkServer.Entity;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections;
using System.Collections.Generic;

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

        public Random Rand { get; protected set; }

        /// <summary>
        /// Это значение true для клиентских миров и false для серверных миров.
        /// </summary>
        public bool IsRemote { get; protected set; } = true;

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
            // колекция для удаления
            MapListEntity entityRemove = new MapListEntity();

            profiler.StartSection("EntitiesUnloadedList");

            // Выгружаем сущности которые имеются в списке выгрузки
            LoadedEntityList.RemoveRange(UnloadedEntityList);
            entityRemove.AddRange(UnloadedEntityList);
            UnloadedEntityList.Clear();

            profiler.EndStartSection("EntityTick");
            
            
            // Пробегаем по всем сущностям и обрабатываеи их такт
            for (int i = 0; i < LoadedEntityList.Count; i++)
            {
                EntityBase entity = LoadedEntityList.GetAt(i);

                if (entity != null)
                {
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
            }

            profiler.EndStartSection("EntityRemove");

            // Удаляем 
            while (entityRemove.Count > 0)
            {

                if (IsRemote)
                {
                    bool b = true;
                }
                EntityBase entity = entityRemove.FirstRemove();
                if (entity.AddedToChunk && ChunkPr.IsChunk(entity.PositionChunk))
                {
                    GetChunk(entity.PositionChunk).RemoveEntity(entity);
                }
                LoadedEntityList.Remove(entity);
                OnEntityRemoved(entity);
            }

            profiler.EndSection();
        }

        protected virtual void UpdateEntity(EntityBase entity)
        {
            
            //byte var5 = 32;

            // Проверка о наличии соседних чанков
            //if (IsAreaLoaded(x - var5, 0, z - var5, x + var5, 0, z + var5, true))
            {
                if (entity.AddedToChunk)
                {
                    entity.Update();
                }

                vec2i posCh = entity.GetChunkPos();

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

        protected virtual void OnEntityAdded(EntityBase entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(var2)).OnEntityAdded(entity);
            //}
        }

        /// <summary>
        /// Вызывается для всех World, когда сущность выгружается или уничтожается. 
        /// В клиентских мирах освобождает любые загруженные текстуры.
        /// В серверных мирах удаляет сущность из трекера сущностей.
        /// </summary>
        protected virtual void OnEntityRemoved(EntityBase entity)
        {
            //for (int i = 0; i < this.worldAccesses.size(); ++i)
            //{
            //    ((IWorldAccess)this.worldAccesses.get(i)).OnEntityRemoved(entity);
            //}
        }

        /// <summary>
        /// Запланировать удаление сущности в следующем тике
        /// </summary>
        public virtual void RemoveEntity(EntityBase entity)
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
        public virtual void SpawnEntityInWorld(EntityBase entity)
        {
            vec2i posCh = entity.GetChunkPos();
           // bool flagSpawn = entity.FlagSpawn;

           // if (entity is EntityPlayer) flagSpawn = true;

            //ChunkBase chunk = null;
           // bool isChunk = false;
            //if (this is WorldServer)
            //{
            //    chunk = ((WorldServer)this).ChunkPrServ.LoadChunk(posCh);
            //    isChunk = chunk != null;
            //}
            //else
            //{
               // isChunk = ChunkPr.IsChunk(posCh);
           // }
            
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

                ChunkBase chunk;
                //if (!isChunk)\
                //if (this is WorldServer worldServer)
                //{
                //    chunk = worldServer.ChunkPrServ.LoadChunk(posCh);
                //} else {
                    chunk = GetChunk(posCh);
                //}
                if (chunk != null) chunk.AddEntity(entity);
                else
                {
                    
                   // return false;
                }
                LoadedEntityList.Add(entity);
                OnEntityAdded(entity);
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
        /// Проверить наличие чанка
        /// </summary>
        //public bool IsChunk(vec2i pos) => ChunkPr.IsChunk(pos);

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
            return Blocks.CreateAir(pos);
        }
        /// <summary>
        /// Получить тип блока
        /// </summary>
        /// <param name="bpos">глобальная позиция блока</param>
        public EnumBlock GetEBlock(BlockPos bpos) => GetEBlock(bpos.Position);
        /// <summary>
        /// Получить тип блока
        /// </summary>
        /// <param name="pos">глобальная позиция блока</param>
        public EnumBlock GetEBlock(vec3i pos)
        {
            if (pos.y >= 0 && pos.y <= 255)
            {
                ChunkBase chunk = GetChunk(new vec2i(pos.x >> 4, pos.z >> 4));
                if (chunk != null)
                {
                    return chunk.GetEBlock(new vec3i(pos.x & 15, pos.y, pos.z & 15));
                }
            }
            return EnumBlock.Air;
        }

        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью для блока
        /// </summary>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public MovingObjectPosition RayCastBlock(vec3 a, vec3 dir, float maxDist)
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

                    return new MovingObjectPosition(block, iend, norm, end);
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

        /// <summary>
        /// Сменить блок
        /// </summary>
        /// <param name="blockPos">позици блока</param>
        /// <param name="eBlock">тип блока</param>
        /// <returns>true смена была</returns>
        public virtual bool SetBlockState(BlockPos blockPos, EnumBlock eBlock)
        {
            if (blockPos.Y >= 0 && blockPos.Y < 256)
            {
                ChunkBase chunk = ChunkPr.GetChunk(blockPos.GetPositionChunk());
                if (chunk != null)
                {
                    chunk.SetEBlock(blockPos.GetPosition0(), eBlock);
                    MarkBlockForUpdate(blockPos);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Отметить блок для обновления
        /// </summary>
        protected virtual void MarkBlockForUpdate(BlockPos blockPos) { }

        /// <summary>
        /// Возвращает все объекты указанного типа класса, которые пересекаются с AABB кроме переданного в него
        /// </summary>
        public MapListEntity GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB type, AxisAlignedBB aabb, int entityId)
        {
            MapListEntity list = new MapListEntity();
            int minX = Mth.Floor((aabb.Min.x - 2f) / 16f);
            int maxX = Mth.Floor((aabb.Max.x + 2f) / 16f);
            int minZ = Mth.Floor((aabb.Min.z - 2f) / 16f);
            int maxZ = Mth.Floor((aabb.Max.z + 2f) / 16f);

            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    ChunkBase chunk = GetChunk(new vec2i(x, z));
                    if (chunk != null)
                    {
                        chunk.GetEntitiesAABB(entityId, aabb, list, type);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// Получить список блоков которые входят в рамку и пересекаются по коллизии
        /// </summary>
        public BlockBase[] GetBlocksAABB(AxisAlignedBB aabb)
        {
            List<BlockBase> blocks = new List<BlockBase>();

            int minX = Mth.Floor(aabb.Min.x);
            int maxX = Mth.Floor(aabb.Max.x + 1f);
            int minY = Mth.Floor(aabb.Min.y);
            int maxY = Mth.Floor(aabb.Max.y + 1f);
            int minZ = Mth.Floor(aabb.Min.z);
            int maxZ = Mth.Floor(aabb.Max.z + 1f);

            for (int x = minX; x < maxX; x++)
            {
                for (int z = minZ; z < maxZ; z++)
                {
                    if (ChunkPr.IsChunk(new vec2i(x >> 4, z >> 4)))
                    {
                        for (int y = minY - 1; y < maxY; y++)
                        {
                            BlockBase block = GetBlock(new BlockPos(x, y, z));
                            AxisAlignedBB mask = block.GetCollision();
                            if (mask != null && mask.IntersectsWith(aabb))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }
            }

            return blocks.ToArray();
        }

        /// <summary>
        /// Возвращает среднюю длину краев ограничивающей рамки блока больше или равна 1
        /// </summary>
        public bool GetAverageEdgeLengthBlock(BlockPos blockPos)
        {
            BlockBase block = GetBlock(blockPos);
            if (block == null) return false;
            AxisAlignedBB axis = block.GetCollision();
            return axis != null && axis.GetAverageEdgeLength() >= 1f;
        }

        /// <summary>
        /// Отправить процесс разрущения блока
        /// </summary>
        /// <param name="breakerId">id сущности который ломает блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="progress">сколько тактом блок должен разрушаться</param>
        public virtual void SendBlockBreakProgress(int breakerId, BlockPos pos, int progress) { }
    }
}
