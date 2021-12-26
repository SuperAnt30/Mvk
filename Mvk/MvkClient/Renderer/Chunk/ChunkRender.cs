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
        public ChunkMesh MeshDense { get; protected set; } = new ChunkMesh();
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        public ChunkMesh MeshAlpha { get; protected set; } = new ChunkMesh();
        /// <summary>
        /// Данные буферов псевдо чанков
        /// </summary>
        public ChunkBuffer[] BufferArrays { get; protected set; } = new ChunkBuffer[16];
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsModifiedToRender { get; protected set; } = false;
        /// <summary>
        /// Буфер сплошных блоков
        /// </summary>
        protected float[] bufferDense = new float[0];

        public ChunkRender(WorldClient worldIn, vec2i pos) 
        {
            World = worldIn;
            Position = pos;
            for (int y = 0; y < StorageArrays.Length; y++)
            {
                StorageArrays[y] = new ChunkStorage(y);
                BufferArrays[y] = new ChunkBuffer(y);
            }
        }

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        public void ModifiedToRender() => IsModifiedToRender = true;

        /// <summary>
        /// Количество полигонов
        /// </summary>
        public int CountPoligon => MeshAlpha.CountPoligon + MeshDense.CountPoligon;

        /// <summary>
        /// Удалить сетки
        /// </summary>
        public void Delete()
        {
            MeshAlpha.Delete();
            MeshDense.Delete();
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
                if (BufferArrays[i].IsModifiedRender)
                {
                   // BufferArrays[i].Alphas.Clear();
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
                                float[] buffer = blockRender.RenderMesh();
                                bufferCache.AddRange(buffer);
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
                    BufferArrays[i].RenderDone(bufferCache.ToArray());
                    bufferCache.Clear();
                }
            }
            // Должен быть в основном потоке где графика!
            bufferDense = GlueBuffer();
            //MeshDense.Render();
        }

        /// <summary>
        /// Занести буфер в рендер если это требуется
        /// </summary>
        public void BufferToRender()
        {
            if (bufferDense.Length > 0)
            {
                MeshDense.Render(bufferDense);
                bufferDense = new float[0];
            }
        }

        /// <summary>
        /// Склейка сетки
        /// </summary>
        protected float[] GlueBuffer()
        {
            // TODO:: 2021.12.24 модернезировать после запуска, аналого как в пакетах!!!
            int count = BufferArrays.Length;
            int countAll = 0;
            for (int i = 0; i < count; i++)
            {
                countAll += BufferArrays[i].Buffer.Length;
            }

            float[] buffer = new float[countAll];
            countAll = 0;
            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < BufferArrays[i].Buffer.Length; j++)
                {
                    buffer[countAll] = BufferArrays[i].Buffer[j];
                    countAll++;
                }
            }
            return buffer;
        }

    }
}
