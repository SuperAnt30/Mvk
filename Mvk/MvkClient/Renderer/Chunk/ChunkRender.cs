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
        private ChunkBase[] chunks = new ChunkBase[8];
        private bool isChunksAll = false;

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

            if (!isChunksAll)
            {
                int c = 0;
                for (int i = 0; i < 8; i++)
                {
                    vec2i pos = Position + MvkStatic.AreaOne8[i];
                    if (chunks[i] == null) chunks[i] = World.ChunkPr.GetChunk(pos);
                    else c++;
                }
                if (c == 8) isChunksAll = true;
            }
        }

        /// <summary>
        /// Получить соседний чанк, где x и y -1..1
        /// </summary>
        public ChunkBase Chunk(int x, int y) => chunks[MvkStatic.GetAreaOne8(x, y)];

        /// <summary>
        /// Рендер псевдо чанка, сплошных и альфа блоков
        /// </summary>
        public void Render(int chY)
        {
            try
            {
                long timeBegin, timeEnd;
                timeBegin = GLWindow.stopwatch.ElapsedTicks;

                ChunkStorage chunkStorage = StorageArrays[chY];
                // буфер альфа блоков
                List<BlockBuffer> alphas = new List<BlockBuffer>();
                vec3i posPlayer = ClientWorld.ClientMain.Player.PositionAlphaBlock;
                ushort id = 0;
                int cbX = Position.x << 4;
                int cbY = chY << 4;
                int cbZ = Position.y << 4;

                BlockAlphaRender blockAlphaRender = new BlockAlphaRender(this, cbX, cbY, cbZ);
                blockAlphaRender.blockUV.buffer = ClientWorld.WorldRender.bufferAlphaCache;

                BlockRender blockRender = new BlockRender(this, cbX, cbY, cbZ);
                blockRender.blockUV.buffer = ClientWorld.WorldRender.buffer;

                int realY, realZ, i;

                for (int y = 0; y < 16; y++)
                {
                    realY = cbY | y;
                    blockRender.posChunkY0 = y;
                    blockRender.posChunkY = realY;
                    for (int z = 0; z < 16; z++)
                    {
                        blockRender.posChunkZ = z;
                        for (int x = 0; x < 16; x++)
                        {
                            i = y << 8 | z << 4 | x;
                            id = chunkStorage.data[i];
                            if (id == 0 || id == 4096) continue;
                            blockRender.block = Blocks.blocksInt[id & 0xFFF];

                            if (blockRender.block.Translucent)
                            {
                                // Альфа!
                                blockAlphaRender.blockState.data = id;
                                blockAlphaRender.blockState.lightBlock = chunkStorage.lightBlock[i];
                                blockAlphaRender.blockState.lightSky = chunkStorage.lightSky[i];
                                blockAlphaRender.posChunkX = x;
                                blockAlphaRender.posChunkY = realY;
                                blockAlphaRender.posChunkZ = z;
                                realZ = cbZ | z;
                                blockAlphaRender.posChunkY0 = y;
                                blockAlphaRender.block = Blocks.blocksInt[id & 0xFFF];
                                blockAlphaRender.DamagedBlocksValue = GetDestroyBlocksValue(x, realY, z);
                                blockAlphaRender.RenderMesh();
                                if (blockAlphaRender.blockUV.buffer.count > 0)
                                {
                                    alphas.Add(new BlockBuffer()
                                    {
                                        buffer = blockAlphaRender.blockUV.buffer.ToArray(),
                                        distance = glm.distance(posPlayer, new vec3i(cbX | x, realY, realZ))
                                    });
                                    blockAlphaRender.blockUV.buffer.Clear();
                                }
                            }
                            else
                            {
                                blockRender.blockState.data = id;
                                blockRender.blockState.lightBlock = chunkStorage.lightBlock[i];
                                blockRender.blockState.lightSky = chunkStorage.lightSky[i];
                                blockRender.posChunkX = x;
                                blockRender.DamagedBlocksValue = GetDestroyBlocksValue(x, realY, z);
                                blockRender.RenderMesh();
                            }
                        }
                    }
                }
                //timeEnd = GLWindow.stopwatch.ElapsedTicks;
                meshDense[chY].SetBuffer(ClientWorld.WorldRender.buffer.ToArray());
                ClientWorld.WorldRender.buffer.Clear();
                timeEnd = GLWindow.stopwatch.ElapsedTicks;
                countAlpha[chY] = alphas.Count;
                ToBufferAlphaY(alphas);
                meshAlpha[chY].SetBuffer(ClientWorld.WorldRender.bufferAlpha.ToArray());
                //timeEnd = GLWindow.stopwatch.ElapsedTicks;
                float time = (timeEnd - timeBegin) / (float)MvkStatic.TimerFrequency;
                Debug.RenderChunckTime8 = (Debug.RenderChunckTime8 * 15f + time) / 16f;
            }
            catch (Exception ex)
            {
                Logger.Crach(ex);
            }
        }

        /// <summary>
        /// Вернуть массив буфера альфа
        /// </summary>
        public void ToBufferAlphaY(List<BlockBuffer> alphas)
        {
            ClientWorld.WorldRender.bufferAlpha.Clear();
            int count = alphas.Count;
            if (count > 0)
            {
                alphas.Sort();
                for (int i = count - 1; i >= 0; i--)
                {
                    ClientWorld.WorldRender.bufferAlpha.AddRange(alphas[i].buffer);
                }
            }
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
