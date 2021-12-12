using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkClient.Renderer.Font;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient.Gui
{
    public class Control
    {
        /// <summary>
        /// Нужен ли рендер
        /// </summary>
        public bool IsRender { get; protected set; } = true;
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
        /// Выравнивания
        /// </summary>
        public EnumAlight Alight { get; set; } = EnumAlight.Center;
        /// <summary>
        /// Позиция
        /// </summary>
        public vec2i Position { get; set; }
        /// <summary>
        /// Дополнительный объект
        /// </summary>
        public object Tag { get; set; }
        /// <summary>
        /// Фокус
        /// </summary>
        public bool Focus { get; protected set; } = false;
        /// <summary>
        /// Текст
        /// </summary>
        public string Text { get; protected set; }
        /// <summary>
        /// Когда мышь находится на элементе
        /// </summary>
        protected bool enter = false;
        /// <summary>
        /// Размер шрифта
        /// </summary>
        protected FontSize size = FontSize.Font12;

        protected Screen screen;
        protected static OpenGL gl;

        protected Control() { }
        public Control(string text)
        {
            Width = 400;
            Height = 40;
            Text = text;
        }

        public void Init(Screen screen)
        {
            this.screen = screen;
            gl = GLWindow.gl;
        }

        /// <summary>
        /// Задать текст
        /// </summary>
        public void SetText(string text)
        {
            Text = text;
            IsRender = true;
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
        /// В облости ли мышь курсора
        /// </summary>
        protected bool IsRectangleMouse(int x, int y) 
            => Enabled && x >= Position.x && y >= Position.y && x < Position.x + Width && y < Position.y + Height;

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public virtual void MouseMove(int x, int y)
        {
            bool b = IsRectangleMouse(x, y);
            if (enter != b)
            {
                enter = b;
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
        /// Нажата клавиша в char формате
        /// </summary>
        public virtual void KeyPress(char key) { }

        /// <summary>
        /// Звук клика
        /// </summary>
        protected void SampleClick() => screen.ClientMain.Sample.PlaySound(AssetsSample.Click, .3f);

        /// <summary>
        /// Получить x в зависимости от смещения
        /// </summary>
        /// <param name="text">Текст</param>
        /// <param name="margin">Отступ</param>
        protected int GetXAlight(string text, int margin)
        {
            int x = margin;
            if (Alight != EnumAlight.Left)
            {
                int ws = FontRenderer.WidthString(text, size);
                x = Alight == EnumAlight.Center ? (Width - ws) / 2 : Width - ws - margin;
            }
            return x;
        }

        /// <summary>
        /// Событие клика по кнопке
        /// </summary>
        public event EventHandler Click;
        protected virtual void OnClick() => Click?.Invoke(this, new EventArgs());
    }
}
