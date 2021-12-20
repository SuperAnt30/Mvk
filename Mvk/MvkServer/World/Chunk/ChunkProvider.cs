using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.World.Chunk
{
    /// <summary>
    /// Объект который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProvider
    {

        /**
         * В этот объект разделён на Server и Client
         * для загрузки, выгрузки и получения чанков
         * по сути тут хранится весь кэш чанков
         */


        // Chunk.func_177439_a = Файл принимает пакеты с сервера байт массив, пакет псевдочанка 16*16*16


            
        /// <summary>
        /// Чанк по умолчанию, если нет ни одного в списке
        /// </summary>
        public ChunkBase ChunkDefault { get; protected set; }

        /// <summary>
        /// Список чанков
        /// </summary>
        protected ChunkMap chunkMapping = new ChunkMap();

        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        protected WorldBase world;


        public ChunkProvider(WorldBase worldIn)
        {
            ChunkDefault = new ChunkBase(worldIn, new vec2i(0));
            world = worldIn;
        }

        /// <summary>
        /// Для дебага
        /// </summary>
        public List<vec2i> GetList() => chunkMapping.GetList();
        // TODO::отладка чанков

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(vec2i pos)
        {
            ChunkBase chunk = ProvideChunk(pos);
            if (chunk.IsChunkLoaded)
            {
                chunk.ChunkUnload();
            }
            chunkMapping.Remove(pos);
        }

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        public void RemoveChunk(vec2i pos)
        {
            chunkMapping.Remove(pos);
        }

        /// <summary>
        /// Загрузить чанк
        /// </summary>
        public ChunkBase LoadGenChunk(vec2i pos)
        {
            ChunkBase chunk = new ChunkBase(world, pos);
            chunkMapping.Set(chunk);
            chunk.ChunkLoad();
            return chunk;
        }

        /// <summary>
        /// Загрузить, если нет такого создаём
        /// </summary>
        public ChunkBase LoadNewChunk(vec2i pos)
        {
            if (chunkMapping.Contains(pos))
            {
                return chunkMapping.Get(pos);
            }
            ChunkBase chunk = new ChunkBase(world, pos);
            chunkMapping.Set(chunk);
            return chunk;
        }

        /// <summary>
        /// Проверить наличие чанка в массиве
        /// </summary>
        public bool IsChunk(vec2i pos)
        {
            if (chunkMapping.Contains(pos))
            {
                ChunkBase chunk = ProvideChunk(pos);
                return chunk.IsChunkLoaded;
            }
            return false;
        }

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase ProvideChunk(vec2i pos)
        {
            ChunkBase chunk = chunkMapping.Get(pos);
            return chunk ?? ChunkDefault;
        }

        /// <summary>
        /// Количество чанков в кэше
        /// </summary>
        public int Count => chunkMapping.Count;

    }
}
