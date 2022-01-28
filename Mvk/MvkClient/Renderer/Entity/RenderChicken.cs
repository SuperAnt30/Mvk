using MvkAssets;
using MvkClient.Renderer.Model;
using MvkServer.Entity;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер курицы
    /// </summary>
    public class RenderChicken : RendererLivingEntity
    {
        public RenderChicken(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Chicken;
            // соотношение высоты 1.0, к цельной модели 2.0, 1.0/2.0 = 0.5
            scale = 1.0f;
        }
    }
}
