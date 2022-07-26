
//using MvkClient.Util;
//using MvkServer.Glm;
//using MvkServer.Util;

//namespace MvkClient.Renderer.Block
//{
//    /// <summary>
//    /// Построение блока с разных сторон
//    /// </summary>
//    public struct BlockFaceUV
//    {
//        private vec3 v1;
//        private vec3 v2;
//        private vec3 v3;
//        private vec3 v4;

//        private vec2 u1;
//        private vec2 u2;
//        private vec2 u3;
//        private vec2 u4;

//        private vec3 c;
//        /// <summary>
//        /// Тень стороны
//        /// </summary>
//        private vec4 l;

//        private vec2 leg;
//        /// <summary>
//        /// Позиция центра блока, для вращения
//        /// </summary>
//        private vec3 pos;

//        /// <summary>
//        /// сторона прямоугольник ли
//        /// </summary>
//        private bool isRectangle;
//        private float radY;
//        private float radP;


//        public BlockFaceUV(vec3 color, vec3 pos)
//        {
//            c = color;
//            this.pos = pos + .5f;

//            v1 = new vec3();
//            v2 = new vec3();
//            v3 = new vec3();
//            v4 = new vec3();
//            u1 = new vec2();
//            u2 = new vec2();
//            u3 = new vec2();
//            u4 = new vec2();
//            radY = 0f;
//            radP = 0f;
//            isRectangle = true;
//            l = new vec4(1f);
//            leg = new vec2();
//        }

//        public void SetVecUV(vec3 vec1, vec3 vec2, vec2 uv1, vec2 uv2)
//        {
//            isRectangle = true;
//            v1 = vec1;
//            v2 = vec2;
//            u1 = uv1;
//            u2 = uv2;
//        }

//        public void SetVec(vec3 vec1, vec3 vec2, vec3 vec3, vec3 vec4)
//        {
//            isRectangle = false;
//            v1 = vec1;
//            v2 = vec2;
//            v3 = vec3;
//            v4 = vec4;
//        }

//        public void SetUV(vec2 uv1, vec2 uv2, vec2 uv3, vec2 uv4)
//        {
//            u1 = uv1;
//            u2 = uv2;
//            u3 = uv3;
//            u4 = uv4;
//        }

        
//        /// <summary>
//        /// Указываем вращение блока по оси Y в радианах
//        /// </summary>
//        public void RotateYaw(float rad) => radY = rad;
        
//        /// <summary>
//        /// Указываем вращение блока по оси X в радианах
//        /// </summary>
//        public void RotatePitch(float rad) => radP = rad;

//        /// <summary>
//        /// Ввернуть сторону по индексу
//        /// </summary>
//        /// <param name="index">индекс стороны</param>
//        /// <param name="l">освещение стороны</param>
//        public byte[] Side(Pole pole, bool isWater)//, vec4 l)
//        {
//            //vec4 l = new vec4(1f);
//            ByteBuffer byteBuffer = new ByteBuffer();
//            if (isRectangle)
//            {
//                if (pole != Pole.Up) return new byte[0];
//                return Up(isWater);
//                //float[] f;
//                //switch (pole)
//                //{
//                //    //case Pole.Down: f = Down(); break;
//                //    //case Pole.East: f = East(); break;
//                //    //case Pole.West: f = West(); break;
//                //    //case Pole.North: f = North(); break;
//                //    //case Pole.South: f = South(); break;
//                //    default: f = Up(); break;
//                //}
//                ////byteBuffer.ArrayByte(f);
//                //byteBuffer.ArrayFloat(Rotate(f));
//                //return Rotate(f);
//            }
//            else
//            {
//                byteBuffer.ArrayFloat(NotRectangle());
//            }
//            return byteBuffer.ToArray();
//            //return NotRectangle();
//        }

//        private float[] Rotate(float[] f)
//        {
//            if (radY == 0f && radP == 0f) return f;
//            vec3 vY = new vec3(0, 1f, 0);
//            vec3 vP = new vec3(1f, 0, 0);
//            for (int i = 0; i < 60; i += 10)
//            {
//                vec3 r = new vec3(f[i], f[i + 1], f[i + 2]) - pos;
//                if (radP != 0f) r = glm.rotate(r, radP, vP);
//                if (radY != 0f) r = glm.rotate(r, radY, vY);
//                r += pos;
//                f[i] = r.x;
//                f[i + 1] = r.y;
//                f[i + 2] = r.z;
//            }
//            return f;
//        }

//        private vec3 Rotate(vec3 v)
//        {
//            vec3 r = v - pos;
//            if (radP != 0f) r = glm.rotate(r, radP, new vec3(1f, 0, 0));
//            if (radY != 0f) r = glm.rotate(r, radY, new vec3(0, 1f, 0));
//            r += pos;
//            return r;
//        }

