using MvkAssets;
using MvkServer.Glm;

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

            buttonBack = new Button(EnumScreenKey.World, Language.T("gui.back.game"));
            InitButtonClick(buttonBack);
            buttonOptions = new Button(EnumScreenKey.Options, Language.T("gui.options"));
            buttonOptions.Click += (sender, e) 
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.InGameMenu));
            buttonExit = new Button(Language.T("gui.exit.world"));
            buttonExit.Click += (sender, e) => ClientMain.ExitingWorld("");
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
            buttonBack.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 48 * sizeInterface);
            buttonOptions.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 92 * sizeInterface);
            buttonExit.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 192 * sizeInterface);
        }
    }
}
