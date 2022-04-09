using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using SharpGL;
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
        /// Получть Сетку блока
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <returns>сетка</returns>
        public float[] RenderMesh(bool check)
        {
            List<float> buffer = new List<float>();

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
        private void RenderMeshSideCheck(List<float> buffer)
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
        private float[] RenderMeshFace()
        {
            vec4 uv = GetUV();
            vec4 color = cFace.GetIsColor() ? new vec4(cFace.GetColor(), 1f) : new vec4(1f);
            float l = 1f - LightPole();
            color.x -= l; if (color.x < 0) color.x = 0;
            color.y -= l; if (color.y < 0) color.y = 0;
            color.z -= l; if (color.z < 0) color.z = 0;
            vec3 col = new vec3(color.x, color.y, color.z);
            //col = new vec3(1f);
            vec3i posi = Block.Position.Position;
            vec3 pos = new vec3(posi.x & 15, posi.y & 15, posi.z & 15);
            //vec3 pos = Block.Position.ToVec3();
            BlockFaceUV blockUV = new BlockFaceUV(col, pos);

            blockUV.SetVecUV(
                pos + cBox.From,
                pos + cBox.To,
                new vec2(uv.x, uv.y),
                new vec2(uv.z, uv.w)
            );

            blockUV.RotateYaw(cBox.RotateYaw);
            blockUV.RotatePitch(cBox.RotatePitch);

            return blockUV.Side(cSide);
        }


        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSide)
            {
                case Pole.Up: return 1f;
                case Pole.South: return 0.85f;
                case Pole.East: return 0.7f;
                case Pole.West: return 0.7f;
                case Pole.North: return 0.85f;
            }
            return 0.6f;
        }

        /// <summary>
        /// Рендер блока VBO, конвертация из  VBO в DisplayList
        /// </summary>
        public void RenderVBOtoDL()
        {
            float[] buffer = RenderMesh(false);

            GLRender.PushMatrix();
            {
                GLRender.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < buffer.Length; i +=10)
                {
                    GLRender.Color(buffer[i + 5], buffer[i + 6], buffer[i + 7]);
                    GLRender.TexCoord(buffer[i + 3], buffer[i + 4]);
                    GLRender.Vertex(buffer[i] - .5f, buffer[i + 1] - .5f, buffer[i + 2] - .5f);
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
