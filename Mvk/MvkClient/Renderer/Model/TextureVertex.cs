using MvkServer.Glm;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Объект вершины с текстурой
    /// </summary>
    public struct TextureVertex
    {
        /// <summary>
        /// Координаты точки
        /// </summary>
        private vec3 position;
        /// <summary>
        /// Координаты текстуры
        /// </summary>
        private vec2 texture;

        public TextureVertex(vec3 pos, vec2 texture)
        {
            position = pos;
            this.texture = texture;
        }

        public TextureVertex(vec3 pos) : this(pos, new vec2(0)) { }

        public TextureVertex(float x, float y, float z, float u, float v)
        {
            position = new vec3(x, y, z);
            texture = new vec2(u, v);
        }

        public TextureVertex(TextureVertex textureVertex, float u, float v)
        {
            position = textureVertex.GetPosition();
            texture = new vec2(u, v);
        }

        public TextureVertex SetTexturePosition(float u, float v) => new TextureVertex(this, u, v);

        /// <summary>
        /// Получить координаты позицию
        /// </summary>
        public vec3 GetPosition() => position;
        /// <summary>
        /// Получить координаты текстуры
        /// </summary>
        public vec2 GetTexture() => texture;
    }
}
