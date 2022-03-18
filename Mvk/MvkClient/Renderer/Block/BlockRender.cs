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
        /// позиция блока в чанке
        /// </summary>
        protected vec3i posChunk;
        /// <summary>
        /// кэш коробка
        /// </summary>
        protected Box cBox;
        /// <summary>
        /// кэш сторона блока
        /// </summary>
        protected Face cFace;
        /// <summary>
        /// кэш Направление
        /// </summary>
        protected Pole cSide;

        public BlockRender(ChunkRender chunkRender, BlockBase block)
        {
            Chunk = chunkRender;
            Block = block;
            // позиция блока в чанке
            posChunk = new vec3i(Block.Position.X & 15, Block.Position.Y, Block.Position.Z & 15);
        }


        /// <summary>
        /// Получть Сетку блока
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <returns>сетка</returns>
        public float[] RenderMesh()
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
                            RenderMeshSide(buffer, (Pole)i);
                        }
                    }
                    else
                    {
                        RenderMeshSide(buffer, face.GetSide());
                    }
                }
            }
            return buffer.ToArray();
        }

        public int DamagedBlocksValue { get; set; } = -1;

        /// <summary>
        /// Получть Сетку стороны блока
        /// </summary>
        protected void RenderMeshSide(List<float> buffer, Pole side)
        {
            EnumBlock enumBlock = GetEBlock(posChunk + EnumFacing.DirectionVec(side));
            //_br = BlockedLight(posChunk + EnumFacing.DirectionVec(side));
            //   if (Blk.AllDrawing || _br.IsDraw)
            if (enumBlock == EnumBlock.Air || Block.AllDrawing || enumBlock == EnumBlock.Cobblestone)
            {
                cSide = side;
                buffer.AddRange(RenderMeshFace());
                if (DamagedBlocksValue == -1)
                {
                    buffer.AddRange(RenderMeshFace());
                }
                else
                {
                    // TODO::Face 4037
                    Face face = cFace;
                    cFace = new Face(side, 4032 + DamagedBlocksValue, true, cFace.GetColor());
                    buffer.AddRange(RenderMeshFace());
                    cFace = face;
                }

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
        protected float[] RenderMeshFace()
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
            vec3 pos = new vec3(posi.x & 15, posi.y, posi.z & 15);
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
        protected float LightPole()
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
        /// Поиск тип блока
        /// </summary>
        public EnumBlock GetEBlock(vec3i pos)
        {
            if (pos.y < 0 || pos.y > 255) return EnumBlock.Air;

            int xc = Chunk.Position.x + (pos.x >> 4);
            int zc = Chunk.Position.y + (pos.z >> 4);
            int xv = pos.x & 15;
            int zv = pos.z & 15;

            if (xc == Chunk.Position.x && zc == Chunk.Position.y)
            {
                // Соседний блок в этом чанке
                return Chunk.GetEBlock(new vec3i(xv, pos.y, zv));
            }
            // Соседний блок в соседнем чанке
            ChunkBase chunk = Chunk.World.ChunkPr.GetChunk(new vec2i(xc, zc));
            if (chunk != null) return chunk.GetEBlock(new vec3i(xv, pos.y, zv));

            return EnumBlock.Air;
        }

        /// <summary>
        /// Рендер блока в 2d для GUI
        /// </summary>
        public void RenderGui()
        {
            foreach (Box box in Block.Boxes)
            {
                cBox = box;
                foreach (Face face in box.Faces)
                {
                    cFace = face;
                    if (face.GetSide() == Pole.All || face.GetSide() == Pole.Up)
                    {
                        RenderSideDL(12, 6, 0, 0, 0, 12, -12, 6, 1f);
                    }
                    if (face.GetSide() == Pole.All || face.GetSide() == Pole.South)
                    {
                        RenderSideDL(12, 6, 0, 12, 12, 20, 0, 26, .5f);
                    }
                    if (face.GetSide() == Pole.All || face.GetSide() == Pole.East)
                    {
                        RenderSideDL(0, 12, -12, 6, 0, 26, -12, 20, .7f);
                    }
                }
            }
        }

        /// <summary>
        /// Рендер сторона DL
        /// </summary>
        private void RenderSideDL(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4, float color)
        {
            vec4 uv = GetUV();
            GLRender.PushMatrix();
            {
                GLRender.Color(new vec4((cFace.GetIsColor() ? cFace.GetColor() : new vec3(1f)) * color, 0.7f));
                GLRender.Begin(OpenGL.GL_TRIANGLE_STRIP);
                GLWindow.gl.TexCoord(uv.x, uv.w); GLRender.Vertex(x1, y1);
                GLWindow.gl.TexCoord(uv.z, uv.w); GLRender.Vertex(x2, y2);
                GLWindow.gl.TexCoord(uv.x, uv.y); GLRender.Vertex(x3, y3);
                GLWindow.gl.TexCoord(uv.z, uv.y); GLRender.Vertex(x4, y4);
                GLRender.End();
            }
            GLRender.PopMatrix();
        }
    }
}
