
using MvkServer.Glm;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности, отвечает за движение вращение и прочее
    /// </summary>
    public class EntityLiving : EntityBase
    {
        /// <summary>
        /// Размер
        /// </summary>
        //public HitBoxSize Size { get; protected set; } = new HitBoxSize();
        /// <summary>
        /// Режим перемещения
        /// </summary>
       // public VEMoving Mode { get; protected set; } = VEMoving.Survival;
        /// <summary>
        /// Объект скоростей
        /// </summary>
       // protected EntitySpeed speed = new EntitySpeed();
        /// <summary>
        /// Ключевой объект перемещения движения
        /// </summary>
        public Moving Mov { get; protected set; } = new Moving();

        /// <summary>
        /// Результат сидеть
        /// </summary>
        //protected EnumSneaking sneaking = EnumSneaking.DonSit;

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();

            float h = Mov.Horizontal;
            float v = Mov.Vertical;
            float j = Mov.Height;
            if (Mov.Sprinting > 0 && v > 0)
            {
                v *= 10f;
            }
            MotionAngle(j, v, h);
        }

        protected void MotionAngle(float j, float v, float h)
        {
            vec3 motion;
            motion.y = j;
            motion.x = glm.sin(RotationYaw + 1.570796f) * h;
            motion.z = glm.cos(RotationYaw + 1.570796f) * h;
            motion.x -= glm.sin(RotationYaw) * v;
            motion.z -= glm.cos(RotationYaw) * v;
            Motion = motion;
        }
    }
}
