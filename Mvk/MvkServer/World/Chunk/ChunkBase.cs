using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
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
        public int DoneStatus { get; set; } = 0;

        /// <summary>
        /// Последнее обновление чанка в тактах
        /// </summary>
        protected long updateTime;

        protected ChunkBase() { }
        public ChunkBase(WorldBase worldIn, vec2i pos)
        {
            World = worldIn;
            Position = pos;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                StorageArrays[y] = new ChunkStorage(y);
                ListEntities[y] = new MapListEntity();
            }
        }

        /// <summary>
        /// Загружен чанк или сгенерирован
        /// </summary>
        public void ChunkLoadGen()
        {
            StorageArraysClear();
            //TODO:: Тест генерации

            //Random random = new Random();

            //if (random.Next(20) == 1)
            //{
            //    IsChunkLoaded = true;
            //    return;
            //}

            for (int y0 = 0; y0 < 24; y0++)
            {
                int sy = y0 >> 4;
                int y = y0 & 15;
                if (y0 > 8 && y0 < 16) continue;
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        StorageArrays[sy].SetEBlock(x, y, z, (y0 == 23) ? EnumBlock.Turf : EnumBlock.Stone);
                    }
                }
            }
            for (int i = 5; i <= 10; i++)
            {
                StorageArrays[1].SetEBlock(12, 7, i, EnumBlock.Air);
                StorageArrays[1].SetEBlock(13, 7, i, EnumBlock.Air);
                StorageArrays[1].SetEBlock(14, 7, i, EnumBlock.Air);
                StorageArrays[1].SetEBlock(15, 7, i, EnumBlock.Air);
            }
            StorageArrays[1].SetEBlock(14, 7, 11, EnumBlock.Air);
            StorageArrays[1].SetEBlock(15, 7, 11, EnumBlock.Air);
            StorageArrays[1].SetEBlock(14, 7, 12, EnumBlock.Air);
            StorageArrays[1].SetEBlock(15, 7, 12, EnumBlock.Air);

            StorageArrays[1].SetEBlock(10, 8, 6, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(10, 8, 5, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(11, 8, 6, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(11, 8, 5, EnumBlock.Turf);

            StorageArrays[1].SetEBlock(14, 8, 12, EnumBlock.Cobblestone);

            //   StorageArrays[1].SetEBlock(8, 8, 0, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(6, 8, 0, EnumBlock.Cobblestone);
            StorageArrays[1].SetEBlock(5, 8, 0, EnumBlock.Stone);
            //StorageArrays[1].SetEBlock(6, 9, 0, EnumBlock.Cobblestone);
            StorageArrays[1].SetEBlock(5, 9, 0, EnumBlock.Cobblestone);

            StorageArrays[1].SetEBlock(3, 8, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(3, 9, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(2, 8, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(2, 9, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(2, 10, 0, EnumBlock.Cobblestone);
            StorageArrays[1].SetEBlock(1, 8, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(1, 9, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(1, 10, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(0, 8, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(0, 9, 0, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(0, 10, 0, EnumBlock.Cobblestone);
            StorageArrays[1].SetEBlock(0, 11, 0, EnumBlock.Dirt);
            StorageArrays[1].SetEBlock(0, 12, 0, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(0, 12, 14, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(0, 12, 15, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(1, 12, 14, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(1, 12, 15, EnumBlock.Turf);
            StorageArrays[1].SetEBlock(1, 8, 15, EnumBlock.Cobblestone);
            StorageArrays[1].SetEBlock(0, 11, 1, EnumBlock.Dirt);
            StorageArrays[1].SetEBlock(0, 11, 2, EnumBlock.Dirt);
            StorageArrays[1].SetEBlock(1, 7, 0, EnumBlock.Dirt);

            int xv = 8;
            int zv = 1;

            StorageArrays[1].SetEBlock(xv, 8, zv, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(xv, 9, zv, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(xv, 10, zv, EnumBlock.Stone);
            StorageArrays[1].SetEBlock(xv, 11, zv, EnumBlock.Stone);

            //IsChunkLoaded = true;// LoadinData();
            // Продумать, для клиента запрос для сервера данных чанка, 
            // для сервера чанк пытается загрузиться, если он не создан то создаём
        }

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
        public void SetBinary(byte[] buffer, int height)
        {
            int i = 0;
            for (int sy = 0; sy < height; sy++)
            {
                StorageArrays[sy].Clear();
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            StorageArrays[sy].SetData(x, y, z, (ushort)(buffer[i++] | buffer[i++] << 8));
                            StorageArrays[sy].SetLightsFor(x, y, z, buffer[i++]);
                        }
                    }
                }
            }
            IsChunkLoaded = true;
        }

        /// <summary>
        /// Обновить время использования чанка
        /// </summary>
        public void UpdateTime() => updateTime = DateTime.Now.Ticks;

        /// <summary>
        /// Старый ли чанк (больше 10 сек)
        /// </summary>
        public bool IsOldTime() => DateTime.Now.Ticks - updateTime > 100000000;

        #region Block

        /// <summary>
        /// Получить блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        public BlockBase GetBlock0(vec3i pos)
        {
            EnumBlock eblock = GetEBlock(pos);
            return Blocks.GetBlock(eblock, new BlockPos(Position.x << 4 | pos.x, pos.y, Position.y << 4 | pos.z));
        }

        /// <summary>
        /// Получить тип блок по координатам чанка XZ 0..15, Y 0..255
        /// </summary>
        public EnumBlock GetEBlock(vec3i pos)
        {
            if (pos.x >> 4 == 0 && pos.z >> 4 == 0) return StorageArrays[pos.y >> 4].GetEBlock(pos.x, pos.y & 15, pos.z);
            return EnumBlock.Air;
        }

        #endregion

        #region Entity

        /// <summary>
        /// Добавить сущность в чанк
        /// </summary>
        public void AddEntity(EntityLiving entity)
        {
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
        public void RemoveEntityAtIndex(EntityLiving entity, int y)
        {
            if (y < 0) y = 0;
            if (y >= COUNT_HEIGHT) y = COUNT_HEIGHT - 1;
            ListEntities[y].Remove(entity);
        }

        /// <summary>
        ///  Удаляет сущность, используя его координату y в качестве индекса
        /// </summary>
        /// <param name="entity">сущность</param>
        public void RemoveEntity(EntityLiving entity) => RemoveEntityAtIndex(entity, entity.PositionChunkY);

        /// <summary>
        /// Получить список id всех сущностей в чанке
        /// </summary>
        public EntityLiving[] GetEntities()
        {
            List<EntityLiving> list = new List<EntityLiving>();
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                for (int i = 0; i < ListEntities[y].Count; i++)
                {
                    EntityLiving entity = (EntityLiving)ListEntities[y].GetAt(i);
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

        /// <summary>
        /// Обновление в такте
        /// </summary>
        public void Update()
        {

        }
    }
}
