using MvkClient.Util;
using MvkServer.Glm;
using MvkServer.Util;
using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Построение блока с разных сторон
    /// </summary>
    public struct BlockSide
    {
        public ByteBuffer BufferByte;

        private readonly vec3 v1;
        private readonly vec3 v2;
        private readonly vec2 u1;
        private readonly vec2 u2;

        private readonly vec3[] colors;
        private readonly byte[] lights;
        private float radY;
        private float radP;
        private vec3 posCenter;

      //  private vec4 l;

        private readonly byte animationFrame;
        private readonly byte animationPause;
        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        private bool cullFace;

        public BlockSide(vec3[] colors, byte[] lights, vec3 vec1, vec3 vec2, vec2 uv1, vec2 uv2, byte animationFrame, byte animationPause)
        {
            this.colors = colors;
            v1 = vec1;
            v2 = vec2;
            u1 = uv1;
            u2 = uv2;
            this.animationFrame = animationFrame;
            this.animationPause = animationPause;

            this.lights = lights;
            // l = new vec4(255f);
            radY = 0f;
            radP = 0f;
            posCenter = new vec3();
            cullFace = true;
            BufferByte = new ByteBuffer();
        }

        ///// <summary>
        ///// Задать цвет сторон
        ///// </summary>
        //public void SetColor(vec3 color1, vec3 color2, vec3 color3, vec3 color4) 
        //    => colors = new vec3[] { color1, color2, color3, color4 };

        /// <summary>
        /// Указываем вращение блока в радианах
        /// </summary>
        public void Rotate(vec3 posCenter, float yaw, float pitch)
        {
            this.posCenter = posCenter;
            radY = yaw;
            radP = pitch;
        }

        /// <summary>
        /// Ввернуть сторону блока с проверкой вращения
        /// </summary>
        public void Side(Pole pole, bool cullFace)
        {
            this.cullFace = cullFace;
            //return Rotate(SideNotRotate(pole));
            SideNotRotate(pole);
        }

        /// <summary>
        /// Ввернуть сторону блока, без проверки вращения 
        /// </summary>
        private void SideNotRotate(Pole pole)
        {
            switch (pole)
            {
                case Pole.Up: Up(); break;
                case Pole.Down: Down(); break;
                case Pole.East: East(); break;
                case Pole.West: West(); break;
                case Pole.North: North(); break;
                case Pole.South: South(); break;
            }
        }

        /// <summary>
        /// Вращение блок если это надо
        /// </summary>
        private byte[] Rotate(byte[] buffer)
        {
            if (radY != 0f || radP != 0f)
            {
                vec3 vY = new vec3(0, 1f, 0);
                vec3 vP = new vec3(1f, 0, 0);
                for (int i = 0; i < buffer.Length; i += 28)
                {
                    float x = BitConverter.ToSingle(buffer, i);
                    float y = BitConverter.ToSingle(buffer, i + 4);
                    float z = BitConverter.ToSingle(buffer, i + 8);
                    vec3 r = new vec3(x, y, z) - posCenter;
                    if (radP != 0f) r = glm.rotate(r, radP, vP);
                    if (radY != 0f) r = glm.rotate(r, radY, vY);
                    r += posCenter;
                    byte[] vs = BitConverter.GetBytes(r.x);
                    Buffer.BlockCopy(vs, 0, buffer, i, 4);
                    vs = BitConverter.GetBytes(r.y);
                    Buffer.BlockCopy(vs, 0, buffer, i + 4, 4);
                    vs = BitConverter.GetBytes(r.z);
                    Buffer.BlockCopy(vs, 0, buffer, i + 8, 4);
                }
            }
            return buffer;
        }

        //private vec3 Rotate(vec3 v)
        //{
        //    vec3 r = v - posCenter;
        //    if (radP != 0f) r = glm.rotate(r, radP, new vec3(1f, 0, 0));
        //    if (radY != 0f) r = glm.rotate(r, radY, new vec3(0, 1f, 0));
        //    r += posCenter;
        //    return r;
        //}

        /// <summary>
        /// Вверх
        /// </summary>
        private void Up()
        {
            BufferSide(new vec3(v1.x, v2.y, v1.z), new vec3(v1.x, v2.y, v2.z), new vec3(v2.x, v2.y, v2.z), new vec3(v2.x, v2.y, v1.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        /// <summary>
        /// Низ
        /// </summary>
        private void Down()
        {
            BufferSide(new vec3(v2.x, v1.y, v1.z), new vec3(v2.x, v1.y, v2.z), new vec3(v1.x, v1.y, v2.z), new vec3(v1.x, v1.y, v1.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        /// <summary>
        /// Восточная сторона
        /// </summary>
        private void East()
        {
            BufferSide(new vec3(v2.x, v1.y, v1.z), new vec3(v2.x, v2.y, v1.z), new vec3(v2.x, v2.y, v2.z), new vec3(v2.x, v1.y, v2.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        /// <summary>
        /// Западная сторона
        /// </summary>
        private void West()
        {
            BufferSide(new vec3(v1.x, v1.y, v2.z), new vec3(v1.x, v2.y, v2.z), new vec3(v1.x, v2.y, v1.z), new vec3(v1.x, v1.y, v1.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        /// <summary>
        /// Южная сторона
        /// </summary>
        private void North()
        {
            BufferSide(new vec3(v1.x, v1.y, v1.z), new vec3(v1.x, v2.y, v1.z), new vec3(v2.x, v2.y, v1.z), new vec3(v2.x, v1.y, v1.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        /// <summary>
        /// Северная сторона
        /// </summary>
        private void South()
        {
            BufferSide(new vec3(v2.x, v1.y, v2.z), new vec3(v2.x, v2.y, v2.z), new vec3(v1.x, v2.y, v2.z), new vec3(v1.x, v1.y, v2.z));
            //return GetBufferSide(new BlockVertex[] {
            //    new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u2.x, u1.y), color[0], 0),
            //    new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u2.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u1.x, u2.y), color[0], 0),
            //    new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u1.x, u1.y), color[0], 0)
            //});
        }

        private void BufferSide(vec3 pos1, vec3 pos2, vec3 pos3, vec3 pos4)
        {
            GetBufferSide(new BlockVertex[] {
                new BlockVertex(pos1, new vec2(u2.x, u1.y), colors[0], lights[0]),
                new BlockVertex(pos2, new vec2(u2.x, u2.y), colors[1], lights[1]),
                new BlockVertex(pos3, new vec2(u1.x, u2.y), colors[2], lights[2]),
                new BlockVertex(pos4, new vec2(u1.x, u1.y), colors[3], lights[3])
            });
        }

        /// <summary>
        /// Сгенерировать буфер сетки одной стороны
        /// </summary>
        /// <param name="blockVertex">массив вершин</param>
        private void GetBufferSide(BlockVertex[] blockVertex)
        {
            if (cullFace)
            {
                AddVertex(blockVertex[0]);
                AddVertex(blockVertex[1]);
                AddVertex(blockVertex[2]);
                AddVertex(blockVertex[0]);
                AddVertex(blockVertex[2]);
                AddVertex(blockVertex[3]);
            }
            else
            {
                AddVertex(blockVertex[2]);
                AddVertex(blockVertex[1]);
                AddVertex(blockVertex[0]);
                AddVertex(blockVertex[3]);
                AddVertex(blockVertex[2]);
                AddVertex(blockVertex[0]);
            }
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(BlockVertex blockVertex)
        {
            BufferByte.ArrayFloat(blockVertex.GetArrayPosUV());
            BufferByte.ArrayByte(blockVertex.GetArrayColorLight());
            BufferByte.ArrayByte(new byte[] { animationFrame, animationPause, 0, 0 });
        }
    }
}
