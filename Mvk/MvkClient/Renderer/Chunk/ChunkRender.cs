using MvkClient.Renderer.Block;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
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
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }
        /// <summary>
        /// Сетка чанка сплошных блоков
        /// </summary>
        public ChunkMesh[] MeshDense { get; private set; } = new ChunkMesh[16];
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        public ChunkMesh MeshAlpha { get; private set; } = new ChunkMesh();
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsModifiedToRender { get; private set; } = false;
        
        /// <summary>
        /// Буфер сплошных блоков
        /// </summary>
        private readonly float[] bufferDense = new float[0];
        /// <summary>
        /// Массив блоков которые разрушаются
        /// </summary>
        private List<DestroyBlockProgress> destroyBlocks = new List<DestroyBlockProgress>(); 

        public ChunkRender(WorldClient worldIn, vec2i pos) :base (worldIn, pos)
        {
            ClientWorld = worldIn;
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
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        /// <param name="y"></param>
        public void ModifiedToRender(int y)
        {
            IsModifiedToRender = true;
            MeshDense[y].SetModifiedRender();
        }

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

                                BlockRender blockRender = new BlockRender(this, block)
                                {
                                    DamagedBlocksValue = GetDestroyBlocksValue(x, y, z)
                                };
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
        private bool BindBuffer(int y) => MeshDense[y].BindBuffer();

        /// <summary>
        /// Занести разрушение блока
        /// </summary>
        /// <param name="breakerId">Id сущности игрока</param>
        /// <param name="blockPos">позиция блока</param>
        /// <param name="progress">процесс разрушения</param>
        public void DestroyBlockSet(int breakerId, BlockPos blockPos, int progress)
        {
            DestroyBlockProgress destroy = null;
            for (int i = 0; i < destroyBlocks.Count; i++)
            {
                if (destroyBlocks[i].BreakerId == breakerId)
                {
                    destroy = destroyBlocks[i];
                    break;
                }
            }
            if (destroy == null)
            {
                destroy = new DestroyBlockProgress(breakerId, blockPos);
                destroyBlocks.Add(destroy);
            }
            destroy.SetPartialBlockDamage(progress);
            destroy.SetCloudUpdateTick(ClientWorld.ClientMain.TickCounter);
        }

        /// <summary>
        /// Удалить разрушение блока
        /// </summary>
        /// <param name="breakerId">Id сущности игрока</param>
        public void DestroyBlockRemove(int breakerId)
        {
            for (int i = destroyBlocks.Count - 1; i >= 0; i--)
            {
                if (destroyBlocks[i].BreakerId == breakerId)
                {
                    destroyBlocks.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Проверить есть ли на тикущем блоке разрушение
        /// </summary>
        /// <param name="x">локальная позиция X блока 0..15</param>
        /// <param name="y">локальная позиция Y блока</param>
        /// <param name="z">локальная позиция Z блока 0..15</param>
        /// <returns>-1 нет разрушения, 0-9 разрушение</returns>
        private int GetDestroyBlocksValue(int x, int y, int z)
        {
            if (destroyBlocks.Count > 0)
            {
                for (int i = 0; i < destroyBlocks.Count; i++)
                {
                    DestroyBlockProgress destroy = destroyBlocks[i];
                    if (destroy.Position.EqualsPosition0(x, y, z))
                    {
                        return destroy.PartialBlockProgress;
                    }
                }
            }
            return -1;
        }
    }
}
