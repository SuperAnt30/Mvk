using MvkAssets;
using MvkClient.Renderer.Model;
using MvkServer.Entity;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер игрока
    /// </summary>
    public class RenderPlayer : RendererLivingEntity
    {

        public RenderPlayer(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Steve;
            // соотношение высоты 3.6, к цельной модели 2.0, 3.6/2.0 = 1.8
            scale = 1.8f;
        }


        //protected override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        //{
        //    //((ModelPlayer)mainModel).SetSneak(entity.IsSneaking);
        //    model.Render(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);

        //}
    }
}
