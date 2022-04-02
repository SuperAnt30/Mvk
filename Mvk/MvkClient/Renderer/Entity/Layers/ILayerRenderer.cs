using MvkServer.Entity;

namespace MvkClient.Renderer.Entity.Layers
{
    public interface ILayerRenderer
    {
        void DoRenderLayer(EntityLiving entity, float limbSwing, float limbSwingAmount, float timeIndex, float ageInTicks, float headYaw, float headPitch, float scale);

        //bool ShouldCombineTextures();
    }
}
