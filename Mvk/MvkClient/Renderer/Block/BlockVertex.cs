namespace MvkClient.Renderer.Block
{
    /// <summary>
    /// Вершина блока
    /// </summary>
    public struct BlockVertex
    {
        /// <summary>
        /// Координата вершины
        /// </summary>
        private readonly float x, y, z;
        /// <summary>
        /// Смещение текстуры
        /// </summary>
        private readonly float u, v;
        /// <summary>
        /// Освещение от блока и неба
        /// </summary>
        private readonly byte light;
        /// <summary>
        /// Цвет
        /// </summary>
        private readonly byte r, g, b;

        public BlockVertex(float x, float y, float z, float u, float v, float r, float g, float b, byte light)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.u = u;
            this.v = v;
            this.light = light;
            this.r = (byte)(r * 255);
            this.g = (byte)(g * 255);
            this.b = (byte)(b * 255);
        }

        /// <summary>
        /// Вернуть массив позиции и смещения текстуры
        /// </summary>
        public float[] GetArrayPosUV() => new float[] { x, y, z, u, v };
        /// <summary>
        /// Вернуть массив цвета и освещения
        /// </summary>
        public byte[] GetArrayColorLight() => new byte[] { r, g, b, light };
    }
}
