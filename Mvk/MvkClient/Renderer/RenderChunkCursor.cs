using MvkServer.Glm;
using MvkServer.Util;
using SharpGL;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Рендер курсора границы чанков
    /// </summary>
    public class RenderChunkCursor : RenderDL
    {
        public void Render(vec3 offset)
        {
            SetRotationPoint(
                offset.x * -1f + ((Mth.Floor(offset.x) >> 4) << 4),
                offset.y * -1f,
                offset.z * -1f + ((Mth.Floor(offset.z) >> 4) << 4)
            );
            Render();
        }

        protected override void DoRender()
        {
            vec4 colorBlue = new vec4(.3f, .4f, 1f, .8f);
            vec4 colorYelow = new vec4(1, 1, .5f, .8f);
            vec4 colorRed = new vec4(1, 0, 0, .8f);
            int height = 256;

            GLRender.PushMatrix();
            {
                GLRender.Texture2DDisable();
                GLRender.LineWidth(1f);
                GLRender.PushMatrix();
                {
                    // Кольца с низу вверх
                    for (int y = 0; y <= height; y += 2)
                    {
                        GLRender.Color(y % 16 == 0 ? colorBlue : colorYelow);
                        GLRender.Begin(OpenGL.GL_LINE_STRIP);
                        GLRender.Vertex(0, y, 0);
                        GLRender.Vertex(16, y, 0);
                        GLRender.Vertex(16, y, 16);
                        GLRender.Vertex(0, y, 16);
                        GLRender.Vertex(0, y, 0);
                        GLRender.End();
                    }
                    GLRender.Begin(OpenGL.GL_LINES);
                    // вертикальные линии границы чанка
                    for (int x = 0; x <= 16; x += 2)
                    {
                        GLRender.Color(x % 16 == 0 ? colorBlue : colorYelow);
                        GLRender.Vertex(x, 0, 0);
                        GLRender.Vertex(x, height, 0);
                        GLRender.Vertex(x, 0, 16);
                        GLRender.Vertex(x, height, 16);
                    }
                    GLRender.Color(colorYelow);
                    for (int z = 2; z < 16; z += 2)
                    {
                        GLRender.Vertex(0, 0, z);
                        GLRender.Vertex(0, height, z);
                        GLRender.Vertex(16, 0, z);
                        GLRender.Vertex(16, height, z);
                    }
                    // вертикальные чанки углов соседнего чанка
                    GLRender.Color(colorRed);
                    for (int x = -16; x <= 32; x += 16)
                    {
                        GLRender.Vertex(x, 0, -16);
                        GLRender.Vertex(x, height, -16);
                        GLRender.Vertex(x, 0, 32);
                        GLRender.Vertex(x, height, 32);
                    }
                    for (int z = 0; z <= 16; z += 16)
                    {
                        GLRender.Vertex(-16, 0, z);
                        GLRender.Vertex(-16, height, z);
                        GLRender.Vertex(32, 0, z);
                        GLRender.Vertex(32, height, z);
                    }
                    GLRender.End();
                }
                GLRender.PopMatrix();
            }
            GLRender.PopMatrix();
        }
    }
}
