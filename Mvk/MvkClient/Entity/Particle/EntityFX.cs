using MvkAssets;
using MvkClient.Renderer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Сущность эффектов частиц
    /// </summary>
    public abstract class EntityFX : EntityBase
    {
        /// <summary>
        /// Текстура
        /// </summary>
        public AssetsTexture Texture { get; protected set; } = AssetsTexture.Particles;
        
        /// <summary>
        /// Координаты смещения в атласе текстуры
        /// </summary>
        protected vec2i textureUV = new vec2i(0);
        /// <summary>
        /// Размер частицы
        /// </summary>
        protected float particleScale;
        /// <summary>
        /// Сколько живёт частица в тактах
        /// </summary>
        protected int particleAge = 0;
        /// <summary>
        /// Максимальная жизнь частицы в тактах
        /// </summary>
        protected int particleMaxAge;
        /// <summary>
        /// Гравитация частицы
        /// </summary>
        protected float particleGravity = 0f;
        /// <summary>
        /// Цвет частицы
        /// </summary>
        protected vec4 color = new vec4(1);

        /// <summary>
        /// Задать частицу с движением
        /// </summary>
        /// <param name="world">основной мир</param>
        /// <param name="pos">стартовая позиция частицы</param>
        public EntityFX(WorldBase world, vec3 pos) : base(world)
        {
            SetPosition(pos);
            LastTickPos = Position;
            particleScale = rand.Next(100, 200) / 100f;
            particleMaxAge = rand.Next(4, 40);
            SetSize(.1f, .2f);
        }

        /// <summary>
        /// Задать частицу с движением
        /// </summary>
        /// <param name="world">основной мир</param>
        /// <param name="pos">стартовая позиция частицы</param>
        /// <param name="motion">стартовое перемещение</param>
        public EntityFX(WorldBase world, vec3 pos, vec3 motion) : this(world, pos)
        {
            motion.x += rand.Next(-100, 100) * .004f;
            motion.y += rand.Next(-100, 100) * .004f;
            motion.z += rand.Next(-100, 100) * .004f;
            float r = (float)(rand.NextDouble() + rand.NextDouble() + 1f) * .06f;
            float sq = Mth.Sqrt(motion.x * motion.x + motion.y * motion.y + motion.z * motion.z);
            motion = motion / sq * r;
            motion.y += .1f;
            Motion = motion;
        }

        public override void Update()
        {
            LastTickPos = PositionPrev = Position;

            if (particleAge++ >= particleMaxAge)
            {
                SetDead();
            }

            vec3 motion = Motion;
            motion.y -= .04f * particleGravity;

            // Проверка столкновения
            MoveEntity(motion);
            motion = Motion * .98f;

            if (OnGround)
            {
                motion.x *= .69999f;
                motion.z *= .69999f;
            }

            Motion = motion;
        }

        /// <summary>
        /// Рендер прямоугольника частицы
        /// </summary>
        public virtual void RenderRectangle(float timeIndex)
        {
            float u1 = textureUV.x / 16f;
            float v1 = textureUV.y / 16f;
            float u2 = u1 + .0624375f;
            float v2 = v1 + .0624375f;
            float scale = particleScale * .1f;

            Render(scale, u1, v1, u2, v2);

            //GLRender.Texture2DDisable();
            //GLRender.Rectangle(-1, -2, 1, 0, color);
            //GLRender.Texture2DEnable();
        }

        protected void Render(float scale, float u1, float v1, float u2, float v2)
        {
            GLRender.Color(color);
            GLRender.Scale(scale, -scale, scale);
            GLRender.Rectangle(-1, -2, 1, 0, u1, v1, u2, v2);
        }

        public override string ToString()
        {
            return GetType().AssemblyQualifiedName + " " + Position.ToString() + " age:" + particleAge.ToString();
        }
    }
}
