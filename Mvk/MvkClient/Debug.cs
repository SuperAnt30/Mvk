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
        public static bool IsDraw { get; set; } = true;
        

        public static int DInt { get; set; } = 0;
        public static float DFloat { get; set; } = 0;
        public static string DStr { get; set; } = "";

        protected static string strTpsFps = "";
        public static void SetTpsFps(int fps, float speedFrame) => strTpsFps = string.Format("Speed: {0} fps {1:0.00} ms", fps, speedFrame);

        public static string strServer = "";

        protected static string ToStringDebug()
        {
            string v = strServer == "" ? "" : "Server " + strServer;
            return strTpsFps + "\r\n" 
                + v + "\r\n" 
                + "DInt: " + DInt.ToString() + "\r\n" 
                + "DFloat: " + DFloat.ToString() + "\r\n"
                + "DStr: " + DStr;
        }

        #region Draw

        private static uint dList;

        public static void RenderDebug()
        {
            if (IsDraw)
            {
                GLRender.ListDelete(dList);
                dList = GLRender.ListBegin();
                GLWindow.gl.MatrixMode(OpenGL.GL_PROJECTION);
                GLWindow.gl.LoadIdentity();
                GLWindow.gl.Ortho2D(0, GLWindow.WindowWidth, GLWindow.WindowHeight, 0);
                GLWindow.gl.MatrixMode(OpenGL.GL_MODELVIEW);
                GLWindow.gl.LoadIdentity();
                GLWindow.gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.Font12);
                FontRenderer.RenderText(11f, 11f, new vec4(.2f, .2f, .2f, 1f), ToStringDebug(), FontSize.Font12);
                FontRenderer.RenderText(10f, 10f, new vec4(0.9f, 0.9f, .6f, 1f), ToStringDebug(), FontSize.Font12);
                GLRender.ListEnd();
            }
        }

        public static void DrawDebug()
        {
            if (IsDraw) GLRender.ListCall(dList);
        }

        #endregion
    }
}
