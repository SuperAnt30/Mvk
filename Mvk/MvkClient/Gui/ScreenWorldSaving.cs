using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenWorldSaving : Screen
    {
        protected Label label;

        public ScreenWorldSaving(Client client) : base(client)
        {
            label = new Label(Language.T("gui.saving"), FontSize.Font16);
        }

        protected override void Init()
        {
            AddControls(label);
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen()
        {
            label.Position = new vec2i(Width / 2 - 200, Height / 4 + 44);
        }
    }
}
