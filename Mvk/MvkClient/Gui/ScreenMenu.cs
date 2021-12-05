
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenMenu : Screen
    {

        public ScreenMenu(Client client) : base(client)
        {
            background = EnumBackground.Menu;
        }

        protected override void Init()
        {
            Button button;
            AddControls(new Button(Width / 2 - 200, Height / 4 + 48, "Одиночная игра"));
            button = new Button(Width / 2 - 200, Height / 4 + 92, "Сетевая игра");
            button.Enabled = false;
            AddControls(button);
            AddControls(new Button(Width / 2 - 200, Height / 4 + 136, "Опции..."));
            button = new Button(Width / 2 - 200, Height / 4 + 192, "Выход");
            button.Click += ButtonExit_Click;
            AddControls(button);
        }

        private void ButtonExit_Click(object sender, System.EventArgs e)
        {
            ClientMain.WindowClosing();
        }

        public override void MouseMove(int x, int y)
        {
            base.MouseMove(x, y);

            
            //RenderList();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public override void Resized()
        {
            base.Resized();
            Controls[0].Position = new vec2i(Width / 2 - 200, Height / 4 + 48);
            Controls[1].Position = new vec2i(Width / 2 - 200, Height / 4 + 92);
            Controls[2].Position = new vec2i(Width / 2 - 200, Height / 4 + 136);
            Controls[3].Position = new vec2i(Width / 2 - 200, Height / 4 + 192);
            RenderList();
        }

        //protected override void Render()
        //{
        //    base.Render();
        //    gl.Disable(OpenGL.GL_TEXTURE_2D);
        //    GLRender.Rectangle(MouseCoord.x, MouseCoord.y, MouseCoord.x + 32, MouseCoord.y + 32, new MvkServer.Glm.vec4(1.0f, .3f, .3f, 0.5f));
        //}
    }
}
