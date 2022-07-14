using MvkServer.Entity;
using MvkServer.Entity.Item;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk.Light;
//using MvkServer.World.Light;
using System;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase
    {
        /// <summary>
        /// Количество псевдо чанков
        /// </summary>
        public const int COUNT_HEIGHT = 16;
        /// <summary>
        /// Количество псевдо чанков COUNT_HEIGHT - 1
        /// </summary>
        public const int COUNT_HEIGHT15 = 15;
        /// <summary>
        /// Количество блоков в высоту
        /// </summary>
        public const int COUNT_HEIGHT_BLOCK = 255;
        /// <summary>
        /// Данные чанка
        /// </summary>
        public ChunkStorage[] StorageArrays { get; protected set; } = new ChunkStorage[COUNT_HEIGHT];
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldBase World { get; protected set; }
        /// <summary>
        /// Позиция чанка
        /// </summary>
        public vec2i Position { get; protected set; }
        /// <summary>
        /// Загружен ли чанк
        /// </summary>
        public bool IsChunkLoaded { get; protected set; } = false;
        /// <summary>
        /// Список сущностей в каждом псевдочанке
        /// </summary>
        public MapListEntity[] ListEntities { get; protected set; } = new MapListEntity[COUNT_HEIGHT];
        /// <summary>
        /// Объект работы с освещением
        /// </summary>
        public ChunkLight Light { get; private set; }

        /// <summary>
        /// Столбцы биомов
        /// </summary>
        //protected EnumBiome[,] eBiomes = new EnumBiome[16, 16];

        /// <summary>
        /// Статус готовности чанка 0-4
        /// 0 - генерация
        /// 1 - объект на этом чанке
        /// 2 - объект на соседнем чанке (карта высот)
        /// 3 - боковое освещение
        /// 4 - готов может и не надо, хватит 3 
        /// </summary>    
        //public int DoneStatus { get; set; } = 0;
        /// <summary>
        /// Совокупное количество тиков, которые игроки провели в этом чанке 
        /// </summary>
        public uint InhabitedTime { get; private set; }

        /// <summary>
        /// Последнее обновление чанка в тактах
        /// </summary>
        protected long updateTime;


        /// <summary>
        /// В тиках проверям, рендер света
        /// </summary>
        //private bool isGapLightingUpdated = false;

        /// <summary>
        /// Установите значение true, если чанк был изменен и нуждается в внутреннем обновлении. Для сохранения
        /// </summary>
        private bool isModified;
        /// <summary>
        /// Имеет ли этот фрагмент какие-либо сущности и, следовательно, требует сохранения на каждом тике
        /// </summary>
        private bool hasEntities = false;
        /// <summary>
        /// Время последнего сохранения этого фрагмента согласно World.worldTime
        /// </summary>
        private long lastSaveTime;

        protected ChunkBase() { }
        public ChunkBase(WorldBase worldIn, vec2i pos)
        {
            World = worldIn;
            Position = pos;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                StorageArrays[y] = new ChunkStorage(y << 4);
                ListEntities[y] = new MapListEntity();
            }
            Light = new ChunkLight(this);
        }

        /// <summary>
        /// Загружен чанк или сгенерирован
        /// </summary>
        //public void ChunkLoadGen()
        //{
        //    StorageArraysClear();

        //    //Random random = new Random();

        //    //if (random.Next(20) == 1)
        //    //{
        //    //    IsChunkLoaded = true;
        //    //    return;
        //    //}

        //    for (int y0 = 0; y0 < 24; y0++)
        //    {
        //        int sy = y0 >> 4;
        //        int y = y0 & 15;
        //        if (y0 > 8 && y0 < 16) continue;
        //        for (int x = 0; x < 16; x++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                StorageArrays[sy].SetEBlock(x, y, z, (y0 == 23) ? EnumBlock.Turf : EnumBlock.Stone);
        //            }
        //        }
        //    }
        //    for (int i = 5; i <= 10; i++)
        //    {
        //        StorageArrays[1].SetEBlock(12, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(13, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(14, 7, i, EnumBlock.Air);
        //        StorageArrays[1].SetEBlock(15, 7, i, EnumBlock.Air);
        //    }
        //    StorageArrays[1].SetEBlock(14, 7, 11, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(15, 7, 11, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(14, 7, 12, EnumBlock.Air);
        //    StorageArrays[1].SetEBlock(15, 7, 12, EnumBlock.Air);

        //    StorageArrays[1].SetEBlock(10, 8, 6, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(10, 8, 5, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(11, 8, 6, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(11, 8, 5, EnumBlock.Turf);

        //    StorageArrays[1].SetEBlock(14, 8, 12, EnumBlock.Cobblestone);

        //    //   StorageArrays[1].SetEBlock(8, 8, 0, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(6, 8, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(5, 8, 0, EnumBlock.Stone);
        //    //StorageArrays[1].SetEBlock(6, 9, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(5, 9, 0, EnumBlock.Cobblestone);

        //    StorageArrays[1].SetEBlock(3, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(3, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(2, 10, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(1, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(1, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(1, 10, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 8, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 9, 0, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(0, 10, 0, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(0, 11, 0, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(0, 12, 0, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(0, 12, 14, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(0, 12, 15, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 12, 14, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 12, 15, EnumBlock.Turf);
        //    StorageArrays[1].SetEBlock(1, 8, 15, EnumBlock.Cobblestone);
        //    StorageArrays[1].SetEBlock(0, 11, 1, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(0, 11, 2, EnumBlock.Dirt);
        //    StorageArrays[1].SetEBlock(1, 7, 0, EnumBlock.Dirt);

        //    int xv = 8;
        //    int zv = 1;

        //    StorageArrays[1].SetEBlock(xv, 8, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 9, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 10, zv, EnumBlock.Stone);
        //    StorageArrays[1].SetEBlock(xv, 11, zv, EnumBlock.Stone);

        //    //IsChunkLoaded = true;// LoadinData();
        //    // Продумать, для клиента запрос для сервера данных чанка, 
        //    // для сервера чанк пытается загрузиться, если он не создан то создаём

        //    System.Threading.Thread.Sleep(2);
        //}

        /// <summary>
        /// Выгружаем чанк
        /// </summary>
        public void OnChunkUnload()
        {
            IsChunkLoaded = false;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                World.UnloadEntities(ListEntities[y]);
            }
            // Продумать, для клиента просто удалить, для сервера записать и удалить
            //Save();
            //StorageArraysClear();
            //for (int y = 0; y < StorageArrays.Length; y++)
            //{
            //    StorageArrays[y].Delete();
            //}
        }

        public void OnChunkLoad()
        {
            //ChunkLoadGen();
            IsChunkLoaded = true;

            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                // Продумать загрузку чанка у сущности тип 
                // ListEntities[y].GetAt(0).OnChunkLoad();

                World.LoadEntities(ListEntities[y]);
            }
        }

        /// <summary>
        /// Очистить данные чанков
        /// </summary>
        protected void StorageArraysClear()
        {
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                StorageArrays[y].Clear();
                ListEntities[y].Clear();
            }
        }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        public void SetBinary(byte[] buffer, bool biom, int flagsYAreas)
        {
            int i = 0;
            for (int sy = 0; sy < COUNT_HEIGHT; sy++)
            {
                if ((flagsYAreas & 1 << sy) != 0)
                {
                    //StorageArrays[sy].CheckDataEmpty();
                    //StorageArrays[sy].countVoxel = 0;

                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            for (int z = 0; z < 16; z++)
                            {
                                StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
                                StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);


                                //StorageArrays[sy].data[y, x, z] = (ushort)(buffer[i++] | buffer[i++] << 8);
                                //if (StorageArrays[sy].data[y, x, z] != 0) StorageArrays[sy].Plus();
                                //StorageArrays[sy].light[y, x, z] = buffer[i++];


                            }
                        }
                    }
                    //StorageArrays[sy].LightCheckBlock();
                    ModifiedToRender(sy);
                }
            }
            // биом
            if (biom)
            {
                
            }
            IsChunkLoaded = true;
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка для клиента
        /// </summary>
        public virtual void ModifiedToRender(int y) { }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        //public void SetBinary(byte[] buffer, int height)
        //{
        //    int i = 0;
        //    for (int sy = 0; sy < height; sy++)
        //    {
        //        StorageArrays[sy].Clear();
        //        for (int y = 0; y < 16; y++)
        //        {
        //            for (int x = 0; x < 16; x++)
        //            {
        //                for (int z = 0; z < 16; z++)
        //                {
        //                    StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
        //                    StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);
        //                }
        //            }
        //        }
        //    }
        //    IsChunkLoaded = true;
        //}

        /// <summary>
        /// Задать псевдо чанк байтами
        /// </summary>
        //public void SetBinaryY(byte[] buffer, int sy)
        //{
        //    int i = 0;
        //    //StorageArrays[sy].Clear();
        //    for (int y = 0; y < 16; y++)
        //    {
        //        for (int x = 0; x < 16; x++)
        //        {
        //            for (int z = 0; z < 16; z++)
        //            {
        //                StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
        //                StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Обновить время использования чанка
        /// </summary>
        public void UpdateTime() => updateTime = DateTime.Now.Ticks;

        /// <summary>
        /// Старый ли чанк (больше 10 сек)
        /// </summary>
        public bool IsOldTime() => DateTime.Now.Ticks - updateTime > 100000000;

        /// <summary>
        /// Возвращает самый верхний экземпляр BlockStorage для этого фрагмента, который фактически содержит блок.
        /// </summary>
        public int GetTopFilledSegment()
        {
            for (int y = StorageArrays.Length - 1; y >= 0; y--)
            {
                if (!StorageArrays[y].IsEmptyData())
                {
                    return StorageArrays[y].GetYLocation();
                }
            }
            return 0;
        }

        /// <summary>
        /// Возвращает строку всех сегментов
        /// </summary>
        public string GetDebugAllSegment()
        {
            string s = "";
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                s += StorageArrays[y].IsEmptyData() ? "." : "*";
                s += StorageArrays[y].IsSky() ? "S" : "s";
            }
            return s;
        }

        #region Block

        ///// <summary>
        ///// Получить блок по координатам чанка XZ 0..15, Y 0..255
        ///// </summary>
        //public BlockBase GetBlock0(int x, int y, int z)
        //{
        //    BlockState blockState = GetBlockState(x, y, z);
        //    return blockState.GetBlock();
        //}

        /// <summary>
        /// Получить блок данных, XZ 0..15, Y 0..255
        /// </summary>
        public BlockState GetBlockState(int x, int y, int z)
        {
            if (x >> 4 == 0 && z >> 4 == 0)
            {
                int chy = y >> 4;
                ChunkStorage chunkStorage = StorageArrays[chy];
                if (!chunkStorage.IsEmptyData())
                {
                    return chunkStorage.GetBlockState(x, y & 15, z);
                }
            }
            return new BlockState().Empty();
        }

        /// <summary>
        /// Получить тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        //public EnumBlock GetEBlock(vec3i pos) => GetEBlock(pos.x, pos.y, pos.z);
        ///// <summary>
        ///// Получить тип блок по координатам чанка XZ 0..15, Y 0..255
        ///// </summary>
        //public EnumBlock GetEBlock(int x, int y, int z)
        //{
        //    int ys = y >> 4;
        //    if (x >> 4 == 0 && z >> 4 == 0 && !StorageArrays[ys].IsEmpty()) return StorageArrays[ys].GetEBlock(x, y & 15, z);
        //    return EnumBlock.Air;
        //}

        /// <summary>
        /// Задать тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        public void SetEBlock(vec3i pos, EnumBlock eBlock) => SetEBlock(pos, eBlock, 0);

        public void SetEBlock(vec3i pos, EnumBlock eBlock, int met)
        {
            int y = pos.y >> 4;
            if (pos.x >> 4 == 0 && pos.z >> 4 == 0 && pos.y >= 0 && y < COUNT_HEIGHT)
            {
                StorageArrays[y].SetData(pos.x, pos.y & 15, pos.z, (ushort)((ushort)eBlock & 0xFFF | met << 12));
            }
        }

        public void SetBlockStateClient(BlockPos blockPos, BlockState blockState)
        {
            int bx = blockPos.X & 15;
            int by = blockPos.Y & 15;
            int bz = blockPos.Z & 15;
            int chy = blockPos.Y >> 4;
            //if (StorageArrays[chy].IsEmptyData())// && StorageArrays[chy].IsEmptyLight())
            //{
            //    // Если вверхняя часть впервые создаётся, заполняем небесный свет все блоки
            //    StorageArrays[chy].LightSky();
            //}
            StorageArrays[chy].SetData(bx, by, bz, blockState.data);
            StorageArrays[chy].SetLightsFor(bx, by, bz, blockState.light);
        }

        /// <summary>
        /// Задать новые данные блока, с перерасчётом освещения если надо и прочего, возвращает прошлые данные блока
        /// </summary>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="blockState">данные нового блока</param>
        public BlockState SetBlockState(BlockPos blockPos, BlockState blockState, bool isCheckLight)
        {
            int bx = blockPos.X & 15;
            int by = blockPos.Y;
            int bz = blockPos.Z & 15;

           // Light.CheckPrecipitationHeightMap(blockPos);

            BlockState blockStateOld = GetBlockState(bx, by, bz);

            if (blockState.Equals(blockStateOld)) return new BlockState().Empty();

            //int height = Light.GetHeight(bx, bz);

            BlockBase block = blockState.GetBlock();
            BlockBase blockOld = blockStateOld.GetBlock();
            int chy = blockPos.Y >> 4;
            // bool heightMapUp = false;

            if (StorageArrays[chy].IsEmptyData())
            {
                if (block.EBlock == EnumBlock.Air) return new BlockState().Empty();
                //heightMapUp = by >= height;

                // Если вверхняя часть впервые создаётся, заполняем небесный свет все блоки
                //StorageArrays[chy].LightSky();
            }

            StorageArrays[chy].SetData(bx, by & 15, bz, blockState.data);
            //StorageArrays[chy].Set(bx, by & 15, bz, blockState);

            if (isCheckLight && blockOld != block)
            {
                // проверка света
                //if (World.IsRemote)
                Light.CheckLightSetBlock(blockPos, block.LightOpacity, blockOld.LightOpacity, block.LightValue != blockOld.LightValue);
            }

            if (StorageArrays[chy].IsEmptyData() || StorageArrays[chy].GetData(bx, by & 15, bz) != (ushort)block.EBlock) return new BlockState();

            //if (heightMapUp)
            //{
            //    Light.GenerateSkylightMap();
            //}
            //else
            //{
            //    Light.CheckBlockState(blockPos, height, block.LightOpacity, blockOld.LightOpacity);
            //}
            //Light.CheckLightSetBlock(blockNew.Position, blockNew.LightOpacity, blockOld.LightOpacity,
            //    blockNew.LightValue != blockOld.LightValue);

            //TileEntity var15;

            //if (blockOld instanceof ITileEntityProvider)
            //{
            //    var15 = this.func_177424_a(blockPos, Chunk.EnumCreateEntityType.CHECK);

            //    if (var15 != null)
            //    {
            //        var15.updateContainingBlockInfo();
            //    }
            //}

            //if (!this.worldObj.isRemote && blockOld != block)
            //{
            //    block.onBlockAdded(this.worldObj, blockPos, blockState);
            //}

            //if (block instanceof ITileEntityProvider)
            //{
            //    var15 = this.func_177424_a(blockPos, Chunk.EnumCreateEntityType.CHECK);

            //    if (var15 == null)
            //    {
            //        var15 = ((ITileEntityProvider)block).createNewTileEntity(this.worldObj, block.getMetaFromState(blockState));
            //        this.worldObj.setTileEntity(blockPos, var15);
            //    }

            //    if (var15 != null)
            //    {
            //        var15.updateContainingBlockInfo();
            //    }
            //}

            Modified();
            return blockStateOld;
        }

        #endregion

        #region Entity

        /// <summary>
        /// Добавить сущность в чанк
        /// </summary>
        public void AddEntity(EntityBase entity)
        {
            hasEntities = true;
            int x = Mth.Floor(entity.Position.x) >> 4;
            int z = Mth.Floor(entity.Position.z) >> 4;

            if (x != Position.x || z != Position.y)
            {
                World.Log.Log("ChunkBase: Неверное местоположение! ({0}, {1}) должно быть ({2}), {3}", x, z, Position, entity.Type);
                entity.SetDead();
                return;
            }

            int y = Mth.Floor(entity.Position.y) >> 4;
            if (y < 0) y = 0;
            if (y >= COUNT_HEIGHT) y = COUNT_HEIGHT - 1;

            entity.SetPositionChunk(Position.x, y, Position.y);
            ListEntities[y].Add(entity);
        }

        /// <summary>
        /// Удаляет сущность из конкретного псевдочанка
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="y">уровень псевдочанка</param>
        public void RemoveEntityAtIndex(EntityBase entity, int y)
        {
            if (y < 0) y = 0;
            if (y >= COUNT_HEIGHT) y = COUNT_HEIGHT - 1;
            ListEntities[y].Remove(entity);
        }

        /// <summary>
        ///  Удаляет сущность, используя его координату y в качестве индекса
        /// </summary>
        /// <param name="entity">сущность</param>
        public void RemoveEntity(EntityBase entity) => RemoveEntityAtIndex(entity, entity.PositionChunkY);

        /// <summary>
        /// Получить список id всех сущностей в чанке
        /// </summary>
        public EntityBase[] GetEntities()
        {
            List<EntityBase> list = new List<EntityBase>();
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                for (int i = 0; i < ListEntities[y].Count; i++)
                {
                    EntityBase entity = ListEntities[y].GetAt(i);
                    if (entity != null && entity.AddedToChunk)
                    {
                        list.Add(entity);
                    }
                }
                //list.AddRange(ListEntities[y].GetList());
            }
            return list.ToArray();
        }

        /// <summary>
        /// Получить список всех сущностей попадающих в рамку кроме входящей сущности
        /// </summary>
        /// <param name="entityId">входящяя сущность</param>
        /// <param name="aabb">рамка</param>
        /// <param name="list">список</param>
        public void GetEntitiesAABB(int entityId, AxisAlignedBB aabb, MapListEntity list, EnumEntityClassAABB type)
        {
            int minY = Mth.Floor((aabb.Min.y - 2f) / 16f);
            int maxY = Mth.Floor((aabb.Max.y + 2f) / 16f);
            minY = Mth.Clamp(minY, 0, ListEntities.Length - 1);
            maxY = Mth.Clamp(maxY, 0, ListEntities.Length - 1);

            for (int y = minY; y <= maxY; y++)
            {
                for (int i = 0; i < ListEntities[y].Count; i++)
                {
                    EntityBase entity = ListEntities[y].GetAt(i);
                    if (entity != null && entity.Id != entityId
                        && (type == EnumEntityClassAABB.All
                            || (type == EnumEntityClassAABB.EntityItem && entity is EntityItem)
                            || (type == EnumEntityClassAABB.EntityLiving && entity is EntityLiving)
                            )
                        && entity.BoundingBox.IntersectsWith(aabb))
                    {
                        list.Add(entity);
                    }
                }
            }
        }

        public enum EnumEntityClassAABB
        {
            /// <summary>
            /// Все
            /// </summary>
            All,
            /// <summary>
            /// Наследники сущности EntityItem
            /// </summary>
            EntityItem,
            /// <summary>
            /// Наследники мобов и игроков
            /// </summary>
            EntityLiving
        }


        /// <summary>
        /// Получить количество сущностей в чанке
        /// </summary>
        public int CountEntity()
        {
            int count = 0;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                count += ListEntities[y].Count;
            }
            return count;
        }

        #endregion

        #region Modify

        /// <summary>
        /// Пометка что чанк надо будет перезаписать
        /// </summary>
        public void Modified() => isModified = true;
        /// <summary>
        /// Пыло сохранение чанка, пометка убирается
        /// </summary>
        public void SavedNotModified() => isModified = false;
        /// <summary>
        /// Внести время сохранения чанка
        /// </summary>
        public void SetLastSaveTime(uint time) => lastSaveTime = time;

        /// <summary>
        /// Возвращает true, если этот чанке необходимо сохранить
        /// </summary>
        public bool NeedsSaving()
        {
            if (hasEntities && World.GetTotalWorldTime() != lastSaveTime || isModified)
            {
                return true;
            }

            return isModified;
        }

        /// <summary>
        /// Заменить флаг на наличие сущностей в чанке.
        /// Этот метод вызывается в момент сохранения чанка
        /// </summary>
        public void SetHasEntities(bool hasEntitiesIn) => hasEntities = hasEntitiesIn;

        #endregion


        /// <summary>
        /// Обновление в такте активных чанков
        /// </summary>
        public void Update()
        {

        }

        /// <summary>
        /// Задать совокупное количество тиков, которые игроки провели в этом чанке 
        /// </summary>
        public void SetInhabitedTime(uint time) => InhabitedTime = time;



        
    }
}
