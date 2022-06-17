using MvkAssets;
using MvkServer.Glm;

namespace MvkClient.Gui
{
    /// <summary>
    /// Загрузка мира
    /// </summary>
    public class ScreenWorldLoading : ScreenLoading
    {
        protected Label label;

        public ScreenWorldLoading(Client client, int slot) : base(client) 
            => label = new Label(string.Format(Language.T("gui.loading.world"), 0), FontSize.Font16);

        /// <summary>
        /// Следующий шаг загрузки
        /// </summary>
        public override void Step()
        {
            label.SetText(string.Format(Language.T("gui.loading.world"), (value + 1f) * 100f / max));
            base.Step();
        }

        protected override void Init() => AddControls(label);

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        protected override void ResizedScreen() => label.Position = new vec2i(Width / 2 - 200 * sizeInterface, Height / 2);
    }
}
