using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using System;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Базовый объект чанка
    /// </summary>
    public class ChunkBase
    {
        /// <summary>
        /// Данные чанка
        /// </summary>
        public ChunkStorage[] StorageArrays { get; protected set; } = new ChunkStorage[16];
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
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                StorageArrays[y] = new ChunkStorage(y);
            }
        }

        /// <summary>
        /// Загружен чанк или сгенерирован
        /// </summary>
        public void ChunkLoadGen()
        {
            StorageArraysClear();
            //TODO:: Тест генерации

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

            IsChunkLoaded = true;// LoadinData();
            // Продумать, для клиента запрос для сервера данных чанка, 
            // для сервера чанк пытается загрузиться, если он не создан то создаём
        }

        /// <summary>
        /// Выгружаем чанк
        /// </summary>
        public void ChunkUnload()
        {
            IsChunkLoaded = false;
            // Продумать, для клиента просто удалить, для сервера записать и удалить
            //Save();
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                StorageArrays[y].Delete();
            }
        }
        /// <summary>
        /// Очистить данные чанков
        /// </summary>
        protected void StorageArraysClear()
        {
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                StorageArrays[y].Clear();
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
    }
}
