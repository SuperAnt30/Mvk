using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenGameOver : Screen
    {
        protected Label label;
        protected Label labelText;
        protected Button buttonRespawn;
        protected Button buttonExit;

        public ScreenGameOver(Client client, string text) : base(client)
        {
            background = EnumBackground.GameOver;

            label = new Label(Language.Current.Translate("gui.game.over"), FontSize.Font16) { Scale = 2.0f };
            labelText = new Label(text, FontSize.Font12);
            buttonRespawn = new Button(Language.Current.Translate("gui.respawn"));
            buttonRespawn.Click += (sender, e) => ClientMain.World.Respawn();
            //InitButtonClick(buttonRespawn);
            buttonExit = new Button(Language.Current.Translate("gui.exit.world"));
            buttonExit.Click += (sender, e) => ClientMain.ExitingWorld("");
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(labelText);
            AddControls(buttonRespawn);
            AddControls(buttonExit);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {


            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, 120);
            labelText.Position = new vec2i(Width / 2 - 200 * sizeInterface, 120 + 80 * sizeInterface);
            buttonRespawn.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 148 * sizeInterface);
            buttonExit.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 192 * sizeInterface);
        }
    }
}
