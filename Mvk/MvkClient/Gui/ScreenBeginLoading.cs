using MvkAssets;
using MvkClient.Renderer;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    /// <summary>
    /// Стартовая загрузка
    /// </summary>
    public class ScreenBeginLoading : ScreenLoading
    {
        public ScreenBeginLoading(Client client) : base(client)
        {
            background = EnumBackground.Loading;
        }

        /// <summary>
        /// Генерация фона
        /// </summary>
        protected override void Render()
        {
            // фон
            RenderBackground();

            vec4 colBg = new vec4(1f, 1f, 1f, 1f);
            vec4 colPr = new vec4(.13f, .44f, .91f, 1f);
            // название
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Title);
            gl.Color(1f, 1f, 1f, 1f);
            int wh = Width / 2;
            GLRender.Rectangle(wh - 256, 128, wh + 256, 384, 0, 0.75f, 0.25f, 0.875f);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            // процесс
            GLRender.Rectangle(wh - 208, Height - 140, wh + 208, Height - 100, colPr);
            GLRender.Rectangle(wh - 204, Height - 136, wh + 204, Height - 104, colBg);
            int wcl = value * 400 / max;
            GLRender.Rectangle(wh - 200, Height - 132, wh - 200 + wcl, Height - 108, colPr);
        }
    }
}
