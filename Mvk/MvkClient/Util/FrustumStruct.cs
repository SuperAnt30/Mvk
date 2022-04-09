using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkClient.Util
{
    /// <summary>
    /// Структура хранении чанков в FrustumCulling
    /// </summary>
    public struct FrustumStruct
    {
        private readonly ChunkRender chunk;
        private readonly vec2i coord;
        private readonly bool isChunk;
        private byte[] showSort;

        public FrustumStruct(ChunkRender chunk) : this(chunk, new byte[0]) { }
        public FrustumStruct(ChunkRender chunk, byte[] showSort)
        {
            this.chunk = chunk;
            coord = chunk.Position;
            isChunk = true;
            this.showSort = showSort;
        }
        public FrustumStruct(vec2i coord)
        {
            this.coord = coord;
            chunk = null;
            isChunk = false;
            showSort = new byte[0];
        }

        public byte[] GetSortList() => showSort;
        public bool IsChunk() => isChunk;
        public vec2i GetCoord() => coord;
        public ChunkRender GetChunk() => isChunk ? chunk : null;

        /// <summary>
        /// В зависимости от обзора, помечаем какие псевдо чанки видны
        /// </summary>
        public int FrustumShow(Frustum frustum, int x1, int z1, int x2, int z2, int offsetY)
        {
            int count = 0;
            bool[] show = new bool[ChunkBase.COUNT_HEIGHT];
            for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
            {
                int yb = (y << 4) - offsetY;
                int y1 = yb - 15;
                int y2 = yb + 24;
                show[y] = frustum.IsBoxInFrustum(x1, y1, z1, x2, y2, z2);
                if (show[y]) count++;
            }
            if (count > 0)
            {
                // массив псевдо чанков по возрастанию
                List<byte> vs = new List<byte>();
                int y0 = offsetY >> 4;
                int ymin = y0;
                int ymax = y0 + 1;
                while (ymin >= 0 || ymax < ChunkBase.COUNT_HEIGHT)
                {
                    if (ymin >= 0)
                    {
                        if (ymin < ChunkBase.COUNT_HEIGHT && show[ymin]) vs.Add((byte)ymin);
                        ymin--;
                    }
                    if (ymax < ChunkBase.COUNT_HEIGHT)
                    {
                        if (ymax >= 0 && show[ymax]) vs.Add((byte)ymax);
                        ymax++;
                    }
                }
                showSort = vs.ToArray();
            }
            else
            {
                showSort = new byte[0];
            }
            return count;
        }

        public override string ToString() => string.Format("{0} {1}", coord, isChunk ? "*" : "");
    }
}
