using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Renderer
{
    /// <summary>
    /// OpenGL прорисовка
    /// </summary>
    public class GLRender
    {
        protected static OpenGL gl;
        public static void Initialize() => gl = GLWindow.gl;

        /***
         *  Ortho2D    TexCoord + Vertex        
         *                
         *   0,0         2       4
         *    +-----+     +-----+    
         *    |     |     | \   |
         *    |     |     |   \ |
         *    +-----+     +-----+
         *         1,1   1       3
         **/

        /// <summary>
        /// Прорисовка символа
        /// </summary>
        /// <param name="symbol">объект символа</param>
        public static void SymbolRender(Symbol symbol)
        {
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            gl.TexCoord(symbol.U1, symbol.V2);
            gl.Vertex(0, FontAdvance.VertAdvance);
            gl.TexCoord(symbol.U2, symbol.V2);
            gl.Vertex(FontAdvance.HoriAdvance, FontAdvance.VertAdvance);
            gl.TexCoord(symbol.U1, symbol.V1);
            gl.Vertex(0, 0);
            gl.TexCoord(symbol.U2, symbol.V1);
            gl.Vertex(FontAdvance.HoriAdvance, 0);
            gl.End();
        }

        /// <summary>
        /// Нарисовать прямоугольник, без текстуры
        /// </summary>
        public static void Rectangle(float x1, float y1, float x2, float y2, vec4 color)
        {
            gl.Color(color.x, color.y, color.z, color.w);
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            gl.Vertex(x1, y2);
            gl.Vertex(x2, y2);
            gl.Vertex(x1, y1);
            gl.Vertex(x2, y1);
            gl.End();
        }

        /// <summary>
        /// Нарисовать прямоугольник, c текстурой
        /// </summary>
        public static void Rectangle(float x1, float y1, float x2, float y2, float u1, float v1, float u2, float v2)
        {
            gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
            gl.TexCoord(u1, v2);
            gl.Vertex(x1, y2);
            gl.TexCoord(u2, v2);
            gl.Vertex(x2, y2);
            gl.TexCoord(u1, v1);
            gl.Vertex(x1, y1);
            gl.TexCoord(u2, v1);
            gl.Vertex(x2, y1);
            gl.End();
        }

        /// <summary>
        /// Сохранить в стек матрицы, придав цвет и перемещение
        /// </summary>
        /// <param name="color">цвет</param>
        /// <param name="trans">перемещение</param>
        public static void PushMatrix(vec4 color, vec3 trans)
        {
            gl.PushMatrix();
            gl.Color(color.x, color.y, color.z, 1f);
            gl.Translate(trans.x, trans.y, trans.z);
        }

        /// <summary>
        /// Восстановить ранее сохраненное состояние текущего стека матриц
        /// </summary>
        public static void PopMatrix() => gl.PopMatrix();

        /// <summary>
        /// Запуск листа
        /// </summary>
        public static uint ListBegin()
        {
            uint list = gl.GenLists(1);
            gl.NewList(list, OpenGL.GL_COMPILE);
            Debug.DInt = (int)list;
            return list;
        }

        /// <summary>
        /// Окончание листа
        /// </summary>
        public static void ListEnd() => gl.EndList();

        /// <summary>
        /// Вызвать лист
        /// </summary>
        public static void ListCall(uint list) => gl.CallList(list);

        public static void ListDelete(uint list) => gl.DeleteLists(list, 1);
        //gl.Begin(OpenGL.GL_TRIANGLES);
        //gl.TexCoord(x0, y0); gl.Vertex(0, 0, 0);
        //gl.TexCoord(x1, y1); gl.Vertex(s, s, 0);
        //gl.TexCoord(x0, y1); gl.Vertex(0, s, 0);

        //gl.TexCoord(x0, y0); gl.Vertex(0, 0, 0);
        //gl.TexCoord(x1, y0); gl.Vertex(s, 0, 0);
        //gl.TexCoord(x1, y1); gl.Vertex(s, s, 0);
        //gl.End();
        //gl.PopMatrix();
    }
}
