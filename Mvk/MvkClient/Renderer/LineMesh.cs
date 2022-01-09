using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Прорисовка линий
    /// </summary>
    public class LineMesh : MeshBuffer
    {
        /// <summary>
        /// Буфер точки, точка xyz и цвет точки rgba
        /// </summary>
        protected override int[] attrs { get; } = new int[] { 3, 4 };

        /// <summary>
        /// Ключь сетки линии
        /// </summary>
        //public string Key { get; protected set; }

        //public LineMesh(string key) => Key = key;


        public float[] Line(float x1, float y1, float z1, float x2, float y2, float z2,
        float r, float g, float b, float a)
        {
            return new float[]
            {
                x1, y1, z1, r, g, b, a,
                x2, y2, z2, r, g, b, a
            };
        }

        public float[] Line(float x1, float y1, float z1, float x2, float y2, float z2, vec4 rgba)
        {
            return new float[]
            {
                x1, y1, z1, rgba.x, rgba.y, rgba.z, rgba.w,
                x2, y2, z2, rgba.x, rgba.y, rgba.z, rgba.w
            };
        }
        public float[] Box(float x, float y, float z, float w, float h, float d, vec4 rgba) 
            => Box(x, y, z, w, h, d, rgba.x, rgba.y, rgba.z, rgba.w);

        public float[] Box(float x, float y, float z, float w, float h, float d, float r, float g, float b, float a)
        {
            w *= 0.5f;
            h *= 0.5f;
            d *= 0.5f;
            List<float> buffer = new List<float>();

            buffer.AddRange(Line(x - w, y - h, z - d, x + w, y - h, z - d, r, g, b, a));
            buffer.AddRange(Line(x - w, y + h, z - d, x + w, y + h, z - d, r, g, b, a));
            buffer.AddRange(Line(x - w, y - h, z + d, x + w, y - h, z + d, r, g, b, a));
            buffer.AddRange(Line(x - w, y + h, z + d, x + w, y + h, z + d, r, g, b, a));

            buffer.AddRange(Line(x - w, y - h, z - d, x - w, y + h, z - d, r, g, b, a));
            buffer.AddRange(Line(x + w, y - h, z - d, x + w, y + h, z - d, r, g, b, a));
            buffer.AddRange(Line(x - w, y - h, z + d, x - w, y + h, z + d, r, g, b, a));
            buffer.AddRange(Line(x + w, y - h, z + d, x + w, y + h, z + d, r, g, b, a));

            buffer.AddRange(Line(x - w, y - h, z - d, x - w, y - h, z + d, r, g, b, a));
            buffer.AddRange(Line(x + w, y - h, z - d, x + w, y - h, z + d, r, g, b, a));
            buffer.AddRange(Line(x - w, y + h, z - d, x - w, y + h, z + d, r, g, b, a));
            buffer.AddRange(Line(x + w, y + h, z - d, x + w, y + h, z + d, r, g, b, a));

            return buffer.ToArray();
        }
    }
}
