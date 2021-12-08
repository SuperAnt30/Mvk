using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenInGameMenu : Screen
    {
        protected Button buttonBack;
        protected Button buttonOptions;
        protected Button buttonExit;

        public ScreenInGameMenu(Client client) : base(client)
        {
            background = EnumBackground.Game;

            buttonBack = new Button(EnumScreenKey.World, "Вернутся в игру");
            InitButtonClick(buttonBack);
            buttonOptions = new Button(EnumScreenKey.Options, "Опции...");
            buttonOptions.Click += (sender, e) 
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.InGameMenu));
            buttonExit = new Button("Выход из мира");
            buttonExit.Click += ButtonExit_Click;
        }

        protected override void Init()
        {
            AddControls(buttonBack);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            buttonBack.Position = new vec2i(Width / 2 - 200, Height / 4 + 48);
            buttonOptions.Position = new vec2i(Width / 2 - 200, Height / 4 + 92);
            buttonExit.Position = new vec2i(Width / 2 - 200, Height / 4 + 192);
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            OnFinished(EnumScreenKey.Main);
        }
    }
}