//        public float[] NotRectangle()
//        {
//            return new float[] {
//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v3.x, v3.y, v3.z, u3.x, u3.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v3.x, v3.y, v3.z, u3.x, u3.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,
//                v4.x, v4.y, v4.z, u4.x, u4.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y
//            };
//        }

//        /// <summary>
//        /// Вверх
//        /// </summary>
//        public byte[] Up(bool isWater)
//        {
//            byte frame = (byte)(isWater ? 32 : 0);
//            byte pause = (byte)(isWater ? 4 : 0);

//            ByteBuffer byteBuffer = new ByteBuffer();
//            byteBuffer.ArrayFloat(new float[] { v1.x, v2.y, v1.z, u2.x, u1.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.x * 255), (byte)(c.y * l.x * 255), (byte)(c.z * l.x * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });
//            byteBuffer.ArrayFloat(new float[] { v1.x, v2.y, v2.z, u2.x, u2.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.y * 255), (byte)(c.y * l.y * 255), (byte)(c.z * l.y * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });
//            byteBuffer.ArrayFloat(new float[] { v2.x, v2.y, v2.z, u1.x, u2.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.z * 255), (byte)(c.y * l.z * 255), (byte)(c.z * l.z * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });


//            byteBuffer.ArrayFloat(new float[] { v1.x, v2.y, v1.z, u2.x, u1.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.x * 255), (byte)(c.y * l.x * 255), (byte)(c.z * l.x * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });
//            byteBuffer.ArrayFloat(new float[] { v2.x, v2.y, v2.z, u1.x, u2.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.z * 255), (byte)(c.y * l.z * 255), (byte)(c.z * l.z * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });
//            byteBuffer.ArrayFloat(new float[] { v2.x, v2.y, v1.z, u1.x, u1.y });
//            byteBuffer.ArrayByte(new byte[] { (byte)(c.x * l.w * 255), (byte)(c.y * l.w * 255), (byte)(c.z * l.w * 255), 0 });
//            byteBuffer.ArrayByte(new byte[] { frame, pause, 0, 0 });

//            return byteBuffer.ToArray();
//        }


//        ///// <summary>
//        ///// Вверх
//        ///// </summary>
//        //public float[] Up()
//        //{
//        //    return new float[] {
//        //        v1.x, v2.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//        //        v1.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//        //        v2.x, v2.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//        //        v1.x, v2.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//        //        v2.x, v2.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,
//        //        v2.x, v2.y, v1.z, u1.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y
//        //    };
//        //}

//        /// <summary>
//        /// Низ
//        /// </summary>
//        public float[] Down()
//        {
//            return new float[] {
//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v1.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v1.x, v1.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v1.y, v1.z, u2.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y,
//                v2.x, v1.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y
//            };
//        }

//        /// <summary>
//        /// Восточная сторона
//        /// </summary>
//        public float[] East()
//        {
//            return new float[] {
//                v2.x, v1.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v2.y, v1.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v2.x, v2.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v2.x, v1.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v2.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,
//                v2.x, v1.y, v2.z, u1.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y
//            };
//        }

//        /// <summary>
//        /// Западная сторона
//        /// </summary>
//        public float[] West()
//        {
//            return new float[] {
//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v1.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v1.x, v2.y, v1.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v1.x, v1.y, v1.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v1.x, v1.y, v2.z, u2.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y,
//                v1.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y
//            };
//        }

//        /// <summary>
//        /// Южная сторона
//        /// </summary>
//        public float[] North()
//        {
//            return new float[] {
//                v1.x, v1.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v1.x, v2.y, v1.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v2.x, v2.y, v1.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v1.x, v1.y, v1.z, u2.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v2.y, v1.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,
//                v2.x, v1.y, v1.z, u1.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y
//            };
//        }

//        /// <summary>
//        /// Северная сторона
//        /// </summary>
//        public float[] South()
//        {
//            return new float[] {
//                v1.x, v1.y, v2.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y,
//                v1.x, v2.y, v2.z, u1.x, u2.y, c.x * l.z, c.y * l.z, c.z * l.z, leg.x, leg.y,

//                v1.x, v1.y, v2.z, u1.x, u1.y, c.x * l.x, c.y * l.x, c.z * l.x, leg.x, leg.y,
//                v2.x, v1.y, v2.z, u2.x, u1.y, c.x * l.w, c.y * l.w, c.z * l.w, leg.x, leg.y,
//                v2.x, v2.y, v2.z, u2.x, u2.y, c.x * l.y, c.y * l.y, c.z * l.y, leg.x, leg.y
//            };
//        }
//    }
//}
