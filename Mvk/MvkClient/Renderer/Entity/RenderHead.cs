using MvkAssets;
using MvkClient.Renderer.Entity.Layers;
using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Рендер руки игрока
    /// </summary>
    public class RenderHead : RendererLivingEntity
    {

        public RenderHead(RenderManager renderManager, ModelPlayerHand model) : base(renderManager, model)
        {
            texture = AssetsTexture.Steve;
            AddLayer(new LayerHeldItem(model.BoxArmRight, renderManager.Item, true));
        }

        public override void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (entity is EntityLiving entityLiving)
            {
                vec3 pos = entity.GetPositionFrame(timeIndex);
                float eye = entityLiving.GetEyeHeightFrame();
                float yawHead = entityLiving.GetRotationYawFrame(timeIndex);
                float headPitch = entityLiving.GetRotationPitchFrame(timeIndex);
                float limbSwing = entityLiving.LimbSwing - entityLiving.LimbSwingAmount * (1f - timeIndex);
                float limbSwingAmount = entityLiving.GetLimbSwingAmountFrame(timeIndex);
                float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

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

                        GLWindow.gl.PushMatrix();
                        {                        
                            // Смещение руки примерно чтоб при ударе не виден разрыв, и в пасиве не сильно мешала
                            GLWindow.gl.Translate(-0.5f, 1.01f, -0.5f);
                            // Руку поворачиваем чтоб видеи ребро
                            GLRender.Rotate(65, 0, 0, 1);
                            RenderModel(entityLiving, limbSwing, limbSwingAmount, ageInTicks, yawHead, headPitch, .0625f);
                            // доп слой
                            LayerRenders(entityLiving, limbSwing, limbSwingAmount, timeIndex, ageInTicks, yawHead, headPitch, .0625f);
                        }
                        GLWindow.gl.PopMatrix();
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
