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

        public override void DoRender(EntityLiving entity, vec3 offset, float timeIndex)
        {
            vec3 pos = entity.GetPositionFrame(timeIndex);
            float eye = entity.GetEyeHeightFrame();
            float yawHead = entity.GetRotationYawFrame(timeIndex);

            model.SetSwingProgress(entity.GetSwingProgressFrame(timeIndex));

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
                    RotateCorpse(entity, timeIndex);
                    GLRender.Translate(0, -eye, 0);
                    GLRender.Rotate(glm.degrees(renderManager.CameraRotationYaw), 0, 1, 0);
                    GLRender.Rotate(glm.degrees(-renderManager.CameraRotationPitch), 1, 0, 0);

                    float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

                    RenderModel(entity, entity.LimbSwing, entity.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
                        yawHead, entity.GetRotationPitchFrame(timeIndex), .0625f);
                    
                }
                GLRender.PopMatrix();
                GLRender.DepthEnable();
            }
            GLRender.PopMatrix();
        }

        //protected override void Render(EntityLiving entity, float limbSwing, float limbSwingAmount, float ageInTicks, float headYaw, float headPitch, float scale)
        //{
        //    //((ModelPlayer)mainModel).SetSneak(entity.IsSneaking);
        //    model.Render(entity, limbSwing, limbSwingAmount, ageInTicks, headYaw, headPitch, scale);

        //}
    }
}
