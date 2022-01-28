using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Renderer.Model
{
    /// <summary>
    /// Объект стороны
    /// </summary>
    public class TexturedQuad
    {
        /// <summary>
        /// Позиции вершин (x, y, z) и координаты текстуры (u, v) для каждой из 4 точек на стороне
        /// </summary>
        public TextureVertex[] Vertices { get; protected set; } = new TextureVertex[4];

        public TexturedQuad(vec3[] pos)
        {
            for (int i = 0; i < 4; i++)
            {
                Vertices[i] = new TextureVertex(pos[i]);
            }
        }

        public TexturedQuad(vec3[] pos, int u1, int v1, int u2, int v2, vec2 textureSize) : this(pos)
        {
            Vertices[0] = Vertices[0].SetTexturePosition((float)u2 / textureSize.x, (float)v1 / textureSize.y);
            Vertices[1] = Vertices[1].SetTexturePosition((float)u1 / textureSize.x, (float)v1 / textureSize.y);
            Vertices[2] = Vertices[2].SetTexturePosition((float)u1 / textureSize.x, (float)v2 / textureSize.y);
            Vertices[3] = Vertices[3].SetTexturePosition((float)u2 / textureSize.x, (float)v2 / textureSize.y);
        }

        /// <summary>
        /// Рендер стороны
        /// </summary>
        /// <param name="scale">Масштаб</param>
        public void Render(float scale)
        {
            GLWindow.gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            VerticeRender(1, scale);
            VerticeRender(2, scale);
            VerticeRender(0, scale);
            VerticeRender(3, scale);
            GLWindow.gl.End();
        }

        /// <summary>
        /// Рендер одной вершины
        /// </summary>
        /// <param name="index">индекс вершины</param>
        /// <param name="scale">масштаб</param>
        private void VerticeRender(int index, float scale)
        {
            TextureVertex vertex = Vertices[index];
            vec3 pos = vertex.GetPosition();
            vec2 texture = vertex.GetTexture();
            GLRender.VertexWithUV(pos.x * scale, pos.y * scale, pos.z * scale, texture.x, texture.y);
        }

        /// <summary>
        /// Перевернуть лицо
        /// </summary>
        public void FlipFace()
        {
            TextureVertex[] vertices = new TextureVertex[4];
            for (int i = 0; i < 4; i++)
            {
                vertices[i] = Vertices[3 - i];
            }
            Vertices = vertices;
        }
    }
}
