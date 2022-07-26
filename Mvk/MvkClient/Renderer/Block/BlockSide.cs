using MvkServer.Glm;
using MvkServer.Util;
using System;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Построение блока с разных сторон
    /// </summary>
    public class BlockSide
    {
        public ListMvk<byte> buffer;

        public float v1x;
        public float v1y;
        public float v1z;
        public float v2x;
        public float v2y;
        public float v2z;
        public float u1x;
        public float u1y;
        public float u2x;
        public float u2y;

        public byte[] colorsr;
        public byte[] colorsg;
        public byte[] colorsb;
        public byte[] lights;
        public int yawUV;
        public float yaw;
        public float pitch;
        public bool isRotate;
        public float translateX;
        public float translateY;
        public float translateZ;
        public bool isTranslate;
        public float posCenterX;
        public float posCenterY;
        public float posCenterZ;

        public byte animationFrame;
        public byte animationPause;
        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        public bool cullFace;

        /// <summary>
        /// Ввернуть сторону блока, без проверки вращения 
        /// </summary>
        public void SideRotate(int pole)
        {
            switch (pole)
            {
                case 0: BufferSide(v1x, v2y, v1z, v1x, v2y, v2z, v2x, v2y, v2z, v2x, v2y, v1z); break;
                case 1: BufferSide(v2x, v1y, v1z, v2x, v1y, v2z, v1x, v1y, v2z, v1x, v1y, v1z); break;
                case 2: BufferSide(v2x, v1y, v1z, v2x, v2y, v1z, v2x, v2y, v2z, v2x, v1y, v2z); break;
                case 3: BufferSide(v1x, v1y, v2z, v1x, v2y, v2z, v1x, v2y, v1z, v1x, v1y, v1z); break;
                case 4: BufferSide(v1x, v1y, v1z, v1x, v2y, v1z, v2x, v2y, v1z, v2x, v1y, v1z); break;
                case 5: BufferSide(v2x, v1y, v2z, v2x, v2y, v2z, v1x, v2y, v2z, v1x, v1y, v2z); break;
            }
        }

        private void BufferSide(float pos1x, float pos1y, float pos1z,
            float pos2x, float pos2y, float pos2z,
            float pos3x, float pos3y, float pos3z,
            float pos4x, float pos4y, float pos4z)
        {
            float u1, u2, u3, u4;
            float v1, v2, v3, v4;

            if (yawUV == 1)
            {
                u2 = u2x; v2 = u1y;
                u3 = u2x; v3 = u2y;
                u4 = u1x; v4 = u2y;
                u1 = u1x; v1 = u1y;
            }
            else if(yawUV == 2)
            {
                u3 = u2x; v3 = u1y;
                u4 = u2x; v4 = u2y;
                u1 = u1x; v1 = u2y;
                u2 = u1x; v2 = u1y;
            }
            else if (yawUV == 3)
            {
                u4 = u2x; v4 = u1y;
                u1 = u2x; v1 = u2y;
                u2 = u1x; v2 = u2y;
                u3 = u1x; v3 = u1y;
            }
            else
            {
                u1 = u2x; v1 = u1y;
                u2 = u2x; v2 = u2y;
                u3 = u1x; v3 = u2y;
                u4 = u1x; v4 = u1y;
            }

            if (cullFace)
            {
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
            }
            else
            {
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos2x, pos2y, pos2z, u2, v2, colorsr[1], colorsg[1], colorsb[1], lights[1]);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
                AddVertex(pos4x, pos4y, pos4z, u4, v4, colorsr[3], colorsg[3], colorsb[3], lights[3]);
                AddVertex(pos3x, pos3y, pos3z, u3, v3, colorsr[2], colorsg[2], colorsb[2], lights[2]);
                AddVertex(pos1x, pos1y, pos1z, u1, v1, colorsr[0], colorsg[0], colorsb[0], lights[0]);
            }
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        private void AddVertex(float x, float y, float z, float u, float v, byte r, byte g, byte b, byte light)
        {
            // pos.x, pos.y, pos.z, uv.x, uv.y
            if (isRotate)
            {
                vec3 vec = new vec3(x - posCenterX, y - posCenterY, z - posCenterZ);
                if (pitch != 0f) vec = glm.rotate(vec, pitch, new vec3(1f, 0, 0));
                if (yaw != 0f) vec = glm.rotate(vec, yaw, new vec3(0, 1f, 0));
                vec.x += posCenterX;
                vec.y += posCenterY;
                vec.z += posCenterZ;
                x = vec.x;
                y = vec.y;
                z = vec.z;

            }
            if (isTranslate)
            {
                x += translateX;
                y += translateY;
                z += translateZ;
            }

            buffer.AddRange(BitConverter.GetBytes(x));
            buffer.AddRange(BitConverter.GetBytes(y));
            buffer.AddRange(BitConverter.GetBytes(z));
            buffer.AddRange(BitConverter.GetBytes(u));
            buffer.AddRange(BitConverter.GetBytes(v));
            buffer.buffer[buffer.count++] = r;
            buffer.buffer[buffer.count++] = g;
            buffer.buffer[buffer.count++] = b;
            buffer.buffer[buffer.count++] = light;
            buffer.buffer[buffer.count++] = animationFrame;
            buffer.buffer[buffer.count++] = animationPause;
            buffer.count += 2;
        }
    }
}
