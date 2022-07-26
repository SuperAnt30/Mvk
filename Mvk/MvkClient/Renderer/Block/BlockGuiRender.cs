using MvkClient.Util;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Объект рендера блока для GUI
    /// </summary>
    public class BlockGuiRender
    {
        /// <summary>
        /// Объект блока кэш
        /// </summary>
        private BlockBase block;
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
        private int cSide;
        /// <summary>
        /// Буфер всех блоков чанка
        /// </summary>
        private ListMvk<byte> buffer;

        /// <summary>
        /// Создание блока генерации для GUI
        /// </summary>
        public BlockGuiRender(BlockBase block) => this.block = block;

        /// <summary>
        /// Получить сетку блока с одной стороны
        /// </summary>
        private void RenderMeshBlock()
        {
            foreach (Box box in block.GetBoxes(0))
            {
                cBox = box;
                foreach (Face face in box.Faces)
                {
                    cFace = face;
                    if (face.side == -1)
                    {
                        for (int i = 0; i < 6; i++)
                        {
                            cSide = i;
                            RenderMeshFace();
                        }
                    }
                    else
                    {
                        cSide = (int)face.side;
                        RenderMeshFace();
                    }
                }
            }
        }

        /// <summary>
        /// Генерация сетки стороны коробки
        /// </summary>
        private void RenderMeshFace()
        {
            float u1 = cFace.u1;
            float v2 = cFace.v2;

            vec3 color = cFace.isColor ? cFace.color : new vec3(1f);
            float lightPole = block.NoSideDimming ? 0f : 1f - LightPole();
            color.x -= lightPole; if (color.x < 0) color.x = 0;
            color.y -= lightPole; if (color.y < 0) color.y = 0;
            color.z -= lightPole; if (color.z < 0) color.z = 0;
            byte cr = (byte)(color.x * 255);
            byte cg = (byte)(color.y * 255);
            byte cb = (byte)(color.z * 255);

            BlockSide blockUV = new BlockSide()
            {
                colorsr = new byte[] { cr, cr, cr, cr },
                colorsg = new byte[] { cg, cg, cg, cg },
                colorsb = new byte[] { cb, cb, cb, cb },
                lights = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF },
                v1x = cBox.From.x,
                v1y = cBox.From.y,
                v1z = cBox.From.z,
                v2x = cBox.To.x,
                v2y = cBox.To.y,
                v2z = cBox.To.z,
                u1x = u1 + cBox.UVFrom.x,
                u1y = v2 + cBox.UVTo.y,
                u2x = u1 + cBox.UVTo.x,
                u2y = v2 + cBox.UVFrom.y,
                animationFrame = cFace.animationFrame,
                animationPause = cFace.animationPause,
                yawUV = cBox.RotateYawUV,
                cullFace = true
            };

            if (cBox.Translate.x != 0 || cBox.Translate.y != 0 || cBox.Translate.z != 0)
            {
                blockUV.translateX = cBox.Translate.x;
                blockUV.translateY = cBox.Translate.y;
                blockUV.translateZ = cBox.Translate.z;
                blockUV.isTranslate = true;
            }
            if (cBox.RotateYaw != 0 || cBox.RotatePitch != 0)
            {
                blockUV.posCenterX = .5f;
                blockUV.posCenterY = .5f;
                blockUV.posCenterZ = .5f;
                blockUV.yaw = cBox.RotateYaw;
                blockUV.pitch = cBox.RotatePitch;
                blockUV.isRotate = true;
            }

            blockUV.buffer = buffer;
            blockUV.SideRotate(cSide);
        }

        /// <summary>
        /// Затемнение стороны от стороны блока
        /// </summary>
        private float LightPole()
        {
            switch (cSide)
            {
                case 0: return 1f;
                case 2: return 0.7f;
                case 3: return 0.7f;
                case 4: return 0.85f;
                case 5: return 0.85f;
            }
            return 0.6f;
        }

        /// <summary>
        /// Рендер блока VBO, конвертация из  VBO в DisplayList
        /// </summary>
        public void RenderVBOtoDL()
        {
            buffer = new ListMvk<byte>(4032);
            RenderMeshBlock();
            byte[] buffer2 = buffer.ToArray();

            GLRender.PushMatrix();
            {
                GLRender.Begin(OpenGL.GL_TRIANGLES);
                for (int i = 0; i < buffer2.Length; i += 28)
                {
                    float r = buffer2[i + 20] / 255f;
                    float g = buffer2[i + 21] / 255f;
                    float b = buffer2[i + 22] / 255f;
                    GLRender.Color(r, g, b);
                    float u = BitConverter.ToSingle(buffer2, i + 12);
                    float v = BitConverter.ToSingle(buffer2, i + 16);
                    GLRender.TexCoord(u, v);
                    float x = BitConverter.ToSingle(buffer2, i);
                    float y = BitConverter.ToSingle(buffer2, i + 4);
                    float z = BitConverter.ToSingle(buffer2, i + 8);
                    GLRender.Vertex(x - .5f, y - .5f, z - .5f);
                }
                GLRender.End();
            }
            GLRender.PopMatrix();
        }
    }
}
