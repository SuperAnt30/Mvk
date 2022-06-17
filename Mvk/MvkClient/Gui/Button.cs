using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    public class Button : Control
    {
        /// <summary>
        /// Ключи для нажатий кнопки и понимания их действий
        /// </summary>
        public EnumScreenKey ScreenKey { get; protected set; } = EnumScreenKey.None;

        public Button(string text) : base(text) { }
        public Button(EnumScreenKey key, string text) : base(text) => ScreenKey = key;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = Enabled ? enter ? 0.3125f : 0.15625f : 0f;
            float v2 = Enabled ? enter ? 0.46875f : 0.3125f : 0.15625f;
            int wh = Width / 2;
            float wh2 = Width / 256f;
            GLRender.Rectangle(0, 0, wh, Height, 0, v1, 0.5f * wh2, v2);
            GLRender.Rectangle(wh, 0, Width, Height, 1f - 0.5f * wh2, v1, 1f, v2);

            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            vec4 color = Enabled ? enter ? new vec4(1f, 1f, .5f, 1f) : new vec4(1f) : new vec4(.5f, .5f, .5f, 1f);
            FontRenderer.RenderString(GetXAlight(Text, 12), 14, color, Text, size);
        }


        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (enter)
                {
                    SampleClick();
                    OnClick();
                }
            }
        }
    }
}
