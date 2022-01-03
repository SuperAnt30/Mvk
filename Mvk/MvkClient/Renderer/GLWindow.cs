using MvkClient.Gui;
using MvkClient.Renderer.Shaders;
using MvkClient.Util;
using MvkClient.World;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
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
        /// Объект шейдоров
        /// </summary>
        public static ShaderItems Shaders { get; protected set; } = new ShaderItems();

        /// <summary>
        /// Таймер для фиксации времени прорисовки кадра
        /// </summary>
        private static Stopwatch stopwatch = new Stopwatch();
        private static float speedFrameAll;
        private static long timerSecond;
        private static int fps;

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

            Shaders.Create(gl);
        }

        public static void Resized(int width, int height)
        {
            WindowWidth = width;
            WindowHeight = height;
        }

        /// <summary>
        /// Прорисовка каждого кадра
        /// </summary>
        /// <param name="screen">объект GUI</param>
        public static void Draw(Client client)
        {
            DrawBegin();
            // тут мир
            if (client.World != null) client.World.WorldRender.Draw();
            // тут gui
            client.Screen.DrawScreen();
            DrawEnd();
        }

        #region Draw

        /// <summary>
        /// Перед прорисовка каждого кадра OpenGL
        /// </summary>
        private static void DrawBegin()
        {
            fps++;
            stopwatch.Restart();
            Debug.CountMesh = 0;
            //gl.Perspective(70.0f, (float)windowWidth / (float)windowHeight, 0.1f, 512);

            // Включает Буфер глубины 
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            gl.Enable(OpenGL.GL_CULL_FACE);
            gl.PolygonMode(OpenGL.GL_FRONT_AND_BACK, OpenGL.GL_FILL);
        }

        /// <summary>
        /// После прорисовки каждого кадра OpenGL
        /// </summary>
        private static void DrawEnd()
        {
            // Перерасчёт кадров раз в секунду, и среднее время прорисовки кадра
            if (Client.Time() >= timerSecond + 1000)
            {
                Debug.SetTpsFps(fps, speedFrameAll / fps);
                timerSecond += 1000;
                speedFrameAll = 0;
                fps = 0;
            }
            Debug.DrawDebug();
            speedFrameAll += (float)stopwatch.ElapsedTicks / MvkStatic.TimerFrequency;
        }

        #endregion
    }
}
