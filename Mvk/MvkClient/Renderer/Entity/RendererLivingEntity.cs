using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Font;
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
            float yawBody = entity.GetRotationYawBodyFrame(timeIndex);
            float yawHead = entity.GetRotationYawFrame(timeIndex);
            model.SetSwingProgress(entity.GetSwingProgressFrame(timeIndex));

            GLRender.PushMatrix();
            {
                
                GLRender.CullDisable();

                vec3 color = new vec3(1);
                if (entity.DamageTime > 0)
                {
                    float dt = Mth.Sqrt((entity.DamageTime + timeIndex - 1f) / 5f * 1.6f);
                    if (dt > 1f) dt = 1f;
                    dt *= .4f;
                    color = new vec3(1f, 1f - dt, 1f - dt);
                }

                GLRender.Color(color);// entity.DamageTime > 0 ? new vec3(1f, .8f, .8f) : new vec3(1f));
                BindTexture();

                GLRender.Translate(pos.x - offset.x, pos.y - offset.y, pos.z - offset.z);
                GLRender.PushMatrix();
                {
                    RotateCorpse(entity, timeIndex);
                    GLRender.Scale(scale, scale, scale);
                    GLRender.Translate(0, -1.507f, 0);
                   
                    GLRender.Rotate(glm.degrees(yawBody), 0, 1, 0);
                    yawBody -= yawHead;

                    float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

                    RenderModel(entity, entity.LimbSwing, entity.GetLimbSwingAmountFrame(timeIndex), ageInTicks,
                        -yawBody, entity.GetRotationPitchFrame(timeIndex), .0625f);
                }
                GLRender.PopMatrix();
                RenderLivingLabel(entity);
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

        /// <summary>
        /// Название сущности над головой, если имеется
        /// </summary>
        protected void RenderLivingLabel(EntityLiving entity)
        {
            if (entity.Name == "" || entity == renderManager.ClientMain.Player) return;

            string text = entity.Name;

            EntityPlayerSP player = renderManager.ClientMain.Player;
            float dis = glm.distance(renderManager.CameraPosition, entity.Position);

            if (dis <= 64) // дистанция между сущностями
            {
                float size = 3.2f;
                float scale = 0.0167f * size;
                FontSize font = FontSize.Font8;
                int ws = FontRenderer.WidthString(entity.Name, font) / 2;
                
                GLRender.PushMatrix();
                {
                    GLRender.DepthDisable();
                    GLRender.Translate(0, entity.Height + .5f, 0);
                    GLRender.Rotate(glm.degrees(-renderManager.CameraRotationYaw), 0, 1, 0);
                    GLRender.Rotate(glm.degrees(renderManager.CameraRotationPitch), 1, 0, 0);
                    GLRender.Scale(renderManager.ClientMain.Player.ViewCamera == EnumViewCamera.Front ? -scale : scale, -scale, scale);
                    GLRender.Texture2DDisable();
                    GLRender.Rectangle(-ws - 1, -1, ws + 1, 8, new vec4(0, 0, 0, .25f));
                    GLRender.Texture2DEnable();
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(font));
                    FontRenderer.RenderString(-ws, 0, new vec4(1), entity.Name, font);

                    GLRender.DepthEnable();
                }
                GLRender.PopMatrix();
            }
        }


        /// <summary>
        /// Разворот вертикальный, сущности
        /// тут же анимация падения при смерти
        /// </summary>
        /// <param name="entity">Объект сущности</param>
        /// <param name="timeIndex">Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        protected void RotateCorpse(EntityLiving entity, float timeIndex)
        {
            GLRender.Rotate(180f, 0, 0, 1);

            if (entity.DeathTime > 0)
            {
                // Если сущность умирает, анимация падения на бок
                float angle = Mth.Sqrt((entity.DeathTime + timeIndex - 1f) / 20f * 1.6f);
                if (angle > 1.0f) angle = 1.0f;
                GLRender.Rotate(angle * 90f, 0, 0, 1);
            }
        }

    }
}
