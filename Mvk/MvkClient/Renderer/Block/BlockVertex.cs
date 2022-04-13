using MvkServer.Glm;

namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Вершина блока
    /// </summary>
    public struct BlockVertex
    {
        private readonly vec3 pos;
        private readonly vec2 uv;
        private readonly byte light;
        private readonly vec3 color;

        private readonly byte r;
        private readonly byte g;
        private readonly byte b;

        public BlockVertex(vec3 pos, vec2 uv, vec3 color, byte light, float shadow)
        {
            this.pos = pos;
            this.uv = uv;
            this.color = color * shadow;
            this.light = light;
            r = (byte)(color.x * 255);
            g = (byte)(color.y * 255);
            b = (byte)(color.z * 255);
        }

        /// <summary>
        /// Координата вершины
        /// </summary>
        public vec3 GetPosition() => pos;
        /// <summary>
        /// Смещение текстуры
        /// </summary>
        public vec2 GetUV() => uv;
        /// <summary>
        /// Цвет
        /// </summary>
        public vec3 GetColor() => color;
        /// <summary>
        /// Освещение от блока и неба
        /// </summary>
        public byte GetLight() => light;
        /// <summary>
        /// Тень для ambient occlusion
        /// </summary>
       // public float GetShadow() => shadow;

        /// <summary>
        /// Вернуть массив позиции и смещения текстуры
        /// </summary>
        public float[] GetArrayPosUV() => new float[] { pos.x, pos.y, pos.z, uv.x, uv.y };
        /// <summary>
        /// Вернуть массив цвета и освещения
        /// </summary>
        public byte[] GetArrayColorLight() => new byte[] { r, g, b, light };
    }
}
