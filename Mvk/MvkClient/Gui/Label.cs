using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    public class Label : Control
    {
        /// <summary>
        /// Текст
        /// </summary>
        protected string text;
        
        public Label(string text, FontSize size)
        {
            Width = 400;
            Height = 40;
            this.size = size;
            this.text = text;
        }

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1f, 1f, 1f, 1f);
            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            int ws = FontRenderer.WidthString(text, size);
            vec4 color = Enabled ? new vec4(1f) : new vec4(.6f, .6f, .6f, 1f);
            FontRenderer.RenderString(Position.x + (Width - ws) / 2, Position.y + 14, color, text, size);
        }

        /// <summary>
        /// Задать текст
        /// </summary>
        public void SetText(string text) => this.text = text;
    }
}
