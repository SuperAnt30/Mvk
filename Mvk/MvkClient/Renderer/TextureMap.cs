using MvkAssets;
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
        protected Hashtable items = new Hashtable();

        /// <summary>
        /// Первая инициализация текстур
        /// </summary>
        public void InitializeOne()
        {
            // Текстура названия
            SetTexture(new BufferedImage(AssetsTexture.Title, Assets.GetBitmap(AssetsTexture.Title)));
            // Текстура шрифта
            BufferedImage font = new BufferedImage(AssetsTexture.Font, Assets.GetBitmap(AssetsTexture.Font));
            SetTexture(font);
            FontAdvance.Initialize(font);
        }

        /// <summary>
        /// Остальная инициализация текстур по одному
        /// </summary>
        public void InitializeKey(BufferedImage buffered)
        {
            SetTexture(buffered);// new BufferedImage(Assets.GetBitmap(key)));
        }

        /// <summary>
        /// Получить индекс текстуры по ключу
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        protected uint GetData(AssetsTexture key) => items.ContainsKey(key) ? (uint)items[key] : 0;

        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        public void BindTexture(AssetsTexture key)
        {
            if (items.ContainsKey(key))
            {
                GLWindow.gl.BindTexture(OpenGL.GL_TEXTURE_2D, GetData(key));
            }
        }

        /// <summary>
        /// Внесение в кеш текстур
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        /// <param name="image">рисунок</param>
        protected uint SetTexture(BufferedImage image)
        {
            OpenGL gl = GLWindow.gl;

            uint[] texture = new uint[1];

            if (items.ContainsKey(image.Key))
            {
                texture[0] = (uint)items[image.Key];
            }
            else
            {
                gl.GenTextures(1, texture);
                items.Add(image.Key, texture[0]);
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
