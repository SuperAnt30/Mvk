using MvkAssets;
using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер руки игрока
    /// </summary>
    public class RenderHead : RendererLivingEntity
    {

        public RenderHead(RenderManager renderManager, ModelBase model) : base(renderManager, model)
        {
            texture = AssetsTexture.Steve;
        }

        public override void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (entity is EntityLiving entityLiving)
            {
                vec3 pos = entity.GetPositionFrame(timeIndex);
                float eye = entityLiving.GetEyeHeightFrame();
                float yawHead = entityLiving.GetRotationYawFrame(timeIndex);

                model.SetSwingProgress(entityLiving.GetSwingProgressFrame(timeIndex));

                GLRender.PushMatrix();
                {
                    GLRender.CullEnable();
                    GLRender.DepthDisable();
                    vec4 color = new vec4(1);
                    GLRender.Color(color);
                    BindTexture();

                    GLRender.Translate(pos.x - offset.x, pos.y - offset.y, pos.z - offset.z);
                    GLRender.PushMatrix();
                    {
                        RotateCorpse(entityLiving, timeIndex);
                        GLRender.Translate(0, -eye, 0);
                        GLRender.Rotate(glm.degrees(renderManager.CameraRotationYaw), 0, 1, 0);
                        GLRender.Rotate(glm.degrees(-renderManager.CameraRotationPitch), 1, 0, 0);

                        float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

                        RenderModel(entityLiving, entityLiving.LimbSwing, entityLiving.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
                            yawHead, entityLiving.GetRotationPitchFrame(timeIndex), .0625f);

                    }
                    GLRender.PopMatrix();
                    GLRender.DepthEnable();
                }
                GLRender.PopMatrix();
            }
        }

        //protected override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        //{
        //    //((ModelPlayer)mainModel).SetSneak(entity.IsSneaking);
        //    model.Render(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);

        //}
    }
}
