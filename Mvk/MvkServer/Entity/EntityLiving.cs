
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности, отвечает за движение вращение и прочее
    /// </summary>
    public abstract class EntityLiving : EntityBase
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
        /// Любое изменение в движении
        /// </summary>
        protected bool isMotion = true;
        /// <summary>
        /// Было ли изменение в хитбоксе
        /// </summary>
        protected bool isMotionHitbox = false;
        /// <summary>
        /// Количество тактов для запрета прыжка
        /// </summary>
        protected int jumpTicks = 0;
        /// <summary>
        /// Результат сидеть
        /// </summary>
        //protected EnumSneaking sneaking = EnumSneaking.DonSit;
        
        private string strHVJ = "";
        private float debugMaxJump1 = 0;
        private float debugMaxJump2 = 0;
        private vec3 motion;

        
        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            Mov.Update();
            if (jumpTicks > 0) jumpTicks--;

            float h = Mov.Horizontal() * .43f;
            float v = Mov.Vertical() * .43f;
            float j = Mov.Up.Value * .75f;// Mov.Height() * .75f;

            // Sprinting
            IsSprinting = Mov.Sprinting.Value > 0;

            // Sneaking
            if (!IsFlying && OnGround)
            {
                if (Mov.Down.GetBegin() && sneaking != EnumSneaking.Sit)
                {
                    sneaking = EnumSneaking.Sit;
                    SetHeightEyes(2.7f, 2.5f);
                    isMotionHitbox = true;
                    isMotion = true;
                }
                else if (Mov.Down.GetEnd() && sneaking == EnumSneaking.Sit)
                {
                    sneaking = EnumSneaking.DonSit;
                    SetHeightEyes(3.6f, 3.4f);
                    isMotionHitbox = true;
                    isMotion = true;
                }
            }

            // Определение скорости
            if (IsSprinting && v > 0 && sneaking == EnumSneaking.DonSit)
            {
                // v *= (1 + 1.78f * Mov.Sprinting.Value); // 100км/ч = 55,6 блока в секунду
                // 5.47 = (2.78 / .43) - 1; находим скорость 100 км/ч с ускорением, при ходьбе 15,5 км/ч (4,3 м/с)
                //v *= (1 + 5.47f * Mov.Sprinting.Value); // 100км/ч
                v *= (1 + 0.3f * Mov.Sprinting.Value); // 20.2км/ч = 11,2 блока в / сек (5,6 м/с)

                if (IsJumping)
                {
                    // Если прыжок с бегом, то скорость увеличивается на 20%
                    v *= 1.2f;
                }
            }
            else if ((v != 0 || h != 0) && sneaking != EnumSneaking.DonSit)
            {
                // Скорость если крадёмся
                v *= .3f;
                h *= .3f;
            }
            

            strHVJ = string.Format("j:{0:0.00} v:{1:0.00} h:{2:0.00}", j, v, h);
            // Определение вращения
            MotionAngle(h, v, j);

            if (!IsFlying && !IsJumping && OnGround && j > 0)
            {
                // Прыжок
                if (jumpTicks == 0)
                {
                    debugMaxJump1 = Position.y;
                    debugMaxJump2 = Position.y;
                    IsJumping = true;
                    OnGround = false;
                    motion.y = 0.84f;
                    jumpTicks = 10;
                }
            }

            

            // Тут должна быть физика перемещения выживания
            CheckCollision();

            if (!IsFlying)
            {
                // Определение падения и прыжка
                MotionOnGround();

                // Проверка падения
                CheckOnGround();
            }

            
            
            // Фиксация вектора перемещения
            Motion = motion;

            // Пометка что было какое-то движение
            if (!isMotion && (Motion.x != 0 || Motion.y != 0 || Motion.z != 0))
            {
                isMotion = true;
            }
        }

        

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected void MotionAngle(float h, float v, float j)
        {
            motion = new vec3(0)
            {
                y = IsFlying ? j : (OnGround && j > 0 ? j : Motion.y)
            };
            if (h != 0)
            {
                motion.x = glm.sin(RotationYaw + 1.570796f) * h;
                motion.z = glm.cos(RotationYaw + 1.570796f) * h;
            }
            if (v != 0)
            {
                motion.x -= glm.sin(RotationYaw) * v;
                motion.z -= glm.cos(RotationYaw) * v;
            }
        }

        /// <summary>
        /// Определение падения и прыжка
        /// </summary>
        protected void MotionOnGround()
        {
            if (jumpTicks < 10 && !OnGround)
            {
                // Падаем
                motion.y -= .16f; // .08
                motion.y *= 0.98f;

                if (Motion.y >= 0 && motion.y < 0)
                {
                    debugMaxJump2 = Position.y;
                }
            }
        }

        /// <summary>
        /// Проверка падения
        /// </summary>
        protected void CheckOnGround()
        {
            vec3 pos = new vec3(Position + motion);
            HitBoxSizeUD hbs = new HitBoxSizeUD(pos, Hitbox);
            vec3i vd = hbs.GetVdi();
            vec3i vu = hbs.GetVui();

            if (OnGround && !World.Collision.IsCollisionDown(this, pos))
            {
                // Если стоим на земле но под нагами нет блоков, начинаем падать
                OnGround = false;
                motion.y = -0.02f;
            }
            else if(!OnGround && motion.y < 0 && World.Collision.IsCollisionDown(this, pos))
            {
                // Если мы падаем и под ногами появились блоки, прекращаем падать
                OnGround = true;
                Position = new vec3(Position.x, Mth.Floor(Position.y), Position.z);
                motion.y = 0;
                isMotion = true;
                IsJumping = false;
            }
        }

        /// <summary>
        /// Проверка коллизии
        /// </summary>
        protected void CheckCollision()
        {
            EnumCollisionBody cxz = World.Collision.IsCollisionBody(this, motion);
            if (cxz != EnumCollisionBody.None)
            {
                // проверяем авто прыжок тут
                if (OnGround && motion.y == 0
                    && World.Collision.IsCollisionBody(this, new vec3(motion.x, 1f, motion.z)) == EnumCollisionBody.None)
                {
                    // Если с прыжком нет колизии то надо прыгать!!!
                    // TODO:: реализовать авто прыжок мягким
                    Position = new vec3(Position.x, Mth.Floor(Position.y + 1f), Position.z);
                    isMotion = true;
                }
                else
                {
                    // одна из сторон не может проходить
                    EnumCollisionBody cx = World.Collision.IsCollisionBody(this, new vec3(motion.x, 0, 0));
                    EnumCollisionBody cz = World.Collision.IsCollisionBody(this, new vec3(0, 0, motion.z));
                    if (cx == EnumCollisionBody.None || (cx == EnumCollisionBody.None && cz == EnumCollisionBody.None))
                    {
                        motion.z = 0;
                        // Если обе стороны могут, это лож, будет глюк колизии угла, идём по этой стороне только
                        // TODO:: определить по какой стороне идём можно по Yaw углу
                        //SetPos(new vec3(Position.x + vec.x, Position.y, Position.z));
                    }
                    else if (cz == EnumCollisionBody.None)
                    {
                        motion.x = 0;
                        //SetPos(new vec3(Position.x, Position.y, Position.z + vec.z));
                    }
                }
            }
        }

        public override string ToString()
        {
            //string.Format("j: {0:0.0}{8}{9} v: {1:0.0} h: {2:0.0} {3}{5}{4}{6}{7}",
            //    j, v, h, OnGround ? "__" : "",
            //    HitBox.IsEyesWater ? "[E]" : "", HitBox.IsDownWater ? "[D]" : "", HitBox.IsLegsWater ? "[L]" : "", HitBox.Flow != Pole.Down ? HitBox.Flow.ToString() : "",
            //    IsSprinting ? "[Sp]" : "", IsSneaking ? "[Sn]" : "");

            return string.Format("{0} {1}{2}{5} jm:{6:0.00} {3} {7}\r\nPos Serv:{4}",
                strHVJ, OnGround ? "__" : "",
                IsSprinting ? "[Sp]" : "",
                Motion, Position, IsJumping ? "[J]" : "", debugMaxJump2 - debugMaxJump1, sneaking);
        }
    }
}
