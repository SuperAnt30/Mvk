using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Тест с графикой
    /// </summary>
    public class GLTest
    {
        public static void Test()
        {
            OpenGL gl = GLWindow.gl;

           // gl.AlphaFunc(OpenGL.GL_GREATER, 0.1f);
            //gl.Enable(OpenGL.GL_ALPHA_TEST);
            //gl.Enable(OpenGL.GL_MULTISAMPLE);

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_BLEND);
            //  gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            //gl.Enable(OpenGL.GL_MULTISAMPLE);

           // FontRenderer.Draw(FontRenderer.RenderString(500f, 100f, new vec4(1, 1, 0, 1), "Тестируем\r\nTesting"));
           

            gl.Disable(OpenGL.GL_TEXTURE_2D);

            uint index = gl.GenLists(1);
            gl.NewList(index, OpenGL.GL_COMPILE);

            //gl.Begin(OpenGL.GL_TRIANGLES);
            //gl.Color(1.0f, 1.0f, 1.0f, 0.5f);
            //gl.Vertex(1f, 1f, 0);
            //gl.Color(1.0f, 0f, 1.0f, 0.5f);
            //gl.Vertex(0, 0, 0);
            //gl.Color(0f, 1.0f, 1.0f, 0.5f);
            //gl.Vertex(1f, 0, 0);
            //gl.End();

            //gl.Begin(OpenGL.GL_QUADS);
            //gl.Color(1.0f, 1.0f, 1.0f, 0.5f);
            //gl.Vertex(1f, 1f, 0);
            //gl.Vertex(0, 1f, 0);
            //gl.Color(1.0f, 0f, 1.0f, 0.5f);
            //gl.Vertex(0, 0, 0);
            //gl.Color(0f, 1.0f, 1.0f, 0.5f);
            //gl.Vertex(1f, 0, 0);

            //gl.End();

            gl.LineWidth(3.0f);
            gl.Begin(OpenGL.GL_LINES);
            gl.Color(1.0f, 0, 0);
            gl.Vertex(100f, 500f, 0);
            gl.Vertex(0, 0, 0);
            gl.End();

            gl.EndList();

            gl.CallList(index);

            
        }
    }
}
