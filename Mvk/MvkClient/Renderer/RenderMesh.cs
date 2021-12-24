using MvkServer.Glm;
using System;
using System.Collections.Generic;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект создания сетки буфера, прорисовки и удаления
    /// </summary>
    public class RenderMesh : IDisposable
    {
        protected Mesh mesh;
        /// <summary>
        /// Буфер точки, точка xyz, текстура точки uv, цвет точки rgba
        /// </summary>
        protected virtual int[] attrs { get; } = new int[] { 3, 2, 4 };

        public int CountPoligon { get; protected set; } = 0;

        /// <summary>
        /// Сгенерировать
        /// </summary>
        public virtual void Render(List<float> buffer)
        {
            Render(buffer.ToArray());
        }

        /// <summary>
        /// Сгенерировать
        /// </summary>
        public virtual void Render(float[] buffer)
        {

            if (mesh != null)
            {
                mesh.Reload(buffer);
            }
            else
            {
                mesh = new Mesh(buffer, attrs);
            }
            CountPoligon = buffer.Length / mesh.PoligonFloat;
        }

        /// <summary>
        /// Прорисовать
        /// </summary>
        public void Draw()
        {
            if (mesh != null)
            {
                Debug.CountMesh++;
                mesh.Draw();
            }
        }

        /// <summary>
        /// Прорисовать
        /// </summary>
        public void DrawLine()
        {
            Debug.CountMesh++;
            mesh.DrawLine();
        }

        /// <summary>
        /// Удалить
        /// </summary>
        public void Delete()
        {
            CountPoligon = 0;
            if (mesh != null) mesh.Delete();
        }

        /// <summary>
        /// Удаление если объект удалиться сам
        /// </summary>
        public void Dispose() => Delete();

        #region Rectangle

        /// <summary>
        /// Нарисовать прямоугольник в 2д
        /// </summary>
        /// <param name="v1">угол левый вверх</param>
        /// <param name="v2">угол правый низ</param>
        /// <param name="z">глубина</param>
        /// <param name="u1">текстура угол 1</param>
        /// <param name="u2">текстура угол 2</param>
        /// <param name="c">цвет</param>
        /// <returns></returns>
        public static float[] Rectangle2d(vec2 v1, vec2 v2, float z, vec2 u1, vec2 u2, vec4 c)
        {
            return new float[]
            {
                v1.x, v1.y, z, u1.x, u1.y, c.x, c.y, c.z, c.w,
                v1.x, v2.y, z, u1.x, u2.y, c.x, c.y, c.z, c.w,
                v2.x, v1.y, z, u2.x, u1.y, c.x, c.y, c.z, c.w,

                v1.x, v2.y, z, u1.x, u2.y, c.x, c.y, c.z, c.w,
                v2.x, v2.y, z, u2.x, u2.y, c.x, c.y, c.z, c.w,
                v2.x, v1.y, z, u2.x, u1.y, c.x, c.y, c.z, c.w
            };
        }

        /// <summary>
        /// Нарисовать прямоугольник в 2д
        /// </summary>
        /// <param name="v1">угол левый вверх</param>
        /// <param name="v2">угол правый низ</param>
        /// <param name="u1">текстура угол 1</param>
        /// <param name="u2">текстура угол 2</param>
        /// <param name="c">цвет</param>
        /// <returns></returns>
        public static float[] Rectangle2d(vec2 v1, vec2 v2, vec2 u1, vec2 u2, vec4 c)
        {
            return Rectangle2d(v1, v2, 0, u1, u2, c);
        }

        #endregion
    }
}
