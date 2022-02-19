using MvkClient.Entity;
using MvkClient.Renderer.Model;
using MvkClient.World;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using SharpGL;
using System.Collections;

namespace MvkClient.Renderer.Entity
{
    public class RenderManager
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        /// <summary>
        /// Скрыть ли хитбокс сущности
        /// </summary>
        public bool IsHiddenHitbox { get; set; } = true;

        /// <summary>
        /// Перечень рендер объектов сущьностей
        /// </summary>
        private Hashtable entities = new Hashtable();

        /// <summary>
        /// Позиция основного игрока, камеры
        /// </summary>
        public vec3 CameraPosition { get; protected set; }
        public float CameraRotationYaw { get; protected set; }
        public float CameraRotationPitch { get; protected set; }
        /// <summary>
        /// Смещение камеры для всего мира и всех элементов
        /// </summary>
        public vec3 CameraOffset { get; protected set; }

        public RenderManager(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            entities.Add(EnumEntities.Player, new RenderPlayer(this, new ModelPlayer()));
            entities.Add(EnumEntities.PlayerHand, new RenderPlayer(this, new ModelPlayerHand()));
            entities.Add(EnumEntities.Chicken, new RenderChicken(this, new ModelChicken()));
            
        }

        /// <summary>
        /// Задать камеру игрока
        /// </summary>
        public void SetCamera(vec3 pos, float yaw, float pitch)
        {
            CameraPosition = pos;
            CameraRotationYaw = yaw;
            CameraRotationPitch = pitch;
            CameraOffset = new vec3(pos.x, 0, pos.z);
        }

        protected RendererLivingEntity GetEntityRenderObject(EntityLiving entity)
        {
            if (entities.ContainsKey(entity.Type))
            {
                return entities[entity.Type] as RendererLivingEntity;
            }
            return null;
        }

        /// <summary>
        /// Сгенерировать сущность на экране
        /// </summary>
        public void RenderEntity(EntityLiving entity, float timeIndex)
        {
            if (!entity.IsDead)
            {
                World.CountEntitiesShowAdd();
                RendererLivingEntity render = GetEntityRenderObject(entity);
                 
                if (render != null)
                {
                    render.DoRender(entity, CameraOffset, timeIndex);
                    if (!IsHiddenHitbox)
                    {
                        RenderEntityBoundingBox(entity, CameraOffset, timeIndex);
                    }
                }
            }
        }

        /// <summary>
        /// Отрисовать рамку хитбокса сущности, для отладки
        /// </summary>
        protected void RenderEntityBoundingBox(EntityLiving entity, vec3 offset, float timeIndex)
        {
            vec3 pos0 = entity.GetPositionFrame2(timeIndex);
            vec3 look = entity.GetLookFrame2(timeIndex);
            AxisAlignedBB aabb = entity.GetBoundingBox(new vec3(0));
            float eye = entity.GetEyeHeight() + aabb.Min.y;
            float width = entity.Width;

            GLRender.PushMatrix();
            GLRender.Translate(pos0 - offset);
            GLRender.Texture2DDisable();
            GLRender.CullDisable();
            GLRender.LineWidth(2f);

            // Рамка хитбокса
            GLRender.Color(new vec4(1));
            GLRender.DrawOutlinedBoundingBox(aabb);

            // Уровень глаз
            GLRender.Color(new vec4(1, 0, 0, 1));
            GLRender.Begin(OpenGL.GL_LINE_STRIP);
            GLRender.Vertex(aabb.Min.x, eye, aabb.Min.z);
            GLRender.Vertex(aabb.Max.x, eye, aabb.Min.z);
            GLRender.Vertex(aabb.Max.x, eye, aabb.Max.z);
            GLRender.Vertex(aabb.Min.x, eye, aabb.Max.z);
            GLRender.Vertex(aabb.Min.x, eye, aabb.Min.z);
            GLRender.End();

            // Луч глаз куда смотрит
            GLRender.Color(new vec4(0, 0, 1, 1));
            GLRender.Begin(OpenGL.GL_LINES);
            vec3 pos = new vec3(aabb.Min.x + width, eye, aabb.Min.z + width);
            GLRender.Vertex(pos);
            GLRender.Vertex(pos + look * 2f);
            GLRender.End();

            GLRender.CullEnable();
            GLRender.PopMatrix();
        }
    }
}
