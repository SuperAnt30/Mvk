using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    public class Button : Control
    {
        
        protected string text;

        public Button(int x, int y, string text)
        {
            Position = new vec2i(x, y);
            Width = 400;
            Height = 40;
            this.text = text;
        }

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = Enabled ? focus ? 0.3359375f : 0.2578125f : 0.1796875f;
            float v2 = Enabled ? focus ? 0.4140625f : 0.3359375f : 0.2578125f;
            GLRender.Rectangle(Position.x, Position.y, Position.x + Width, Position.y + Height, 0, v1, 0.78125f, v2);
            GLWindow.Texture.BindTexture(AssetsTexture.Font);
            int ws = FontRenderer.WidthString(text);
            vec4 color = Enabled ? new vec4(1f) : new vec4(.6f, .6f, .6f, 1f);
            FontRenderer.RenderString(Position.x + (Width - ws) / 2, Position.y + 14, color, text);
        }


        public override void MouseClick(int x, int y)
        {
            base.MouseClick(x, y);
            if (focus) screen.ClientMain.TestSound();
        }
    }
}
