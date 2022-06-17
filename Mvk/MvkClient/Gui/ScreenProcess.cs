using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    public class ScreenProcess : Screen
    {
        protected Label label;

        public ScreenProcess(Client client, string text) : base(client)
        {
            label = new Label(text, FontSize.Font16);
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
            label.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 4 + 44 * sizeInterface);
        }
    }
}
