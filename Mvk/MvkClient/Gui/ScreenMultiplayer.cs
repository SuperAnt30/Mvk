using MvkAssets;
using MvkClient.Setitings;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenMultiplayer : Screen
    {
        protected Label label;
        protected Label labelAddress;
        protected TextBox textBoxAddress;
        protected Button buttonConnect;
        protected Button buttonCancel;

        public ScreenMultiplayer(Client client) : base(client)
        {
            label = new Label(Language.T("gui.multiplayer"), FontSize.Font16);
            labelAddress = new Label(Language.T("gui.ip"), FontSize.Font12)
            {
                Width = 160,
                Alight = EnumAlight.Right
            };
            textBoxAddress = new TextBox(Setting.IpAddress) { Width = 160 };
            buttonConnect = new Button(Language.T("gui.connect")) { Width = 256 };
            buttonConnect.Click += ButtonConnect_Click;
            buttonCancel = new Button(EnumScreenKey.Main, Language.T("gui.cancel")) { Width = 256 };
            InitButtonClick(buttonCancel);
        }

        protected override void Init()
        {
            AddControls(label);
            AddControls(labelAddress);
            AddControls(textBoxAddress);
            AddControls(buttonConnect);
            AddControls(buttonCancel);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4);
            labelAddress.Position = new vec2i(Width / 2 - 162 * sizeInterface, Height / 4 + 92 * sizeInterface);
            textBoxAddress.Position = new vec2i(Width / 2 + 2 * sizeInterface, Height / 4 + 92 * sizeInterface);
            buttonConnect.Position = new vec2i(Width / 2 - 258 * sizeInterface, Height / 4 + 192 * sizeInterface);
            buttonCancel.Position = new vec2i(Width / 2 + 2 * sizeInterface, Height / 4 + 192 * sizeInterface);
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            Setting.IpAddress = textBoxAddress.Text;
            Setting.Save();
            OnFinished(new ScreenEventArgs(EnumScreenKey.Connection) { Tag = textBoxAddress.Text });
        }
    }
}
