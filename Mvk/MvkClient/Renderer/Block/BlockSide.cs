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
        private readonly vec3 v1;
        private readonly vec3 v2;
        private readonly vec2 u1;
        private readonly vec2 u2;

        private readonly vec3 color;
        private float radY;
        private float radP;
        private vec3 posCenter;

        private vec3 c;
        private vec4 l;

        private readonly byte animationFrame;
        private readonly byte animationPause;
        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        private bool cullFace;

        public BlockSide(vec3 color, vec3 vec1, vec3 vec2, vec2 uv1, vec2 uv2, byte animationFrame, byte animationPause)
        {
            this.color = color;
            v1 = vec1;
            v2 = vec2;
            u1 = uv1;
            u2 = uv2;
            this.animationFrame = animationFrame;
            this.animationPause = animationPause;

            l = new vec4(255f);
            c = color;
            radY = 0f;
            radP = 0f;
            posCenter = new vec3();
            cullFace = true;
        }

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
        public byte[] Side(Pole pole, bool cullFace)
        {
            this.cullFace = cullFace;
            return Rotate(SideNotRotate(pole));
        }

        /// <summary>
        /// Ввернуть сторону блока, без проверки вращения 
        /// </summary>
        private byte[] SideNotRotate(Pole pole)
        {
            switch (pole)
            {
                case Pole.Up: return Up();
                case Pole.Down: return Down();
                case Pole.East: return East();
                case Pole.West: return West();
                case Pole.North: return North();
                case Pole.South: return South();
                default: return new byte[0];
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
        private byte[] Up()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Низ
        /// </summary>
        private byte[] Down()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Восточная сторона
        /// </summary>
        private byte[] East()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Западная сторона
        /// </summary>
        private byte[] West()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Южная сторона
        /// </summary>
        private byte[] North()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v1.x, v1.y, v1.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v1.x, v2.y, v1.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v2.x, v2.y, v1.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v2.x, v1.y, v1.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Северная сторона
        /// </summary>
        private byte[] South()
        {
            return GetBufferSide(new BlockVertex[] {
                new BlockVertex(new vec3(v2.x, v1.y, v2.z), new vec2(u2.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.x),
                new BlockVertex(new vec3(v2.x, v2.y, v2.z), new vec2(u2.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.y),
                new BlockVertex(new vec3(v1.x, v2.y, v2.z), new vec2(u1.x, u2.y), new vec3(color.x, color.y, color.z), 0, l.z),
                new BlockVertex(new vec3(v1.x, v1.y, v2.z), new vec2(u1.x, u1.y), new vec3(color.x, color.y, color.z), 0, l.w)
            });
        }

        /// <summary>
        /// Сгенерировать буфер сетки одной стороны
        /// </summary>
        /// <param name="blockVertex">массив вершин</param>
        private byte[] GetBufferSide(BlockVertex[] blockVertex)
        {
            ByteBuffer byteBuffer = new ByteBuffer();
            if (cullFace)
            {
                AddVertex(byteBuffer, blockVertex[0]);
                AddVertex(byteBuffer, blockVertex[1]);
                AddVertex(byteBuffer, blockVertex[2]);
                AddVertex(byteBuffer, blockVertex[0]);
                AddVertex(byteBuffer, blockVertex[2]);
                AddVertex(byteBuffer, blockVertex[3]);
            }
            else
            {
                AddVertex(byteBuffer, blockVertex[2]);
                AddVertex(byteBuffer, blockVertex[1]);
                AddVertex(byteBuffer, blockVertex[0]);
                AddVertex(byteBuffer, blockVertex[3]);
                AddVertex(byteBuffer, blockVertex[2]);
                AddVertex(byteBuffer, blockVertex[0]);
            }

            return byteBuffer.ToArray();
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(ByteBuffer byteBuffer, BlockVertex blockVertex)
        {
            byteBuffer.ArrayFloat(blockVertex.GetArrayPosUV());
            byteBuffer.ArrayByte(blockVertex.GetArrayColorLight());
            byteBuffer.ArrayByte(new byte[] { animationFrame, animationPause, 0, 0 });
        }
    }
}
