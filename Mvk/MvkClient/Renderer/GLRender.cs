using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Util;
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
        //public static void SymbolRender(Symbol symbol)
        //{
        //    gl.Begin(OpenGL.GL_TRIANGLE_STRIP);
        //    gl.TexCoord(symbol.U1, symbol.V2);
        //    gl.Vertex(0, FontAdvance.VertAdvance[symbol.Size]);
        //    gl.TexCoord(symbol.U2, symbol.V2);
        //    gl.Vertex(FontAdvance.HoriAdvance[symbol.Size], FontAdvance.VertAdvance[symbol.Size]);
        //    gl.TexCoord(symbol.U1, symbol.V1);
        //    gl.Vertex(0, 0);
        //    gl.TexCoord(symbol.U2, symbol.V1);
        //    gl.Vertex(FontAdvance.HoriAdvance[symbol.Size], 0);
        //    gl.End();
        //}

        /// <summary>
        /// Нарисовать прямоугольник, без текстуры
        /// </summary>
        public static void Rectangle(float x1, float y1, float x2, float y2, vec4 color)
        {
            Color(color);
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
        /// Добавить вершину с текстурой 3D
        /// </summary>
        public static void VertexWithUV(float x, float y, float z, float u, float v)
        {
            gl.TexCoord(u, v);
            gl.Vertex(x, y, z);
        }

        /// <summary>
        /// Добавить вершину
        /// </summary>
        public static void Vertex(float x, float y, float z) => gl.Vertex(x, y, z);
        public static void Vertex(vec3 pos) => gl.Vertex(pos.x, pos.y, pos.z);

        /// <summary>
        /// Запуск листа
        /// </summary>
        public static uint ListBegin()
        {
            uint list = gl.GenLists(1);
            gl.NewList(list, OpenGL.GL_COMPILE);
            //Debug.DInt = (int)list;
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
        /// <summary>
        /// Удалить лист
        /// </summary>
        public static void ListDelete(uint list) => gl.DeleteLists(list, 1);
        /// <summary>
        /// Видим обратную сторону полигона
        /// </summary>
        public static void CullDisable() => gl.Disable(OpenGL.GL_CULL_FACE);
        /// <summary>
        /// Видим только лицевую сторону полигона
        /// </summary>
        public static void CullEnable() => gl.Enable(OpenGL.GL_CULL_FACE);

        public static void PushMatrix() => gl.PushMatrix();
        public static void PopMatrix() => gl.PopMatrix();


        public static void Color(float r, float g, float b, float a) => gl.Color(r, g, b, a);
        public static void Color(vec4 color) => gl.Color(color.x, color.y, color.z, color.w);
        public static void Color(float r, float g, float b) => gl.Color(r, g, b);
        public static void Color(float rgb) => gl.Color(rgb, rgb, rgb);

        public static void Scale(float x, float y, float z) => gl.Scale(x, y, z);
        public static void Scale(float scale) => gl.Scale(scale, scale, scale);

        public static void Translate(float x, float y, float z) => gl.Translate(x, y, z);
        public static void Translate(vec3 pos) => gl.Translate(pos.x, pos.y, pos.z);

        public static void Rotate(float angle, float x, float y, float z) => gl.Rotate(angle, x, y, z);

        public static void Texture2DEnable() => gl.Enable(OpenGL.GL_TEXTURE_2D);
        public static void Texture2DDisable() => gl.Disable(OpenGL.GL_TEXTURE_2D);
        public static void LineWidth(float width) => gl.LineWidth(width);

        public static void Begin(uint mode) => gl.Begin(mode);
        public static void End() => gl.End();

        public static void DepthEnable() => gl.Enable(OpenGL.GL_DEPTH_TEST);
        public static void DepthDisable() => gl.Disable(OpenGL.GL_DEPTH_TEST);

        /// <summary>
        /// Нарисовать рамку, с заданным цветом и выбранной толщиной линии
        /// </summary>
        /// <param name="aabb">Объект рамки</param>
        /// <param name="color">цвет</param>
        /// <param name="depth">толщина линии</param>
        public static void DrawOutlinedBoundingBox(AxisAlignedBB aabb)
        {
            Begin(OpenGL.GL_LINE_STRIP);
            Vertex(aabb.Min.x, aabb.Min.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Min.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Min.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Min.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Min.y, aabb.Min.z);
            End();

            gl.Begin(OpenGL.GL_LINE_STRIP);
            Vertex(aabb.Min.x, aabb.Max.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Max.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Max.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Max.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Max.y, aabb.Min.z);
            End();

            Begin(OpenGL.GL_LINES);
            Vertex(aabb.Min.x, aabb.Min.y, aabb.Min.z);
            Vertex(aabb.Min.x, aabb.Max.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Min.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Max.y, aabb.Min.z);
            Vertex(aabb.Max.x, aabb.Min.y, aabb.Max.z);
            Vertex(aabb.Max.x, aabb.Max.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Min.y, aabb.Max.z);
            Vertex(aabb.Min.x, aabb.Max.y, aabb.Max.z);
            End();
        }
    }
}
