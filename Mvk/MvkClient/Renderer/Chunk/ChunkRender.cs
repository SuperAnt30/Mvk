using MvkClient.Renderer.Block;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

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
        private ChunkMesh[] meshDense = new ChunkMesh[COUNT_HEIGHT];
        /// <summary>
        /// Сетка чанка альфа блоков
        /// </summary>
        private readonly ChunkMesh[] meshAlpha = new ChunkMesh[COUNT_HEIGHT];
        /// <summary>
        /// Массив блоков которые разрушаются
        /// </summary>
        private List<DestroyBlockProgress> destroyBlocks = new List<DestroyBlockProgress>();
        /// <summary>
        /// Количество альфа блоков в псевдо чанке
        /// </summary>
        private readonly int[] countAlpha = new int[COUNT_HEIGHT];

        /// <summary>
        /// Соседние чанки, заполняются перед рендером
        /// </summary>
        private Hashtable chunks = new Hashtable(); 

        public ChunkRender(WorldClient worldIn, vec2i pos) :base (worldIn, pos)
        {
            ClientWorld = worldIn;
            for (int y = 0; y < COUNT_HEIGHT; y++)
            {
                meshDense[y] = new ChunkMesh();
                meshAlpha[y] = new ChunkMesh();
            }
        }

        /// <summary>
        /// Проверка псевдо чанка на количество альфа блоков, если равно 0 то true
        /// </summary>
        public bool CheckAlphaZero(int y) => countAlpha[y] == 0;

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка
        /// </summary>
        public override void ModifiedToRender(int y)
        {
            if (y >= 0 && y < COUNT_HEIGHT) meshDense[y].SetModifiedRender();
        }
        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка
        /// </summary>
        public bool IsModifiedRender(int y) => meshDense[y].IsModifiedRender;

        /// <summary>
        /// Пометить что надо перерендерить сетку чанка альфа блоков
        /// </summary>
        public void ModifiedToRenderAlpha(int y)
        {
            if (y >= 0 && y < COUNT_HEIGHT) meshAlpha[y].SetModifiedRender();
        }
        /// <summary>
        /// Проверка, нужен ли рендер этого псевдо чанка  альфа блоков
        /// </summary>
        public bool IsModifiedRenderAlpha(int y) => meshAlpha[y].IsModifiedRender;

        /// <summary>
        /// Удалить сетки
        /// </summary>
        public void MeshDelete()
        {
            for (int y = 0; y < meshDense.Length; y++)
            {
                meshDense[y].Delete();
                meshAlpha[y].Delete();
                countAlpha[y] = 0;
            }
        }

        /// <summary>
        /// Старт рендеринга
        /// </summary>
        public void StartRendering(int y)
        {
            meshDense[y].StatusRendering();
            meshAlpha[y].StatusRendering();
            chunks.Clear();
            foreach (vec2i pos in MvkStatic.AreaOne8)
            {
                vec2i pos2 = Position + pos;
                chunks.Add(pos2, World.ChunkPr.GetChunk(pos2));
            }
        }

        public ChunkBase Chunk(vec2i pos)
        {
            if (chunks.ContainsKey(pos)) return chunks[pos] as ChunkBase;
            return null;
        }

        Stopwatch stopwatch = new Stopwatch();
        
        /// <summary>
        /// Рендер псевдо чанка, сплошных и альфа блоков
        /// </summary>
        public void Render(int chY)
        {
            try
            {
                long timeBegin = GLWindow.stopwatch.ElapsedTicks;

                //chunks.Clear();
                //foreach(vec2i pos in MvkStatic.AreaOne8)
                //{
                //    vec2i pos2 = Position + pos;
                //    chunks.Add(pos2, World.ChunkPr.GetChunk(pos2));
                //}

                // буфер блоков
                List<byte> bufferCache = new List<byte>();
                // буфер альфа блоков
                List<BlockBuffer> alphas = new List<BlockBuffer>();
                vec3i posPlayer = ClientWorld.ClientMain.Player.PositionAlphaBlock;
                BlockState blockState = new BlockState().Empty();
                BlockPos blockPos = new BlockPos();
                BlockRender blockRender = new BlockRender(this);
                BlockBase block;
                byte[] buffer = new byte[0];
                for (int y = 0; y < 16; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            blockState.data = StorageArrays[chY].data[y, x, z];
                            //blockState.data = StorageArrays[chY].GetData(x, y, z);
                            int id = blockState.data & 0xFFF;// blockState.Id();
                            if (id == 0) continue;

                            int yBlock = chY << 4 | y;
                            blockPos.X = Position.x << 4 | x;
                            blockPos.Y = yBlock;
                            blockPos.Z = Position.y << 4 | z;

                            // TODO::2022-05-09
                            // тут 0,01
                            block = Blocks.BlocksInt[id];
                            blockRender.blockState = blockState;
                            blockRender.block = block;
                            blockRender.blockPos = blockPos;
                            blockRender.buffer = bufferCache;

                            if (block.IsAlpha) blockRender.bufferAlpha = new List<byte>();
                                //blockRender.Set(block, blockPos);
                                //blockRender.Set(blockState.GetBlock(), blockPos);
                                blockRender.DamagedBlocksValue = GetDestroyBlocksValue(x, yBlock, z);
                            // тут 0,2
                            blockRender.RenderMesh();
                            // тут 0,5 - 3,5
                            //blockRender.
                            //BlockPos blockPos = new BlockPos(Position.x << 4 | x, yBlock, Position.y << 4 | z);
                            //blockState.data = GetBlockState(blockPos);
                            //if (StorageArrays[chY].GetEBlock(x, y, z) == EnumBlock.Air) continue;

                            //BlockPos blockPos = new BlockPos(Position.x << 4 | x, yBlock, Position.y << 4 | z);
                            //BlockState blockState = GetBlockState(blockPos);
                            //if (blockState.IsEmpty()) continue;

                            //BlockBase block = GetBlock0(new vec3i(x, yBlock, z));
                            //if (block == null) continue;

                            //BlockRender blockRender = new BlockRender(this, blockState, blockPos)
                            //{
                            //    DamagedBlocksValue = GetDestroyBlocksValue(x, yBlock, z)
                            //};

                            //byte[] buffer = new byte[0];
                            //buffer = blockRender.RenderMesh(true);
                            //if (buffer.Length > 0)
                            //{
                            //    BlockBase block = blockState.GetBlock();
                                if (block.IsAlpha)
                                {
                                    alphas.Add(new BlockBuffer(block.EBlock, new vec3i(x, y, z), blockRender.bufferAlpha.ToArray(),
                                        glm.distance(posPlayer, new vec3i(Position.x << 4 | x, yBlock, Position.y << 4 | z))
                                    ));
                                }
                            //    else
                            //    {
                            //        bufferCache.AddRange(buffer);
                            //    }
                            //}
                        }
                    }
                }
                countAlpha[chY] = alphas.Count;
                meshAlpha[chY].SetBuffer(ToBufferAlphaY(alphas));
                meshDense[chY].SetBuffer(bufferCache.ToArray());
                bufferCache.Clear();
                long timeEnd = GLWindow.stopwatch.ElapsedTicks;
                float time = (timeEnd - timeBegin) / (float)MvkStatic.TimerFrequency;
                Debug.RenderChunckTime8 = (Debug.RenderChunckTime8 * 3f + time) / 4f;
                Debug.RenderChunckTime = time;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Вернуть массив буфера альфа
        /// </summary>
        public byte[] ToBufferAlphaY(List<BlockBuffer> alphas)
        { 
            int count = alphas.Count;
            if (count > 0)
            {
                alphas.Sort();
                List<byte> buffer = new List<byte>();
                for (int i = count - 1; i >= 0; i--)
                {
                    buffer.AddRange(alphas[i].Buffer());
                }
                return buffer.ToArray();
            }
            return new byte[0];
        }

        /// <summary>
        /// Прорисовка сплошных блоков псевдо чанка
        /// </summary>
        public void DrawDense(int y) => meshDense[y].Draw();
        /// <summary>
        /// Прорисовка альфа блоков псевдо чанка
        /// </summary>
        public void DrawAlpha(int y) => meshAlpha[y].Draw();

        /// <summary>
        /// Занести буфер сплошных блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferDense(int y) => meshDense[y].BindBuffer();
        //// <summary>
        /// Занести буфер альфа блоков псевдо чанка если это требуется
        /// </summary>
        public void BindBufferAlpha(int y) => meshAlpha[y].BindBuffer();

        public void BindDelete(int y)
        {
            meshDense[y].Delete();
            meshAlpha[y].Delete();
        }
        /// <summary>
        /// проверка есть ли альфа блоки во всём чанке
        /// </summary>
        //public bool IsAlpha()
        //{
        //    for (int i = 0; i < COUNT_HEIGHT; i++)
        //    {
        //        if (countAlpha[i] > 0) return true;
        //    }
        //    return false;
        //}

        #region DestroyBlock

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

        #endregion

        #region Status

        /// <summary>
        /// Статсус возможности для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseWait(int y) => meshDense[y].Status == ChunkMesh.StatusMesh.Wait || meshDense[y].Status == ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус возможности для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaWait(int y) => meshAlpha[y].Status == ChunkMesh.StatusMesh.Wait || meshAlpha[y].Status == ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера сплошных блоков
        /// </summary>
        public bool IsMeshDenseBinding(int y) => meshDense[y].Status == ChunkMesh.StatusMesh.Binding;
        /// <summary>
        /// Статсус связывания сетки с OpenGL для рендера альфа блоков
        /// </summary>
        public bool IsMeshAlphaBinding(int y) => meshAlpha[y].Status == ChunkMesh.StatusMesh.Binding;
        /// <summary>
        /// Статсус не пустой для рендера сплошных блоков
        /// </summary>
        public bool NotNullMeshDense(int y) => meshDense[y].Status != ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Статсус не пустой для рендера альфа блоков
        /// </summary>
        public bool NotNullMeshAlpha(int y) => meshAlpha[y].Status != ChunkMesh.StatusMesh.Null;
        /// <summary>
        /// Изменить статус на отмена рендеринга альфа блоков
        /// </summary>
        public void NotRenderingAlpha(int y) => meshAlpha[y].NotRendering();

        #endregion
    }
}
