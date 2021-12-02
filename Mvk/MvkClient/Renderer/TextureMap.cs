using MvkClient.Renderer.Font;
using MvkClient.Util;
using SharpGL;
using System.Collections;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект текстур
    /// </summary>
    public class TextureMap
    {
        /// <summary>
        /// Массив текстур uint
        /// </summary>
        protected Hashtable hashtable = new Hashtable();

        /// <summary>
        /// Индекст для текстуры 
        /// </summary>
        public void Initialize()
        {
            // Текстура шрифта
            BufferedImage font = new BufferedImage(@"textures\font\ascii4.png");
            SetTexture("font", font);
            FontAdvance.Initialize(font);
        }

        /// <summary>
        /// Получить индекс текстуры по ключу
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        public uint GetData(string key) => hashtable.ContainsKey(key) ? (uint)hashtable[key] : 0;

        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        public void BindTexture(string key)
        {
            if (hashtable.ContainsKey(key))
            {
                GLWindow.gl.BindTexture(OpenGL.GL_TEXTURE_2D, GetData(key));
            }
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        /// <param name="image">рисунок</param>
        public uint SetTexture(string key, BufferedImage image)
        {
            OpenGL gl = GLWindow.gl;

            uint[] texture = new uint[1];

            if (hashtable.ContainsKey(key))
            {
                texture[0] = (uint)hashtable[key];
            }
            else
            {
                gl.GenTextures(1, texture);
                hashtable.Add(key, texture[0]);
            }

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture[0]);
            gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 1);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, image.Width, image.Height,
                0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, image.Buffer);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            return texture[0];
        }
    }
}
