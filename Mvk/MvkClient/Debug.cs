using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using MvkServer.Util;
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
        

        public static int DInt { get; set; } = 0;
        public static float DFloat { get; set; } = 0;
        public static string DStr { get; set; } = "";

        protected static string strTpsFps = "";
        public static void SetTpsFps(int fps, float speedFrame) => strTpsFps = string.Format("Speed: {0} fps {1:0.00} ms", fps, speedFrame);

        public static string strServer = "";
        public static string strClient = "";


        /// <summary>
        /// Прорисовать в 2д чанки 
        /// </summary>
        public static bool IsDrawServerChunk { get; set; } = true;

        public static DebugChunk ListChunks { get; set; } = new DebugChunk();

        protected static string ToStringDebug()
        {
            string s = strServer == "" ? "" : "Server " + strServer + "\r\n";
            string c = strClient == "" ? "" : "Client " + strClient + "\r\n";
            return strTpsFps + "\r\n" 
                + s + c 
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

                if (IsDrawServerChunk && strServer != "")
                {
                    // сетка карты
                    GLWindow.gl.Disable(OpenGL.GL_TEXTURE_2D);

                    // квадрат
                    int x = GLWindow.WindowWidth / 2;
                    int z = GLWindow.WindowHeight / 2;
                    vec4 colPr = new vec4(.13f, .44f, .91f, 1f);
                    int size = 8;

                    if (ListChunks.listChunkServer != null)
                    {
                        foreach (vec2i p in ListChunks.listChunkServer)
                        {
                            vec2i pos = p * size;
                            GLRender.Rectangle(pos.x + x, -pos.y + z, pos.x + x + size, -pos.y + z + size, colPr);
                        }
                    }
                    if (ListChunks.listChunkPlayers != null)
                    {
                        colPr = new vec4(.9f, .9f, .9f, 1f);
                        foreach (vec2i p in ListChunks.listChunkPlayers)
                        {
                            vec2i pos = p * size;
                            GLRender.Rectangle(pos.x + x + 1, -pos.y + z + 1, pos.x + x + size - 1, -pos.y + z + size - 1, colPr);
                        }
                    }
                    if (ListChunks.listChunkPlayer != null)
                    {
                        colPr = new vec4(.0f, .0f, .6f, 1f);
                        foreach (vec2i p in ListChunks.listChunkPlayer)
                        {
                            vec2i pos = p * size;
                            GLRender.Rectangle(pos.x + x + 2, -pos.y + z + 2, pos.x + x + size - 2, -pos.y + z + size - 2, colPr);
                        }
                    }
                }

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
