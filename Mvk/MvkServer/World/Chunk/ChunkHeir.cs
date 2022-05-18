namespace MvkServer.World.Chunk
{
    public class ChunkHeir
    {
        /// <summary>
        /// Объект базового мира
        /// </summary>
        public WorldBase World { get; protected set; }
        /// <summary>
        /// Объект кэш чанка
        /// </summary>
        public ChunkBase Chunk { get; protected set; }

        protected ChunkHeir() { }
        public ChunkHeir(ChunkBase chunk)
        {
            Chunk = chunk;
            World = chunk.World;
        }
    }
}
