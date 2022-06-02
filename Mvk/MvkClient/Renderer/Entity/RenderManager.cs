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
        public Client ClientMain { get; private set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; private set; }
        /// <summary>
        /// Скрыть ли хитбокс сущности
        /// </summary>
        public bool IsHiddenHitbox { get; set; } = true;
        /// <summary>
        /// Позиция основного игрока, камеры
        /// </summary>
        public vec3 CameraPosition { get; private set; }
        public float CameraRotationYaw { get; private set; }
        public float CameraRotationPitch { get; private set; }
        /// <summary>
        /// Смещение камеры для всего мира и всех элементов
        /// </summary>
        public vec3 CameraOffset { get; private set; }

        public RenderItem Item { get; private set; }

        /// <summary>
        /// Перечень рендер объектов сущьностей
        /// </summary>
        private Hashtable entities = new Hashtable();

        

        public RenderManager(WorldClient world)
        {
            World = world;
            ClientMain = world.ClientMain;
            Item = new RenderItem();
            entities.Add(EnumEntities.Player, new RenderPlayer(this, new ModelPlayer()));
            entities.Add(EnumEntities.PlayerHand, new RenderHead(this, new ModelPlayerHand()));
            entities.Add(EnumEntities.Chicken, new RenderChicken(this, new ModelChicken()));
            entities.Add(EnumEntities.Item, new RenderEntityItem(this, Item));

        }

        /// <summary>
        /// Задать камеру игрока
        /// </summary>
        public void SetCamera(vec3 pos, float yaw, float pitch)
        {
            CameraPosition = pos;
            CameraRotationYaw = yaw;
            CameraRotationPitch = pitch;
            CameraOffset = new vec3(pos.x, pos.y, pos.z);
        }

        protected RenderEntityBase GetEntityRenderObject(EntityBase entity)
        {
            if (entities.ContainsKey(entity.Type))
            {
                return entities[entity.Type] as RenderEntityBase;
            }
            return null;
        }

        /// <summary>
        /// Сгенерировать сущность на экране
        /// </summary>
        public void RenderEntity(EntityBase entity, float timeIndex)
        {
            if (!entity.IsDead)
            {
                World.CountEntitiesShowAdd();
                RenderEntityBase render = GetEntityRenderObject(entity);
                 
                if (render != null)
                {
                    render.DoRenderShadowAndFire(entity, CameraOffset, timeIndex);
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
        protected void RenderEntityBoundingBox(EntityBase entity, vec3 offset, float timeIndex)
        {
            vec3 pos0 = entity.GetPositionFrame(timeIndex);
            vec3 look = new vec3(0);
            AxisAlignedBB aabb = entity.GetBoundingBox(new vec3(0));
            float eye = aabb.Min.y;
            float width = entity.Width;

            bool isLook = false;
            if (entity is EntityLiving)
            {
                look += ((EntityLiving)entity).GetLookFrame(timeIndex);
                eye += ((EntityLiving)entity).GetEyeHeight();
                isLook = true;
            }

            GLRender.PushMatrix();
            {
                GLRender.Translate(pos0 - offset);
                GLRender.Texture2DDisable();
                GLRender.CullDisable();
                GLRender.LineWidth(2f);

                // Рамка хитбокса
                GLRender.Color(new vec4(1));
                GLRender.DrawOutlinedBoundingBox(aabb);

                if (isLook)
                {
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
                }

                GLRender.CullEnable();
            }
            GLRender.PopMatrix();
        }
    }
}
