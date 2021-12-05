using MvkAssets;
using MvkClient.Actions;
using MvkClient.Renderer;
using MvkServer.Glm;
using SharpGL;
using System;
using System.Collections.Generic;

namespace MvkClient.Gui
{
    public class Screen : IDisposable
    {
        /// <summary>
        /// Колекция всех контролов
        /// </summary>
        public List<Control> Controls { get; protected set; } = new List<Control>();
        /// <summary>
        /// Ширина окна
        /// </summary>
        public int Width { get; private set; }
        /// <summary>
        /// Высота окна
        /// </summary>
        public int Height { get; private set; }
        /// <summary>
        /// Координата мыши
        /// </summary>
        public vec2i MouseCoord { get; private set; }
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        protected static OpenGL gl;

        /// <summary>
        /// Тип фона
        /// </summary>
        protected EnumBackground background = EnumBackground.Menu;
        /// <summary>
        /// Графический лист
        /// </summary>
        protected uint dList;

        protected Screen() { }
        public Screen(Client client) => ClientMain = client;

        public void Initialize()
        {
            gl = GLWindow.gl;
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            Init();
            Resized();
        }

        protected virtual void Init() { }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public virtual void Resized()
        {
            Width = GLWindow.WindowWidth;
            Height = GLWindow.WindowHeight;
            foreach (Control control in Controls)
            {
                control.Resized();
            }
            RenderList();
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        public void Draw() => GLRender.ListCall(dList);

        /// <summary>
        /// Рендер листа
        /// </summary>
        protected void RenderList()
        {
            uint list = GLRender.ListBegin();

            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(0, Width, Height, 0);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();

            Render();

            gl.EndList();
            GLRender.ListDelete(dList);
            dList = list;
        }

        /// <summary>
        /// Контролы
        /// </summary>
        protected virtual void RenderControls()
        {
            foreach(Control control in Controls)
            {
                control.Draw();
            }
        }
        /// <summary>
        /// Фон
        /// </summary>
        protected void RenderBackground()
        {
            if (background == EnumBackground.Menu)
            {
                gl.Enable(OpenGL.GL_TEXTURE_2D);
                GLWindow.Texture.BindTexture(AssetsTexture.OptionsBackground);
                gl.Color(.3f, .3f, .3f, 1f);
                GLRender.Rectangle(0, 0, Width, Height, 0, 0, Width / 64f, Height / 64f);
            }
            else
            {
                gl.Disable(OpenGL.GL_TEXTURE_2D);
                vec4 colBg = background == EnumBackground.Loading ? new vec4(1f, 1f, 1f, 1f) : new vec4(.3f, .3f, .3f, .5f);
                GLRender.Rectangle(0, 0, Width, Height, colBg);
            }
        }

        /// <summary>
        /// Прорисовка
        /// </summary>
        protected virtual void Render()
        {
            // Фон
            RenderBackground();
            // Контролы
            RenderControls();
        }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public virtual void MouseMove(int x, int y)
        {
            MouseCoord = new vec2i(x, y);
            bool isRender = false;
            foreach (Control control in Controls)
            {
                control.MouseMove(x, y);
                if (control.IsRender) isRender = true;
            }

            if (isRender) RenderList();
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public void MouseDown(MouseButton button, int x, int y)
        {
            if (button == MouseButton.Left)
            {
                // Отправляем клик на все контролы
                bool isRender = false;
                foreach (Control control in Controls)
                {
                    control.MouseClick(x, y);
                    if (control.IsRender) isRender = true;
                }
                if (isRender) RenderList();
            }
        }

        public void Dispose() => Delete();
        public void Delete() => GLRender.ListDelete(dList);

        public void AddControls(Control control)
        {
            control.Init(this);
            Controls.Add(control);
        }
    }
}
