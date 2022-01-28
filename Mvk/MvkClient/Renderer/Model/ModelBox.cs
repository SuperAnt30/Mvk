using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Модель коробки
    /// </summary>
    public class ModelBox
    {
        /// <summary>
        /// Массив из 6 текстурных квадратов, по одному на каждую грань куба.
        /// </summary>
        public TexturedQuad[] Quads { get; protected set; } = new TexturedQuad[6];
        /// <summary>
        /// Координата наименьшей вершины
        /// </summary>
        public vec3 PosMin { get; protected set; }
        /// <summary>
        /// Координата наибольшей вершины
        /// </summary>
        public vec3 PosMax { get; protected set; }

        /// <summary>
        /// Создать коробку
        /// </summary>
        public ModelBox(vec2 textureSize, int u, int v, float x, float y, float z, int w, int h, int d, float scaleFactor, bool mirror)
        {
            PosMin = new vec3(x, y, z);
            PosMax = PosMin + new vec3(w, h, d);

            float xm = PosMax.x;
            float ym = PosMax.y;
            float zm = PosMax.z;

            x -= scaleFactor;
            y -= scaleFactor;
            z -= scaleFactor;
            xm += scaleFactor;
            ym += scaleFactor;
            zm += scaleFactor;

            if (mirror)
            {
                float x0 = xm;
                xm = x;
                x = x0;
            }

            vec3[] p = new vec3[8];

            p[0] = new vec3(x, y, z);
            p[1] = new vec3(xm, y, z);
            p[2] = new vec3(xm, ym, z);
            p[3] = new vec3(x, ym, z);
            p[4] = new vec3(x, y, zm);
            p[5] = new vec3(xm, y, zm);
            p[6] = new vec3(xm, ym, zm);
            p[7] = new vec3(x, ym, zm);

            Quads[0] = new TexturedQuad(new vec3[]
            { p[5], p[1], p[2], p[6] }, u + d + w, v + d, u + d + w + d, v + d + h, textureSize);
            Quads[1] = new TexturedQuad(new vec3[]
            { p[0], p[4], p[7], p[3] }, u, v + d, u + d, v + d + h, textureSize);
            // up (по картинке, но по факту снизу)
            Quads[2] = new TexturedQuad(new vec3[]
            { p[5], p[4], p[0], p[1] }, u + d, v, u + d + w, v + d, textureSize);
            // down (по картинке, но по факту сверху)
            Quads[3] = new TexturedQuad(new vec3[]
            { p[2], p[3], p[7], p[6] }, u + d + w, v + d, u + d + w + w, v, textureSize);
            Quads[4] = new TexturedQuad(new vec3[]
            { p[1], p[0], p[3], p[2] }, u + d, v + d, u + d + w, v + d + h, textureSize);
            Quads[5] = new TexturedQuad(new vec3[]
            { p[4], p[5], p[6], p[7] }, u + d + w + d, v + d, u + d + w + d + w, v + d + h, textureSize);

            if (mirror)
            {
                for (int i = 0; i < 6; i++)
                {
                    Quads[i].FlipFace();
                }
            }
        }

        public void Render(float scale)
        {
            for (int i = 0; i < 6; i++)
            {
                Quads[i].Render(scale);
            }
        }

        /// <summary>
        /// До рендера обрабатываем вращение
        ///       +-----+------+
        ///       |  2  |  3   |
        ///       | Top |Bottom|
        /// +-----+-----+------+----+
        /// |  1  |  4  |   0  | 5  |
        /// |Right|Front| Left |Back|
        /// +-----+-----+------+----+
        ///             7 +-----+ 6
        /// y ^  _  z    /     /|
        ///   |  /|   3 +-----+2|
        ///   | /       | 4   | + 5
        ///   |/        |     |/
        ///   +----> x  +-----+     
        ///            0       1 
        /// </summary>
    }
}
