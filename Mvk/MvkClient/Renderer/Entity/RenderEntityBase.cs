using MvkAssets;
using MvkClient.Entity;
using MvkClient.Renderer.Font;
using MvkServer.Entity;
using MvkServer.Glm;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Абстракный класс рендера сущностей
    /// </summary>
    public abstract class RenderEntityBase
    {
        protected RenderManager renderManager;
        /// <summary>
        /// Текстура тени
        /// </summary>
        //private static final ResourceLocation shadowTextures = new ResourceLocation("textures/misc/shadow.png");
        /// <summary>
        /// Размер тени
        /// </summary>
        protected float shadowSize;
        /// <summary>
        /// Определяет темноту тени объекта. Чем выше значение, тем темнее тень.
        /// </summary>
        protected float shadowOpaque = 1.0f;

        protected RenderEntityBase(RenderManager renderManager) => this.renderManager = renderManager;

        public virtual void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (IsEntityLabel(entity))
            {
                RenderLivingLabel(entity, entity.GetName(), offset);
            }
        }

        /// <summary>
        /// Можно ли прописывать название над сущностью
        /// </summary>
        protected bool IsEntityLabel(EntityBase entity) => entity.HasCustomName();

        /// <summary>
        /// Название сущности над головой
        /// </summary>
        protected void RenderLivingLabel(EntityBase entity, string text, vec3 offset)
        {
            EntityPlayerSP player = renderManager.ClientMain.Player;
            float dis = glm.distance(renderManager.CameraPosition, entity.Position);

            if (dis <= 64) // дистанция между сущностями
            {
                float size = 3.2f;
                float scale = 0.0167f * size;
                FontSize font = FontSize.Font8;
                int ws = FontRenderer.WidthString(text, font) / 2;

                GLRender.PushMatrix();
                {
                    GLRender.DepthDisable();
                    GLRender.Translate(offset.x, offset.y + entity.Height + .5f, offset.z);
                    GLRender.Rotate(glm.degrees(-renderManager.CameraRotationYaw), 0, 1, 0);
                    GLRender.Rotate(glm.degrees(renderManager.CameraRotationPitch), 1, 0, 0);
                    GLRender.Scale(renderManager.ClientMain.Player.ViewCamera == EnumViewCamera.Front ? -scale : scale, -scale, scale);
                    GLRender.Texture2DDisable();
                    GLRender.Rectangle(-ws - 1, -1, ws + 1, 8, new vec4(0, 0, 0, .25f));
                    GLRender.Texture2DEnable();
                    GLWindow.Texture.BindTexture(Assets.ConvertFontToTexture(font));
                    FontRenderer.RenderString(-ws, 0, new vec4(1), text, font);

                    GLRender.DepthEnable();
                }
                GLRender.PopMatrix();
            }
        }
    }
}
