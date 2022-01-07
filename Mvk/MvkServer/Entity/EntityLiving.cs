
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
            Mov.Update();

            float h = Mov.Horizontal() * .43f;
            float v = Mov.Vertical() * .43f;
            float j = Mov.Height() * .75f;

            if (Mov.Sprinting.Value > 0 && v > 0)
            {
                // v *= (1 + 1.78f * Mov.Sprinting.Value); // 100км/ч = 55,6 блока в секунду
                // 5.47 = (2.78 / .43) - 1; находим скорость 100 км/ч с ускорением, при ходьбе 15,5 км/ч (4,3 м/с)
                //v *= (1 + 5.47f * Mov.Sprinting.Value); // 100км/ч
                v *= (1 + 0.3f * Mov.Sprinting.Value); // 20.2км/ч = 11,2 блока в / сек (5,6 м/с)
            }
            MotionAngle(j, v, h);
        }

        protected void MotionAngle(float j, float v, float h)
        {
            
            if (j != 0 || v != 0 || h != 0)
            {
                vec3 motion;
                motion.y = j;
                motion.x = glm.sin(RotationYaw + 1.570796f) * h;
                motion.z = glm.cos(RotationYaw + 1.570796f) * h;
                motion.x -= glm.sin(RotationYaw) * v;
                motion.z -= glm.cos(RotationYaw) * v;
                Motion = motion;
            }
            else
            {
                Motion = new vec3(0);
            }
            
        }
    }
}
