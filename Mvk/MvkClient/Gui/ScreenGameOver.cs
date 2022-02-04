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

            label = new Label(Language.T("gui.game.over"), FontSize.Font16) { Scale = 2.0f };
            labelText = new Label(text, FontSize.Font12);
            buttonRespawn = new Button(Language.T("gui.respawn"));
            buttonRespawn.Click += (sender, e) => ClientMain.World.Player.Respawn();
            //InitButtonClick(buttonRespawn);
            buttonExit = new Button(Language.T("gui.exit.world"));
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
            label.Position = new vec2i(Width / 2 - 200, 120);
            labelText.Position = new vec2i(Width / 2 - 200, 200);
            buttonRespawn.Position = new vec2i(Width / 2 - 200, Height / 4 + 148);
            buttonExit.Position = new vec2i(Width / 2 - 200, Height / 4 + 192);
        }
    }
}
