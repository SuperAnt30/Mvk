using MvkClient.Renderer;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    public class Control
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; protected set; }
        /// <summary>
        /// Высотаа
        /// </summary>
        public int Height { get; protected set; }
        /// <summary>
        /// Активный
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Видимый
        /// </summary>
        public bool Visible { get; set; } = true;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i Position { get; set; }
        /// <summary>
        /// фокус
        /// </summary>
        protected bool focus = false;

        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsRender { get; protected set; } = true;

        protected Screen screen;
        protected static OpenGL gl;

        public void Init(Screen screen)
        {
            this.screen = screen;
            gl = GLWindow.gl;
        }


        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void Resized() { }

        /// <summary>
        /// Прорисовка контрола
        /// </summary>
        public virtual void Draw() => IsRender = false;
        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public virtual void MouseMove(int x, int y)
        {
            bool b = Enabled && x >= Position.x && y >= Position.y 
                && x < Position.x + Width && y < Position.y + Height;
            if (focus != b)
            {
                focus = b;
                IsRender = true;
            }
        }

        public virtual void MouseClick(int x, int y)
        {
            MouseMove(x, y);
            if (focus)
            {
                OnClick();
            }
        }

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        protected virtual void OnClick() => Click?.Invoke(this, new EventArgs());
    }
}
