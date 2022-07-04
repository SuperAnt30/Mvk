using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenError : Screen
    {
        protected Label labelTitle;
        protected Label label;
        protected Button buttonCancel;
        protected int slot;

        public ScreenError(Client client, string text) : base(client)
        {
            labelTitle = new Label(Language.Current.Translate("gui.error"), FontSize.Font16);
            label = new Label(Language.Current.Translate(text), FontSize.Font12);
            
            buttonCancel = new Button(EnumScreenKey.Main, Language.Current.Translate("gui.menu")) { Width = 200 };
            buttonCancel.Click += (sender, e) => OnFinished(new ScreenEventArgs(EnumScreenKey.Main));
        }

        protected override void Init()
        {
            AddControls(labelTitle);
            AddControls(label);
            AddControls(buttonCancel);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            labelTitle.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4);
            label.Width = Width - 200;
            label.TransferText();
            label.Position = new vec2i(Width / 2 - (label.Width / 2) * sizeInterface, Height / 4 + 44 * sizeInterface);
            buttonCancel.Position = new vec2i(Width / 2 - 100 * sizeInterface, Height / 4 + 192 * sizeInterface);
        }
    }
}
