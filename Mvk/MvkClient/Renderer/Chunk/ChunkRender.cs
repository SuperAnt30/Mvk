using MvkClient.Renderer.Block;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections.Generic;

namespace MvkClient.Renderer.Chunk
{
    /// <summary>
    /// Объект рендера чанка
    /// </summary>
    public class ChunkRender : ChunkBase
    {
        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        public ChunkMesh[] MeshDense { get; protected set; } = new ChunkMesh[16];
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        public ChunkMesh MeshAlpha { get; protected set; } = new ChunkMesh();
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsModifiedToRender { get; protected set; } = false;
        /// <summary>
        /// Буфер сплошных блоков
        /// </summary>
        protected float[] bufferDense = new float[0];

        public ChunkRender(WorldClient worldIn, vec2i pos) :base (worldIn, pos)
        {
            World = worldIn;
            //Position = pos;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
            //    StorageArrays[y] = new ChunkStorage(y);

                MeshDense[y] = new ChunkMesh();
            }
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        public void ModifiedToRender() => IsModifiedToRender = true;

        /// <summary>
        /// Количество полигонов
        /// </summary>
        public int CountPoligon => 0;// MeshAlpha.CountPoligon + MeshDense.CountPoligon;

        /// <summary>
        /// Удалить сетки
        /// </summary>
        public void MeshDelete()
        {
            MeshAlpha.Delete();
            for (int y = 0; y < MeshDense.Length; y++)
            {
                MeshDense[y].Delete();
            }
        }

        /// <summary>
        /// Рендер чанка
        /// </summary>
        public void Render()
        {
            IsModifiedToRender = false;
            //TODO:: Надо вытягивать наивысший блок, чтоб не тратить ресурс на рендер воздуха
            int yMax = 15; 
            for (int i = 0; i <= yMax; i++)
            {
                if (MeshDense[i].IsModifiedRender)
                {
                    int y0 = i * 16;
                    // буфер блоков
                    List<float> bufferCache = new List<float>();
                    for (int y = y0; y < y0 + 16; y++)
                    {
                        for (int z = 0; z < 16; z++)
                        {
                            for (int x = 0; x < 16; x++)
                            {
                                if (StorageArrays[i].GetEBlock(x, y & 15, z) == EnumBlock.Air) continue;
                                BlockBase block = GetBlock0(new vec3i(x, y, z));
                                if (block == null) continue;

                                BlockRender blockRender = new BlockRender(this, block);
                                //float[] buffer = blockRender.RenderMesh();
                                //bufferCache.AddRange(buffer);
                                bufferCache.AddRange(blockRender.RenderMesh());
                                //if (block.IsAlphe)
                                //{
                                //    if (buffer.Length > 0)
                                //    {
                                //        Chunk.StorageArrays[i].Buffer.Alphas.Add(new VoxelData()
                                //        {
                                //            Block = block,
                                //            Buffer = buffer,
                                //            Distance = camera.DistanceTo(
                                //                new vec3(Chunk.X << 4 | x, y, Chunk.Z << 4 | z)
                                //                )
                                //        });
                                //    }
                                //}
                                //else
                                //{
                                //    bufferCache.AddRange(buffer);
                                //}
                            }
                        }
                    }
                    MeshDense[i].SetBuffer(bufferCache.ToArray());
                    bufferCache.Clear();
                }
            }
        }

        public bool Draw(bool isBufferToRender)
        {
            bool b = false;
            for (int i = 15; i >= 0; i--)
            {
                if (isBufferToRender && BindBuffer(i)) b = true;
                // прорисовка
                MeshDense[i].Draw();
            }
            return b;
        }

        /// <summary>
        /// Занести буфер в рендер если это требуется
        /// </summary>
        protected bool BindBuffer(int y) => MeshDense[y].BindBuffer();
    }
}
