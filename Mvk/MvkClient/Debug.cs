using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;

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

        protected static string ToStringTpsFps()
        {
            return string.Format("Speed: {0} fps {1:0.00} mc {2} tps", Fps, SpeedFrame, Tps);
        }

        protected static string ToStringDebug()
        {
            return ToStringTpsFps() + "\r\nDInt: " + DInt.ToString();
        }

        private static uint dList;

        public static void RenderDebug()
        {
            if (IsDraw)
            {
                GLRender.ListDelete(dList);
                dList = FontRenderer.RenderText(10f, 10f, new vec4(.8f, .6f, .2f, 1f), ToStringDebug());
            }
        }

        public static void DrawDebug()
        {
            if (IsDraw) GLRender.ListCall(dList);
        }
    }
}
