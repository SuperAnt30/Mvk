using MvkServer.Glm;
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
            for (int y = 0; y < StorageArrays.Length; y++) StorageArrays[y] = new ChunkStorage(y);
        }

        /// <summary>
        /// Загружен чанк или сгенерирован
        /// </summary>
        public void ChunkLoadGen()
        {
            //TODO:: Тест
            
            for (int y0 = 0; y0 < 24; y0++)
            {
                int sy = y0 >> 4;
                int y = y0 & 15;
                for (int x = 0; x < 16; x++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        StorageArrays[sy].SetEBlock(x, y, z, Block.EnumBlock.Stone);
                    }
                }
            }

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
        }

        /// <summary>
        /// Задать чанк байтами
        /// </summary>
        public void SetBinary(byte[] buffer, int height)
        {
            int i = 0;
            for (int sy = 0; sy < height; sy++)
            {
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
    }
}
