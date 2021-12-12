using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenMainMenu : Screen
    {
        protected Button buttonSingle;
        protected Button buttonMultiplayere;
        protected Button buttonOptions;
        protected Button buttonExit;

        public ScreenMainMenu(Client client) : base(client)
        {
            background = EnumBackground.TitleMain;

            buttonSingle = new Button(EnumScreenKey.SinglePlayer, Language.T("gui.singleplayer")) { Width = 300 };
            InitButtonClick(buttonSingle);
            buttonMultiplayere = new Button(EnumScreenKey.Multiplayere, Language.T("gui.multiplayere")) { Width = 300 };
            InitButtonClick(buttonMultiplayere);
            buttonOptions = new Button(EnumScreenKey.Options, Language.T("gui.options")) { Width = 300 };
            buttonOptions.Click += (sender, e)
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.Main));
            buttonExit = new Button(Language.T("gui.exit")) { Width = 300 };
            buttonExit.Click += ButtonExit_Click;

        }

        protected override void Init()
        {
            AddControls(buttonSingle);
            AddControls(buttonMultiplayere);
            AddControls(buttonOptions);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            buttonSingle.Position = new vec2i(100, Height / 4 + 92);
            buttonMultiplayere.Position = new vec2i(100, Height / 4 + 136);
            buttonOptions.Position = new vec2i(100, Height / 4 + 180);
            buttonExit.Position = new vec2i(100, Height / 4 + 240);
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            ClientMain.WindowClosing();
        }
    }
}
