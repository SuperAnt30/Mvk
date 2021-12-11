using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    public class Label : Control
    {
        public Label(string text, FontSize size) : base(text) => this.size = size;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1f, 1f, 1f, 1f);
            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            int ws = FontRenderer.WidthString(Text, size);
            vec4 color = Enabled ? new vec4(1f) : new vec4(.6f, .6f, .6f, 1f);
            FontRenderer.RenderString(Position.x + (Width - ws) / 2, Position.y + 14, color, Text, size);
        }

        /// <summary>
        /// Задать текст
        /// </summary>
        public void SetText(string text) => Text = text;
    }
}
