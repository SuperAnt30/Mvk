using MvkClient.Renderer.Chunk;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
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

        /// <summary>
        /// Получть Сетку стороны блока
        /// </summary>
        protected void RenderMeshSide(List<float> buffer, Pole side)
        {
            EnumBlock enumBlock = GetEBlock(posChunk + EnumFacing.DirectionVec(side));
            //_br = BlockedLight(posChunk + EnumFacing.DirectionVec(side));
            //   if (Blk.AllDrawing || _br.IsDraw)
            if (enumBlock == EnumBlock.Air)
            {
                cSide = side;
                buffer.AddRange(RenderMeshFace());
            }
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
            float u1 = (cFace.GetNumberTexture() % 64) * 0.015625f;// 0.015625f;
            float v2 = cFace.GetNumberTexture() / 64 * 0.015625f;
            vec4 color = (Block.IsGrass && cFace.GetIsColor()) ? new vec4(0.56f, 0.73f, 0.35f, 1f) : new vec4(1f);
            float l = 1f - LightPole();
            color.x -= l; if (color.x < 0) color.x = 0;
            color.y -= l; if (color.y < 0) color.y = 0;
            color.z -= l; if (color.z < 0) color.z = 0;
            vec3 col = new vec3(color.x, color.y, color.z);
            //col = new vec3(1f);
            vec3 pos = Block.Position.ToVec3();
            BlockFaceUV blockUV = new BlockFaceUV(col, pos);

            blockUV.SetVecUV(
                pos + cBox.From,
                pos + cBox.To,
                new vec2(u1 + cBox.UVFrom.x, v2 + cBox.UVTo.y),
                new vec2(u1 + cBox.UVTo.x, v2 + cBox.UVFrom.y)
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
                case Pole.South: return 0.9f;
                case Pole.East: return 0.8f;
                case Pole.West: return 0.8f;
                case Pole.North: return 0.9f;
            }
            return 0.7f;
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
    }
}
