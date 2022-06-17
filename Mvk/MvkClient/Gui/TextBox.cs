using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    public class TextBox : Control
    {
        /// <summary>
        /// Счётчик для анимации
        /// </summary>
        protected int cursorCounter;
        /// <summary>
        /// Максимальная длинна 
        /// </summary>
        protected int limit = 24;
        /// <summary>
        /// Видимость курсора
        /// </summary>
        protected bool isVisibleCursor;

        public TextBox(string text) : base(text) { }

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public override void Draw()
        {
            base.Draw();

            gl.Enable(OpenGL.GL_TEXTURE_2D);
            GLWindow.Texture.BindTexture(AssetsTexture.Widgets);
            gl.Color(1f, 1f, 1f, 1f);
            float v1 = 0f;
            float v2 = 0.15625f;
            int wh = Width / 2;
            float wh2 = Width / 256f;
            GLRender.Rectangle(0, 0, wh, Height, 0, v1, 0.5f * wh2, v2);
            GLRender.Rectangle(wh, 0, Width, Height, 1f - 0.5f * wh2, v1, 1f, v2);

            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));

            vec4 color0 = new vec4(.3f, .3f, .3f, 1f);
            if (Focus) FontRenderer.RenderString(13, 15, color0, Text, size);
            vec4 color = Enabled ? enter ? new vec4(1f, 1f, .5f, 1f) : new vec4(1f) : new vec4(.5f, .5f, .5f, 1f);
            FontRenderer.RenderString(12, 14, color, Text, size);

            if (isVisibleCursor)
            {
                int ws = FontRenderer.WidthString(Text, size);
                color = new vec4(1f);
                FontRenderer.RenderString(ws + 13, 15, color0, "_", size);
                FontRenderer.RenderString(ws + 12, 14, color, "_", size);
            }
        }

        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (enter && !Focus)
                {
                    // Задать фокус
                    Focus = true;
                }
                else if(!enter && Focus) 
                {
                    // Потерять фокус
                    isVisibleCursor = false;
                    Focus = false;
                    IsRender = true;
                }
            }
        }

        public override void KeyPress(char key)
        {
            int id = Convert.ToInt32(key);
            if (id == 8)
            {
                // back
                if (Text.Length > 0)
                {
                    Text = Text.Substring(0, Text.Length - 1);
                    IsRender = true;
                }
            }
            else if (Text.Length < limit && ((id >= 48 && id <= 57) // цифры
                || (id >= 65 && id <= 90) // Большие
                || (id >= 97 && id <= 122) // Маленькие
                || id == 46 || id == 58)) // точка и двое точие
            {
                Text += key;
                IsRender = true;
            }
        }

        
        /// <summary>
        /// Увеличивает счетчик курсора 
        /// </summary>
        public void UpdateCursorCounter()
        {
            cursorCounter++;
            if (Focus && cursorCounter / 6 % 2 == 0)
            {
                if (!isVisibleCursor)
                {
                    isVisibleCursor = true;
                    IsRender = true;
                }
            } else
            {
                if (isVisibleCursor)
                {
                    isVisibleCursor = false;
                    IsRender = true;
                }
            }
        }
    }
}
