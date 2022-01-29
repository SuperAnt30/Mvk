using MvkClient.Renderer.Chunk;
using MvkServer.Glm;

namespace MvkClient.Util
{
    /// <summary>
    /// Структура хранении чанков в FrustumCulling
    /// </summary>
    public struct FrustumStruct
    {
        private readonly ChunkRender chunk;
        private vec2i coord;
        private readonly bool isChunk;

        public FrustumStruct(ChunkRender chunk)
        {
            this.chunk = chunk;
            coord = chunk.Position;
            isChunk = true;
        }
        public FrustumStruct(vec2i coord)
        {
            this.coord = coord;
            chunk = null;
            isChunk = false;
        }

        public bool IsChunk() => isChunk;
        public vec2i GetCoord() => coord;
        public ChunkRender GetChunk() => isChunk ? chunk : null;
    }
}
