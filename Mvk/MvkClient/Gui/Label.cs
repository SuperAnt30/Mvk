using MvkAssets;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    public class Label : Control
    {
        /// <summary>
        /// Масшатб
        /// </summary>
        public float Scale { get; set; } = 1f;

        public Label(string text, FontSize size) : base(text) => this.size = size;

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();
            if (Text == "") return;

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.Color(1f, 1f, 1f, 1f);
            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            vec4 color = Enabled ? new vec4(1f) : new vec4(.6f, .6f, .6f, 1f);

            GLRender.PushMatrix();
            {
                int w2 = Width / 2;
                int h2 = Height / 2;
                GLRender.Translate(Position.x + w2, Position.y + h2, 0);
                GLRender.Scale(Scale);
                GLRender.Translate(-w2, -h2, 0);

                string[] stringSeparators = new string[] { "\r\n" };
                string[] strs = Text.Split(stringSeparators, StringSplitOptions.None);
                int h = 0;
                foreach (string str in strs)
                {
                    FontRenderer.RenderString(GetXAlight(str, 0), 14 + h, color, str, size);
                    h += FontAdvance.VertAdvance[(int)size] + 4;
                }
            }
            GLRender.PopMatrix();
        }

        /// <summary>
        /// Перенести текст согласно ширине контрола
        /// </summary>
        public void TransferText()
        {
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = Text.Split(stringSeparators, StringSplitOptions.None);

            List<string> symbols = new List<string>();
            foreach(string str in strs)
            {
                symbols.AddRange(str.Split(' '));
            }

            int wspase = FontRenderer.WidthString(" ", size);
            int w = 0;
            string text = "";
            foreach (string symbol in symbols)
            {
                int ws = FontRenderer.WidthString(symbol, size);
                if (w + wspase + ws > Width)
                {
                    w = ws;
                    text += "\r\n" + symbol;
                } else
                {
                    if (w > 0) text += " ";
                    text += symbol;
                    w += wspase + ws;
                }
            }
            Text = text;
        }
    }
}
