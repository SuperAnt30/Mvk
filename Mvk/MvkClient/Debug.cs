using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient
{
    /// <summary>
    /// Класс для отладки, и видеть на экране
    /// </summary>
    public class Debug
    {
        /// <summary>
        /// Выводить ли на экран
        /// </summary>
        public static bool IsDraw { get; set; } = false;
        /// <summary>
        /// Кадры в секунду
        /// </summary>
        public static float Fps { get; set; } = 0;
        /// <summary>
        /// Скорость прорисовки кадра в милисекундах
        /// </summary>
        public static float SpeedFrame { get; set; } = 0;
        /// <summary>
        /// Количество тактов в секунду
        /// </summary>
        public static float Tps { get; set; } = 0;

        public static int DInt { get; set; } = 0;
        public static float DFloat { get; set; } = 0;

        protected static string ToStringTpsFps()
        {
            return string.Format("Speed: {0} fps {1:0.00} mc {2} tps", Fps, SpeedFrame, Tps);
        }

        protected static string ToStringDebug()
        {
            return ToStringTpsFps() + "\r\nDInt: " + DInt.ToString() + "\r\nDFloat: " + DFloat.ToString();
        }

        private static uint dList;

        public static void RenderDebug()
        {
            if (IsDraw)
            {
                GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.Font12);
                GLRender.ListDelete(dList);
                dList = GLRender.ListBegin();
                //FontRenderer.RenderText(11f, 11f, new vec4(.2f, .2f, .2f, 1f), ToStringDebug(), FontSize.Font12);
                FontRenderer.RenderText(10f, 10f, new vec4(0.9f, 0.9f, .6f, 1f), ToStringDebug(), FontSize.Font12);
                GLRender.ListEnd();
            }
        }

        public static void DrawDebug()
        {
            if (IsDraw) GLRender.ListCall(dList);
        }
    }
}
