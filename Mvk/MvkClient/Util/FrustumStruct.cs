using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.World.Chunk;

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
        /// <summary>
        /// Какие псевдо чанки видны после Frustum
        /// </summary>
        private readonly bool[] show;

        public FrustumStruct(ChunkRender chunk) : this(chunk, new bool[ChunkBase.COUNT_HEIGHT]) { }
        public FrustumStruct(ChunkRender chunk, bool[] show)
        {
            this.chunk = chunk;
            coord = chunk.Position;
            isChunk = true;
            this.show = show;
        }
        public FrustumStruct(vec2i coord)
        {
            this.coord = coord;
            chunk = null;
            isChunk = false;
            show = new bool[ChunkBase.COUNT_HEIGHT];
        }

        public bool[] GetShowList() => show;
        public bool IsChunk() => isChunk;
        public vec2i GetCoord() => coord;
        public ChunkRender GetChunk() => isChunk ? chunk : null;

        /// <summary>
        /// В зависимости от обзора, помечаем какие псевдо чанки видны
        /// </summary>
        public int FrustumShow(Frustum frustum, int x1, int z1, int x2, int z2, float offsetY)
        {
            int count = 0;
            for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
            {
                int yb = y << 4;
                int y1 = yb;
                int y2 = yb + 15;
                this.show[y] = frustum.IsBoxInFrustum(x1, y1 - offsetY, z1, x2, y2 - offsetY, z2);
                if (this.show[y]) count++;
            }
            return count;
        }

        public bool IsShow(int y) => show[y];
    }
}
