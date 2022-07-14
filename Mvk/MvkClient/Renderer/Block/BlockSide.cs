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

        public vec3[] colors;
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

        //public BlockSide(vec3[] colors, byte[] lights, vec3 vec1, vec3 vec2, vec2 uv1, vec2 uv2, byte animationFrame, byte animationPause)
        //{
        //    this.colors = colors;
        //    v1 = vec1;
        //    v2 = vec2;
        //    u1 = uv1;
        //    u2 = uv2;
        //    this.animationFrame = animationFrame;
        //    this.animationPause = animationPause;

        //    this.lights = lights;
        //    yawUV = 0;
        //    yaw = 0f;
        //    pitch = 0f;
        //    isRotate = false;
        //    translate = new vec3(0);
        //    isTranslate = false;
        //    posCenter = new vec3();
        //    cullFace = true;
        //    BufferByte = new ByteBuffer();
        //}

        /// <summary>
        /// Ввернуть сторону блока, без проверки вращения 
        /// </summary>
        public void SideRotate(int pole)
        {
            switch (pole)
            {
                case 0: Up(); break;
                case 1: Down(); break;
                case 2: East(); break;
                case 3: West(); break;
                case 4: North(); break;
                case 5: South(); break;
            }
        }

        /// <summary>
        /// Вращение блок если это надо
        /// </summary>
        //private byte[] Rotate(byte[] buffer)
        //{
        //    if (radY != 0f || radP != 0f)
        //    {
        //        vec3 vY = new vec3(0, 1f, 0);
        //        vec3 vP = new vec3(1f, 0, 0);
        //        for (int i = 0; i < buffer.Length; i += 28)
        //        {
        //            float x = BitConverter.ToSingle(buffer, i);
        //            float y = BitConverter.ToSingle(buffer, i + 4);
        //            float z = BitConverter.ToSingle(buffer, i + 8);
        //            vec3 r = new vec3(x, y, z) - posCenter;
        //            if (radP != 0f) r = glm.rotate(r, radP, vP);
        //            if (radY != 0f) r = glm.rotate(r, radY, vY);
        //            r += posCenter;
        //            byte[] vs = BitConverter.GetBytes(r.x);
        //            Buffer.BlockCopy(vs, 0, buffer, i, 4);
        //            vs = BitConverter.GetBytes(r.y);
        //            Buffer.BlockCopy(vs, 0, buffer, i + 4, 4);
        //            vs = BitConverter.GetBytes(r.z);
        //            Buffer.BlockCopy(vs, 0, buffer, i + 8, 4);
        //        }
        //    }
        //    return buffer;
        //}

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
        private void Up() => BufferSide(v1x, v2y, v1z, v1x, v2y, v2z, v2x, v2y, v2z, v2x, v2y, v1z);
        /// <summary>
        /// Низ
        /// </summary>
        private void Down() => BufferSide(v2x, v1y, v1z, v2x, v1y, v2z, v1x, v1y, v2z, v1x, v1y, v1z);
        /// <summary>
        /// Восточная сторона
        /// </summary>
        private void East() => BufferSide(v2x, v1y, v1z, v2x, v2y, v1z, v2x, v2y, v2z, v2x, v1y, v2z);
        /// <summary>
        /// Западная сторона
        /// </summary>
        private void West() => BufferSide(v1x, v1y, v2z, v1x, v2y, v2z, v1x, v2y, v1z, v1x, v1y, v1z);
        /// <summary>
        /// Южная сторона
        /// </summary>
        private void North() => BufferSide(v1x, v1y, v1z, v1x, v2y, v1z, v2x, v2y, v1z, v2x, v1y, v1z);
        /// <summary>
        /// Северная сторона
        /// </summary>
        private void South() => BufferSide(v2x, v1y, v2z, v2x, v2y, v2z, v1x, v2y, v2z, v1x, v1y, v2z);

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

            GetBufferSide(new BlockVertex[] {
                new BlockVertex(pos1x, pos1y, pos1z, u1, v1, colors[0].x, colors[0].y, colors[0].z, lights[0]),
                new BlockVertex(pos2x, pos2y, pos2z, u2, v2, colors[1].x, colors[1].y, colors[1].z, lights[1]),
                new BlockVertex(pos3x, pos3y, pos3z, u3, v3, colors[2].x, colors[2].y, colors[2].z, lights[2]),
                new BlockVertex(pos4x, pos4y, pos4z, u4, v4, colors[3].x, colors[3].y, colors[3].z, lights[3])
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
            // pos.x, pos.y, pos.z, uv.x, uv.y
            float[] buf = blockVertex.GetArrayPosUV();

            if (isRotate)
            {
                vec3 r = new vec3(buf[0] - posCenterX, buf[1] - posCenterY, buf[2] - posCenterZ);
                if (pitch != 0f) r = glm.rotate(r, pitch, new vec3(1f, 0, 0));
                if (yaw != 0f) r = glm.rotate(r, yaw, new vec3(0, 1f, 0));
                r.x += posCenterX;
                r.y += posCenterY;
                r.z += posCenterZ;
                buf[0] = r.x;
                buf[1] = r.y;
                buf[2] = r.z;
                
            }
            if (isTranslate)
            {
                buf[0] += translateX;
                buf[1] += translateY;
                buf[2] += translateZ;
            }

            BufferByte.ArrayFloat(buf);
            BufferByte.ArrayByte(blockVertex.GetArrayColorLight());
            BufferByte.ArrayByte(new byte[] { animationFrame, animationPause, 0, 0 });
        }
    }
}
