﻿using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;

namespace MvkClient.Gui
{
    public class Slider : Control
    {
        /// <summary>
        /// Значение
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// Минимальное значение
        /// </summary>
        public int Min { get; protected set; }
        /// <summary>
        /// Максимальное значение
        /// </summary>
        public int Max { get; protected set; }
        /// <summary>
        /// Шаг
        /// </summary>
        public int Step { get; protected set; }
        /// <summary>
        /// Текст
        /// </summary>
        protected string text;

        protected float GetIndex() => (float)(Value - Min) / (float)(Max - Min);
        protected int GetSize() => Max - Min;
        /// <summary>
        /// Зажата ли левая клавиша мыши
        /// </summary>
        protected bool IsLeftDown = false;

        public Slider(int min, int max, int step, string text) 
        {
            Width = 400;
            Height = 40;
            this.text = text;
            Min = min;
            Max = max;
            Step = step;
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
            float v1 = 0f;
            float v2 = 0.15625f;
            int w = Width;
            int wh = Width / 2;
            float wh2 = Width / 256f;
            int x = Position.x;
            GLRender.Rectangle(x, Position.y, x + wh, Position.y + Height, 0, v1, 0.5f * wh2, v2);
            GLRender.Rectangle(x + wh, Position.y, x + w, Position.y + Height, 1f - 0.5f * wh2, v1, 1f, v2);

            if (Enabled)
            {
                v1 = 0.15625f;
                v2 = 0.3125f;
                float index = GetIndex();
                int px = (int)((Width - 16) * index);
                w = 16;
                wh = 8;
                wh2 = w / 256f;
                x = Position.x + px;
                GLRender.Rectangle(x, Position.y, x + wh, Position.y + Height, 0, v1, 0.5f * wh2, v2);
                GLRender.Rectangle(x + wh, Position.y, x + w, Position.y + Height, 1f - 0.5f * wh2, v1, 1f, v2);
            }
            GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(size));
            string s = text + " " + Value;
            int ws = FontRenderer.WidthString(s, size);
            vec4 color = Enabled ? focus ? new vec4(.8f, .8f, .4f, 1f) : new vec4(.7f, .7f, .7f, 1f) : new vec4(.5f, .5f, .5f, 1f);
            FontRenderer.RenderString(Position.x + (Width - ws) / 2, Position.y + 14, color, s, size);
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public override void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                MouseMove(x, y);
                if (focus)
                {
                    IsLeftDown = true;
                    CheckMouse(x);
                }
            }
        }
        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public override void MouseUp(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left && IsLeftDown)
            {
                IsLeftDown = false;
                MouseMove(x, y);
                CheckMouse(x);
            }
        }
        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public override void MouseMove(int x, int y)
        {
            base.MouseMove(x, y);
            if (IsLeftDown) CheckMouse(x);
        }
        /// <summary>
        /// Проверка по активации мышки
        /// </summary>
        /// <param name="x">координата мыши по X</param>
        protected void CheckMouse(int x)
        {
            float xm = (float)(x - Position.x - 4) / (float)(Width - 8);
            if (xm < 0) xm = 0f;
            if (xm > 1f) xm = 1f;

            Value = (int)((Max - Min) * xm + Min) / Step;
            Value *= Step;

            IsRender = true;
        }
    }
}