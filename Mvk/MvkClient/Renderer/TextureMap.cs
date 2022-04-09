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
            BufferedImage font8 = new BufferedImage(AssetsTexture.Font8, Assets.GetBitmap(AssetsTexture.Font8));
            BufferedImage font12 = new BufferedImage(AssetsTexture.Font12, Assets.GetBitmap(AssetsTexture.Font12));
            BufferedImage font16 = new BufferedImage(AssetsTexture.Font16, Assets.GetBitmap(AssetsTexture.Font16));
            SetTexture(font8);
            SetTexture(font12);
            SetTexture(font16);
            FontAdvance.Initialize(font8, font12, font16);
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
        public TextureStruct GetData(AssetsTexture key) => items.ContainsKey(key) ? (TextureStruct)items[key] : new TextureStruct();

        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        public void BindTexture(AssetsTexture key) => BindTexture(key, 0);
        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        public void BindTexture(AssetsTexture key, uint texture)
        {
            TextureStruct ts = GetData(key);
            if (!ts.IsEmpty()) BindTexture(ts.GetKey(), texture);
        }
        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        public void BindTexture(uint key) => BindTexture(key, 0);
        /// <summary>
        /// Запустить текстуру
        /// </summary>
        /// <param name="key">ключ текстуры</param>
        /// <param name="texture">OpenGL.GL_TEXTURE0 + texture</param>
        public void BindTexture(uint key, uint texture)
        {
            GLWindow.gl.ActiveTexture(OpenGL.GL_TEXTURE0 + texture);
            GLWindow.gl.BindTexture(OpenGL.GL_TEXTURE_2D, key);
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

            TextureStruct ts = GetData(image.Key);
            if (ts.IsEmpty())
            {
                gl.GenTextures(1, texture);
                ts = new TextureStruct(texture[0], image.Width, image.Height, image.Key);
                items.Add(image.Key, ts);
            } else
            {
                texture[0] = ts.GetKey();
            }
            //if (items.ContainsKey(image.Key))
            //{
            //    texture[0] = (uint)items[image.Key];
            //}
            //else
            //{
            //    gl.GenTextures(1, texture);
            //    items.Add(image.Key, texture[0]);
            //}

            gl.BindTexture(OpenGL.GL_TEXTURE_2D, texture[0]);
            gl.PixelStore(OpenGL.GL_UNPACK_ALIGNMENT, 1);
            gl.TexImage2D(OpenGL.GL_TEXTURE_2D, 0, OpenGL.GL_RGBA, image.Width, image.Height,
                0, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, image.Buffer);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_CLAMP_TO_BORDER);
            //gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_CLAMP_TO_BORDER);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_NEAREST);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_NEAREST);
            
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, 0);

            return texture[0];
        }
    }
}
