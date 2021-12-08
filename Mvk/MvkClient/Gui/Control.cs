using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Setitings;
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
        public int Width { get; set; }
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
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// фокус
        /// </summary>
        protected bool focus = false;
        /// <summary>
        /// Размер шрифта
        /// </summary>
        protected FontSize size = FontSize.Font12;

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
            bool b = Enabled && x >= Position.x && y >= Position.y && x < Position.x + Width && y < Position.y + Height;
            if (focus != b)
            {
                focus = b;
                IsRender = true;
            }
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public virtual void MouseDown(MouseButton button, int x, int y) { }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public virtual void MouseUp(MouseButton button, int x, int y) { }

        /// <summary>
        /// Звук клика
        /// </summary>
        protected void SampleClick() => screen.ClientMain.Sample.PlaySound(AssetsSample.Click, .3f * Setting.ToFloatSoundVolume());

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        protected virtual void OnClick() => Click?.Invoke(this, new EventArgs());
    }
}
