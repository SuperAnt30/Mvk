using MvkAssets;
using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Абстракный класс рендера сущностей
    /// </summary>
    public abstract class RendererLivingEntity
    {
        protected RenderManager renderManager;
        protected ModelBase model;

        protected AssetsTexture texture = AssetsTexture.Steve;
        protected float scale = 1f;

        public RendererLivingEntity(RenderManager renderManager, ModelBase model)
        {
            this.renderManager = renderManager;
            this.model = model;
        }

        public void DoRender(EntityLiving entity, vec3 offset, float timeIndex)
        {
            vec3 pos = entity.GetPositionFrame(timeIndex);

            GLRender.PushMatrix();
            {
                
                GLRender.CullDisable();
                GLRender.Color(1f, 1f, 1f);
                BindTexture();

                GLRender.Translate(pos.x - offset.x, pos.y - offset.y, pos.z - offset.z);
                GLRender.PushMatrix();
                {
                    GLRender.Scale(scale, scale, scale);
                    GLRender.Translate(0, 1.500f, 0);
                    GLRender.Rotate(180f, 0, 0, 1);
                    float yaw = entity.GetRotationYawBodyFrame(timeIndex);
                    GLRender.Rotate(glm.degrees(yaw), 0, 1, 0);
                    yaw -= entity.GetRotationYawFrame(timeIndex);

                    float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

                    RenderModel(entity, entity.LimbSwing, entity.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
                        -yaw, entity.GetRotationPitchFrame(timeIndex), .0625f);
                }
                GLRender.PopMatrix();
            }
            GLRender.PopMatrix();
        }

        /// <summary>
        /// Рендер модели
        /// </summary>
        protected virtual void RenderModel(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            model.Render(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);
        }

        protected void BindTexture()
        {
            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(texture);
            GLWindow.Texture.BindTexture(ts.GetKey());
        }
    }
}
