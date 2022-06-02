using MvkAssets;
using MvkClient.Entity;
using MvkClient.Entity.Particle;
using MvkClient.Renderer.Entity;
using MvkClient.World;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.World.Block;
using System;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Объект рендера эффектов
    /// </summary>
    public class EffectRenderer
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }

        private RenderManager renderManager;
        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        private readonly Random rand = new Random();
        /// <summary>
        /// Карта всех тикущий частиц
        /// </summary>
        private MapListEntity map = new MapListEntity();
        /// <summary>
        /// Последний id
        /// </summary>
        private ushort lastId = 0;

        public EffectRenderer(WorldClient world)
        {
            renderManager = world.RenderEntityManager;
            World = world;
        }

        /// <summary>
        /// Заспавнить частицу
        /// </summary>
        public void SpawnParticle(EnumParticle particle, vec3 pos, vec3 motion, params int[] items)
        {
            switch (particle)
            {
                case EnumParticle.Test: AddEffect(new EntityTestFX(World, pos, motion)); break;
                case EnumParticle.Digging: AddEffect(new EntityDiggingFX(World, pos, motion, (EnumBlock)items[0])); break;
            }
        }

        /// <summary>
        /// Добавить сущность частицы
        /// </summary>
        /// <param name="particle"></param>
        public void AddEffect(EntityFX particle)
        {
            particle.SetEntityId(lastId++);
            map.Add(particle);
            if (map.Count > 4000) map.FirstRemove();
        }

        public void UpdateParticles()
        {
            MapListEntity remove = new MapListEntity();
            // Пробегаемся по всем частица
            for (int i = 0; i < map.Count; i++)
            {
                EntityFX entity = (EntityFX)map.GetAt(i);
                entity.Update();
                if (entity.IsDead)
                {
                    remove.Add(entity);
                }
            }
            // Удаляем мёртвых
            if (remove.Count > 0)
            {
                map.RemoveRange(remove);
                remove.Clear();
            }
        }

        /// <summary>
        /// Количество активных частиц
        /// </summary>
        public int CountParticles() => map.Count;

        /// <summary>
        /// Рендер всех тикущих частиц
        /// </summary>
        public void Render(float timeIndex)
        {
            GLRender.TextureLightmapEnable();
            for (int i = 0; i < map.Count; i++)
            {
                EntityFX entity = (EntityFX)map.GetAt(i);
                RenderParticles(entity, timeIndex);
            }
            GLRender.TextureLightmapDisable();
        }

        protected void BindTexture(AssetsTexture texture)
        {
            GLRender.Texture2DEnable();
            TextureStruct ts = GLWindow.Texture.GetData(texture);
            GLWindow.Texture.BindTexture(ts.GetKey());
        }

        public void RenderParticles(EntityFX particle, float timeIndex)
        {
            vec3 pos = particle.GetPositionFrame(timeIndex);
            pos.y += .2f;
            vec3 offset = renderManager.CameraOffset;
            GLRender.PushMatrix();
            {
                GLRender.LightmapTextureCoords(particle.GetBrightnessForRender());
                BindTexture(particle.Texture);
                GLRender.Translate(pos.x - offset.x, pos.y - offset.y, pos.z - offset.z);
                GLRender.Rotate(glm.degrees(-renderManager.CameraRotationYaw), 0, 1, 0);
                GLRender.Rotate(glm.degrees(renderManager.CameraRotationPitch), 1, 0, 0);
                particle.RenderRectangle(timeIndex);
            }
            GLRender.PopMatrix();
        }

    }
}
