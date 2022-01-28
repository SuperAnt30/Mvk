using MvkServer.Util;

namespace MvkClient.Util
{
    /// <summary>
    /// Frustum Culling
    /// </summary>
    public class Frustum
    {
        protected float[,] frustum = new float[6, 4];

        /// <summary>
        /// Иницилизация данных
        /// </summary>
        /// <param name="la">матрица lookAt GL_MODELVIEW_MATRIX</param>
        /// <param name="p">матрица projection GL_PROJECTION_MATRIX</param>
        public void Init(float[] la, float[] p)
        {
            float[] clip = new float[]
            {
                la[0] * p[0] + la[1] * p[4] + la[2] * p[8] + la[3] * p[12],
                la[0] * p[1] + la[1] * p[5] + la[2] * p[9] + la[3] * p[13],
                la[0] * p[2] + la[1] * p[6] + la[2] * p[10] + la[3] * p[14],
                la[0] * p[3] + la[1] * p[7] + la[2] * p[11] + la[3] * p[15],
                la[4] * p[0] + la[5] * p[4] + la[6] * p[8] + la[7] * p[12],
                la[4] * p[1] + la[5] * p[5] + la[6] * p[9] + la[7] * p[13],
                la[4] * p[2] + la[5] * p[6] + la[6] * p[10] + la[7] * p[14],
                la[4] * p[3] + la[5] * p[7] + la[6] * p[11] + la[7] * p[15],
                la[8] * p[0] + la[9] * p[4] + la[10] * p[8] + la[11] * p[12],
                la[8] * p[1] + la[9] * p[5] + la[10] * p[9] + la[11] * p[13],
                la[8] * p[2] + la[9] * p[6] + la[10] * p[10] + la[11] * p[14],
                la[8] * p[3] + la[9] * p[7] + la[10] * p[11] + la[11] * p[15],
                la[12] * p[0] + la[13] * p[4] + la[14] * p[8] + la[15] * p[12],
                la[12] * p[1] + la[13] * p[5] + la[14] * p[9] + la[15] * p[13],
                la[12] * p[2] + la[13] * p[6] + la[14] * p[10] + la[15] * p[14],
                la[12] * p[3] + la[13] * p[7] + la[14] * p[11] + la[15] * p[15]
            };

            frustum = new float[,]
            {
                { clip[3] - clip[0], clip[7] - clip[4], clip[11] - clip[8], clip[15] - clip[12] },
                { clip[3] + clip[0], clip[7] + clip[4], clip[11] + clip[8], clip[15] + clip[12] },
                { clip[3] + clip[1], clip[7] + clip[5], clip[11] + clip[9], clip[15] + clip[13] },
                { clip[3] - clip[1], clip[7] - clip[5], clip[11] - clip[9], clip[15] - clip[13] },
                { clip[3] - clip[2], clip[7] - clip[6], clip[11] - clip[10], clip[15] - clip[14] },
                { clip[3] + clip[2], clip[7] + clip[6], clip[11] + clip[10], clip[15] + clip[14] }
            };
            for (int i = 0; i < 6; i++)
            {
                Divide(i);
            }
        }

        private void Divide(int index)
        {
            float f = Mth.Sqrt(frustum[index, 0] * frustum[index, 0] + frustum[index, 1] * frustum[index, 1]
                + frustum[index, 2] * frustum[index, 2]);
            frustum[index, 0] /= f;
            frustum[index, 1] /= f;
            frustum[index, 2] /= f;
            frustum[index, 3] /= f;
        }

        private float Multiply(int index, float x, float y, float z)
        {
            return frustum[index, 0] * x + frustum[index, 1] * y + frustum[index, 2] * z + frustum[index, 3];
        }

        /// <summary>
        /// Возвращает true, если прямоугольник находится внутри всех 6 плоскостей отсечения,
        /// в противном случае возвращает false.
        /// </summary>
        public bool IsBoxInFrustum(float x1, float y1, float z1, float x2, float y2, float z2)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Multiply(i, x1, y1, z1) <= 0f
                    && Multiply(i, x2, y1, z1) <= 0f
                    && Multiply(i, x1, y2, z1) <= 0f
                    && Multiply(i, x2, y2, z1) <= 0f
                    && Multiply(i, x1, y1, z2) <= 0f
                    && Multiply(i, x2, y1, z2) <= 0f
                    && Multiply(i, x1, y2, z2) <= 0f
                    && Multiply(i, x2, y2, z2) <= 0f)
                {
                    return false;
                }
            }

            return true;
        }
        /// <summary>
        /// Возвращает true, если прямоугольник находится внутри всех 6 плоскостей отсечения,
        /// в противном случае возвращает false.
        /// </summary>
        public bool IsBoxInFrustum(AxisAlignedBB aabb) 
            => IsBoxInFrustum(aabb.Min.x, aabb.Min.y, aabb.Min.z, aabb.Max.x, aabb.Max.y, aabb.Max.z);
    }
}
