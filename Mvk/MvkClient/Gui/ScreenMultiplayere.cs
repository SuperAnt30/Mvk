using MvkAssets;
using MvkServer.Glm;
using System;

namespace MvkClient.Gui
{
    public class ScreenMultiplayere : Screen
    {
        protected Label label;
        protected Label labelAddress;
        protected TextBox textBoxAddress;
        protected Button buttonConnect;
        protected Button buttonCancel;

        public ScreenMultiplayere(Client client) : base(client)
        {
            label = new Label("Сетевая игра", FontSize.Font16);
            labelAddress = new Label("IP адрес:", FontSize.Font12) { Width = 160 };
            textBoxAddress = new TextBox("127.0.0.1") { Width = 160 };
            buttonConnect = new Button("Соединится") { Width = 256 };
            buttonConnect.Click += ButtonConnect_Click;
            buttonCancel = new Button(EnumScreenKey.Main, "Отмена") { Width = 256 };
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
            label.Position = new vec2i(Width / 2 - 200, Height / 4);
            labelAddress.Position = new vec2i(Width / 2 - 158, Height / 4 + 92);
            textBoxAddress.Position = new vec2i(Width / 2 + 2, Height / 4 + 92);
            buttonConnect.Position = new vec2i(Width / 2 - 258, Height / 4 + 192);
            buttonCancel.Position = new vec2i(Width / 2 + 2, Height / 4 + 192);
        }

        private void ButtonConnect_Click(object sender, EventArgs e) 
            => OnFinished(new ScreenEventArgs(EnumScreenKey.Connection) { Tag = textBoxAddress.Text });
    }
}
