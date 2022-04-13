using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока
    /// </summary>
    public class BlockRender
    {
        /// <summary>
        /// Объект рендера чанков
        /// </summary>
        public ChunkRender Chunk { get; protected set; }
        /// <summary>
        /// Объект блока
        /// </summary>
        public BlockBase Block { get; protected set; }
        /// <summary>
        /// Пометка, разрушается ли блок и его стадия
        /// -1 не разрушается, 0-9 разрушается
        /// </summary>
        public int DamagedBlocksValue { get; set; } = -1;

        /// <summary>
        /// позиция блока в чанке
        /// </summary>
        private vec3i posChunk;
        /// <summary>
        /// кэш коробка
        /// </summary>
        private Box cBox;
        /// <summary>
        /// кэш сторона блока
        /// </summary>
        private Face cFace;
        /// <summary>
        /// кэш Направление
        /// </summary>
        private Pole cSide;
        /// <summary>
        /// Объект создан для генерации блока в мире, не GUI
        /// </summary>
        private readonly bool isWorld = false;

        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        private bool cullFace = true;

        /// <summary>
        /// Создание блока генерации для мира
        /// </summary>
        public BlockRender(ChunkRender chunkRender, BlockBase block) : this(block)
        {
            isWorld = true;
            Chunk = chunkRender;
            // позиция блока в чанке
            posChunk = new vec3i(Block.Position.X & 15, Block.Position.Y, Block.Position.Z & 15);
        }

        /// <summary>
        /// Создание блока генерации для GUI
        /// </summary>
        public BlockRender(BlockBase block) => Block = block;


        /// <summary>
        /// Получть Сетку блока с возможностью двух сторон
        /// </summary>
        /// <returns>сетка</returns>
        public byte[] RenderMesh(bool check)
        {
            if (Block.EBlock == EnumBlock.Water)
            {
                List<byte> buffer = new List<byte>();
                cullFace = false;
                buffer.AddRange(RenderMeshBlock(check));
                cullFace = true;
                buffer.AddRange(RenderMeshBlock(check));
                return buffer.ToArray();
            }
            return RenderMeshBlock(check);
            
        }

        /// <summary>
        /// Получить сетку блока с одной стороны
        /// </summary>
        private byte[] RenderMeshBlock(bool check)
        {
            List<byte> buffer = new List<byte>();

            foreach (Box box in Block.Boxes)
            {
                cBox = box;
                foreach (Face face in box.Faces)
                {
                    cFace = face;
                    if (face.GetSide() == Pole.All)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cSide = (Pole)i;
                            if (check) RenderMeshSideCheck(buffer);
                            else buffer.AddRange(RenderMeshFace());
                        }
                    }
                    else
                    {
                        cSide = face.GetSide();
                        if (check) RenderMeshSideCheck(buffer);
                        else buffer.AddRange(RenderMeshFace());
                    }
                }
            }
            return buffer.ToArray();
        }

        /// <summary>
        /// Получть Сетку стороны блока с проверкой соседнего блока и разрушения его
        /// </summary>
        private void RenderMeshSideCheck(List<byte> buffer)
        {
            if (isWorld)
            {
                ResultSide resultSide = GetResultSide(cSide);

                if (resultSide.IsEmpty() || resultSide.IsDraw())
                {
                    buffer.AddRange(RenderMeshFace());
                    if (DamagedBlocksValue != -1)
                    {
                        Face face = cFace;
                        cFace = new Face(cSide, 4032 + DamagedBlocksValue, true, cFace.GetColor());
                        buffer.AddRange(RenderMeshFace());
                        cFace = face;
                    }

                }
            }
            else
            {
                buffer.AddRange(RenderMeshFace());
            }
        }


        /// <summary>
        /// Получить смещение на текстуру блока
        /// </summary>
        /// <returns>x = u1, Y = v1, z = u2, w = v2</returns>
        private vec4 GetUV()
        {
            float u1 = (cFace.GetNumberTexture() % 64) * 0.015625f;// 0.015625f;
            float v2 = cFace.GetNumberTexture() / 64 * 0.015625f;
            return new vec4(
                u1 + cBox.UVFrom.x, v2 + cBox.UVTo.y,
                u1 + cBox.UVTo.x, v2 + cBox.UVFrom.y
                );
        }

        /// <summary>
        /// Генерация сетки стороны коробки
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="side">направление блока</param>
        /// <param name="face">объект стороны блока</param>
        /// <param name="box">объект коробки</param>
        /// <param name="lightValue">яркость дневного соседнего блока</param>
        /// <returns></returns>
        private byte[] RenderMeshFace()
        {
            vec4 uv = GetUV();
            vec4 color = cFace.GetIsColor() ? new vec4(cFace.GetColor(), 1f) : new vec4(1f);
            // подготовка для теста плавности цвета
            //if (Block.EBlock == EnumBlock.Turf && cFace.GetIsColor() && Chunk.Position.x == -1 && Chunk.Position.y == -1)
            //{
            //    color = new vec4(.76f, .53f, .25f, 1f);
            //}

            bool isWater = Block.EBlock == EnumBlock.Water;

            float l = 1f - LightPole();
            color.x -= l; if (color.x < 0) color.x = 0;
            color.y -= l; if (color.y < 0) color.y = 0;
            color.z -= l; if (color.z < 0) color.z = 0;
            vec3 col = new vec3(color.x, color.y, color.z);
            //col = new vec3(1f);
            vec3i posi = Block.Position.Position;
            vec3 pos = new vec3(posi.x & 15, posi.y & 15, posi.z & 15);
            //vec3 pos = Block.Position.ToVec3();

            BlockSide blockUV = new BlockSide(
                col, 
                pos + cBox.From,
                pos + cBox.To,
                new vec2(uv.x, uv.y),
                new vec2(uv.z, uv.w),
                Block.AnimationFrame,
                Block.AnimationPause
            );

            if (cBox.RotateYaw != 0 || cBox.RotatePitch != 0)
            {
                blockUV.Rotate(pos + .5f, cBox.RotateYaw, cBox.RotatePitch);
            }
            return blockUV.Side(cSide, cullFace);
        }


        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSide)
            {
                case Pole.Up: return cullFace ? 1f : .6f;
                case Pole.South: return 0.85f;
                case Pole.East: return 0.7f;
                case Pole.West: return 0.7f;
                case Pole.North: return 0.85f;
            }
            return cullFace ? 0.6f : 1f;
        }

        /// <summary>
        /// Рендер блока VBO, конвертация из  VBO в DisplayList
        /// </summary>
        public void RenderVBOtoDL()
        {
            byte[] buffer = RenderMesh(false);

            GLRender.PushMatrix();
            {
                GLRender.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < buffer.Length; i += 28)
                {
                    float r = buffer[i + 20] / 255f;
                    float g = buffer[i + 21] / 255f;
                    float b = buffer[i + 22] / 255f;
                    GLRender.Color(r, g, b);
                    float u = BitConverter.ToSingle(buffer, i + 12);
                    float v = BitConverter.ToSingle(buffer, i + 16);
                    GLRender.TexCoord(u, v);
                    float x = BitConverter.ToSingle(buffer, i);
                    float y = BitConverter.ToSingle(buffer, i + 4);
                    float z = BitConverter.ToSingle(buffer, i + 8);
                    GLRender.Vertex(x - .5f, y - .5f, z - .5f);
                }
                GLRender.End();
            }
            GLRender.PopMatrix();
        }

        /// <summary>
        /// Получить результат стороны с соседним блоком
        /// </summary>
        /// <param name="side">сторона соседнего блока</param>
        private ResultSide GetResultSide(Pole side)
        {
            vec3i pos = posChunk + EnumFacing.DirectionVec(side);
            int yc = pos.y >> 4;
            // проверка высоты
            if (yc < 0 || yc >= ChunkBase.COUNT_HEIGHT) return new ResultSide();

            int xc = Chunk.Position.x + (pos.x >> 4);
            int zc = Chunk.Position.y + (pos.z >> 4);
            int xv = pos.x & 15;
            int yv = pos.y & 15;
            int zv = pos.z & 15;

            // Определяем рабочий чанк соседнего блока
            ChunkBase chunk = (xc == Chunk.Position.x && zc == Chunk.Position.y) ? Chunk
                : Chunk.World.ChunkPr.GetChunk(new vec2i(xc, zc));

            if (chunk == null) return new ResultSide();

            EnumBlock eBlock = chunk.StorageArrays[yc].GetEBlock(xv, yv, zv);
            BlockBase block = Blocks.GetBlockCache(eBlock);

            bool isDraw = eBlock == EnumBlock.Air || !block.IsNotTransparent();

            // Для слияния однотипных блоков
            if (isDraw && (eBlock == Block.EBlock /*|| (Block.IsWater && Blocks.IsWater(eBlock)*/))
            {
                isDraw = false;
            }

            if (!isDraw && Block.AllDrawing) isDraw = true;

            return new ResultSide(
                isDraw,
                chunk.StorageArrays[yc].GetLightFor(xv, yv, zv, EnumSkyBlock.Sky),
                chunk.StorageArrays[yc].GetLightFor(xv, yv, zv, EnumSkyBlock.Block),
                chunk.StorageArrays[yc].GetLightsFor(xv, yv, zv),
                block
            );
        }

        /// <summary>
        /// Результат стороны блока
        /// </summary>
        private struct ResultSide
        {
            private readonly bool isDraw;
            private readonly byte lightSky;
            private readonly byte lightBlock;
            private readonly byte light;
            private readonly BlockBase block;
            private readonly bool body;

            /// <summary>
            /// Результат стороны блока
            /// </summary>
            /// <param name="isDraw">Прорисовывать ли сторону</param>
            /// <param name="lightSky">Яркость света от блока</param>
            /// <param name="lightBlock">Яркость света от блока</param>
            /// <param name="light">Общаяя яркость света неба и блока в одном байте</param>
            /// <param name="blockCache">блок кэша для параметров</param>
            public ResultSide(bool isDraw, byte lightSky, byte lightBlock, byte light, BlockBase blockCache)
            {
                this.isDraw = isDraw;
                this.lightSky = lightSky;
                this.lightBlock = lightBlock;
                this.light = light;
                block = blockCache;
                body = true;
            }

            /// <summary>
            /// Прорисовывать ли сторону
            /// </summary>
            public bool IsDraw() => isDraw;
            /// <summary>
            /// Яркость света неба
            /// </summary>
            public byte LightSky() => lightSky;
            /// <summary>
            /// Яркость света от блока
            /// </summary>
            public byte LightBlock() => lightBlock;
            /// <summary>
            /// Общаяя яркость света неба и блока в одном байте
            /// </summary>
            public byte Light() => light;
            /// <summary>
            /// Блок кэша
            /// </summary>
            public BlockBase BlockCache() => block;
            /// <summary>
            /// Пустой ли объект
            /// </summary>
            public bool IsEmpty() => !body;
        }
    }
}
