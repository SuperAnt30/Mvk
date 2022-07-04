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
            background = EnumBackground.GameWindow;

            buttonBack = new Button(EnumScreenKey.World, Language.Current.Translate("gui.back.game"));
            InitButtonClick(buttonBack);
            buttonOptions = new Button(EnumScreenKey.Options, Language.Current.Translate("gui.options"));
            buttonOptions.Click += (sender, e) 
                => OnFinished(new ScreenEventArgs(EnumScreenKey.Options, EnumScreenKey.InGameMenu));
            buttonExit = new Button(Language.Current.Translate("gui.exit.world"));
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
            int h = Height / 4 + 48 * sizeInterface;
            int hMax = h + 248 * sizeInterface;
            if (hMax > Height) h -= hMax - Height;

            buttonBack.Position = new vec2i(Width / 2 - 200 * sizeInterface, h);
            buttonOptions.Position = new vec2i(Width / 2 - 200 * sizeInterface, h + 44 * sizeInterface);
            buttonExit.Position = new vec2i(Width / 2 - 200 * sizeInterface, h + 144 * sizeInterface);
        }
    }
}
