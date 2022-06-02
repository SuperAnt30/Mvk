using MvkAssets;
using MvkClient.Renderer.Entity.Layers;
using MvkClient.Renderer.Model;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Collections.Generic;

namespace MvkClient.Renderer.Entity
{
    /// <summary>
    /// Абстракный класс рендера сущностей
    /// </summary>
    public abstract class RendererLivingEntity : RenderEntityBase
    {
        protected ModelBase model;

        protected AssetsTexture texture = AssetsTexture.Steve;
        protected float scale = 1f;
        protected List<ILayerRenderer> layers = new List<ILayerRenderer>();


        protected RendererLivingEntity(RenderManager renderManager, ModelBase model) : base(renderManager) 
            => this.model = model;

        public override void DoRender(EntityBase entity, vec3 offset, float timeIndex)
        {
            if (entity is EntityLiving entityLiving)
            {
                vec3 pos = entity.GetPositionFrame(timeIndex);
                vec3 offsetPos = pos - offset;
                float yawBody = entityLiving.GetRotationYawBodyFrame(timeIndex);
                float yawHead = entityLiving.GetRotationYawFrame(timeIndex);
                float headPitch = entityLiving.GetRotationPitchFrame(timeIndex);
                float limbSwing = entityLiving.LimbSwing - entityLiving.LimbSwingAmount * (1f - timeIndex);
                float limbSwingAmount = entityLiving.GetLimbSwingAmountFrame(timeIndex);
                float ageInTicks = renderManager.World.ClientMain.TickCounter + timeIndex;

                model.SetSwingProgress(entityLiving.GetSwingProgressFrame(timeIndex));
                
                GLRender.PushMatrix();
                
                GLRender.CullDisable();
                vec3 color = new vec3(1f);
                    
                if (entityLiving.DamageTime > 0)
                {
                    float dt = Mth.Sqrt((entityLiving.DamageTime + timeIndex - 1f) / 5f * 1.6f);
                    if (dt > 1f) dt = 1f;
                    dt *= .4f;
                    color = new vec3(1f, 1f - dt, 1f - dt);
                }

                GLRender.LightmapTextureCoords(entity.GetBrightnessForRender());
                GLRender.Color(color);
                BindTexture();

                GLRender.Translate(offsetPos.x, offsetPos.y, offsetPos.z);
                RotateCorpse(entityLiving, timeIndex);

                GLRender.Scale(scale);
                GLRender.Translate(0, -1.508f, 0);

                GLRender.Rotate(glm.degrees(yawBody), 0, 1, 0);
                yawBody -= yawHead;
                
                RenderModel(entityLiving, limbSwing, limbSwingAmount, ageInTicks, -yawBody, headPitch, .0625f);

                // доп слой
                LayerRenders(entityLiving, limbSwing, limbSwingAmount, timeIndex, ageInTicks, -yawBody, headPitch, .0625f);

                GLRender.TextureLightmapDisable();
                GLRender.PopMatrix();
                
                base.DoRender(entity, offset, timeIndex);
            }
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
            GLRender.TextureLightmapEnable();
            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(texture);
            GLWindow.Texture.BindTexture(ts.GetKey());
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

        /// <summary>
        /// Добавить слой
        /// </summary>
        public void AddLayer(ILayerRenderer layer) => layers.Add(layer);

        /// <summary>
        /// Удалить слой
        /// </summary>
        public void RemoveLayer(ILayerRenderer layer) => layers.Remove(layer);

        /// <summary>
        /// Рендер доп слоёв если такие есть
        /// </summary>
        protected void LayerRenders(EntityLiving entity, float limbSwing, float limbSwingAmount, float timeIndex, float ageInTicks, float headYaw, float headPitch, float scale)
        {
            foreach (ILayerRenderer layer in layers)
            {
                layer.DoRenderLayer(entity, limbSwing, limbSwingAmount, timeIndex, ageInTicks, headYaw, headPitch, scale);
            }
        }
    }
}
