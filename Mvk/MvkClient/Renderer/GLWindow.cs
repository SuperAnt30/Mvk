using MvkAssets;
using MvkClient.Util;
using SharpGL;
using System.Diagnostics;

namespace MvkClient.Renderer
{
    /// <summary>
    /// OpenGL обращение с окном
    /// </summary>
    public class GLWindow
    {
        /// <summary>
        /// Объект OpenGL
        /// </summary>
        public static OpenGL gl { get; protected set; }
        /// <summary>
        /// Текстуры
        /// </summary>
        public static TextureMap Texture { get; protected set; }
        /// <summary>
        /// Ширина окна
        /// </summary>
        public static int WindowWidth { get; protected set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public static int WindowHeight { get; protected set; }

        /// <summary>
        /// Таймер для фиксации времени
        /// </summary>
        protected static Stopwatch stopwatch = new Stopwatch();

        /// <summary>
        /// Инициализировать, первый запуск OpenGL
        /// </summary>
        public static void Initialize(OpenGL gl)
        {
            GLWindow.gl = gl;
            GLRender.Initialize();
            stopwatch.Start();

            gl.ClearColor(0.3f, 0.3f, 0.3f, 1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);

            Texture = new TextureMap();
            Texture.InitializeOne();
        }

        public static void Resized(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
        }

        /// <summary>
        /// Перед прорисовка каждого кадра OpenGL
        /// </summary>
        public static void DrawBegin()
        {
            stopwatch.Restart();
            //gl.Perspective(70.0f, (float)windowWidth / (float)windowHeight, 0.1f, 512);

            // Включает Буфер глубины 
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        public static void DrawEnd()
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(0, WindowWidth, WindowHeight, 0);

            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            Texture.BindTexture(AssetsTexture.Font);
            Debug.RenderDebug();
            Debug.DrawDebug();
            Debug.SpeedFrame = (float)stopwatch.ElapsedTicks / Ticker.Frequency;
        }
    }
}
