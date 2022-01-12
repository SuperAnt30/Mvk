using MvkServer.Glm;
using MvkServer.Util;
using System.Collections.Generic;

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
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        protected int jumpTicks = 0;
        
        private string strHVJ = "";
        /// <summary>
        /// Дистанция падения
        /// </summary>
        private float fallDistance = 0;
        /// <summary>
        /// Дистанция прыжка вверх
        /// </summary>
        private float jumpDistance = 0;

        private vec3 motion;

        
        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            Mov.Update();
            if (jumpTicks > 0) jumpTicks--;

            float strafe = Mov.Strafe();
            float forward = Mov.ForwardAndBack();
            float height = IsFlying ? Mov.Height() : Mov.Up.Value > 0 ? 1f : 0;

            // Sprinting
            IsSprinting = Mov.Sprinting.Value > 0;

            // Обновить положение сидя
            if (!IsFlying && OnGround && Mov.Down.GetBegin() && !IsSneaking)
            {
                // Только в выживании можно сесть
                IsSneaking = true;
                SetSize(2.7f, 2.5f);
                isMotionHitbox = true;
                isMotion = true;
            }

            // Перемещение, определяем скорости
            vec3 motion = MoveWithHeading(strafe, forward, height);

            // Коллизия перемещения
            MoveCheckCollision(motion);

            // Если хотим встать
            if ((Mov.Down.GetEnd() || Mov.Down.IsZero()) && IsSneaking)
            {
                // Проверка коллизии вверхней части при положении стоя
                SetSize(3.6f, 3.4f);
                if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position + Motion)))
                {
                    IsSneaking = false;
                    isMotionHitbox = true;
                    isMotion = true;
                }
                else
                {
                    SetSize(2.7f, 2.5f);
                }
            }

            // Пометка что было какое-то движение
            if (!isMotion && (Motion.x != 0 || Motion.y != 0 || Motion.z != 0))
            {
                isMotion = true;
            }
        }

        /// <summary>
        /// Перемещение, определяем скорости
        /// </summary>
        protected vec3 MoveWithHeading(float strafe, float forward, float height)
        {
            if (IsFlying)
            {
                strafe *= .43f;
                forward *= 1.1f;
                height *= .75f;
            } else
            {
                strafe *= .43f;
                forward *= .43f;
            }

            // == Определение скорости
            if (IsSprinting && forward < 0 && !IsSneaking)
            {
                // Бег 
                if (IsFlying)
                {
                    forward *= 1.0f + Mov.Sprinting.Value;
                } else
                {
                    forward *= 1.0f + 0.3f * Mov.Sprinting.Value;
                    if (IsJumping)
                    {
                        // Если прыжок с бегом, то скорость увеличивается на 20%
                        forward *= 1.2f;
                    }
                }
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && IsSneaking)
            {
                // Скорость если крадёмся
                forward *= .3f;
                strafe *= .3f;
            }

            strHVJ = string.Format("strafe:{0:0.00} forward:{1:0.00} height:{2:0.00}", strafe, forward, height);
            motion = MotionAngle(strafe, forward, height);

            motion.x *= .98f;
            motion.z *= .98f;

            // Проверка начала прыжка
            if (!IsFlying)
            {
                if (!IsJumping && OnGround && motion.y > 0)
                {
                    // Прыжок
                    if (jumpTicks == 0)
                    {
                        IsJumping = true;
                        OnGround = false;
                        motion.y = 0.84f;
                        jumpDistance = motion.y;
                        jumpTicks = 10;
                    }
                    else
                    {
                        // отмена прыжка
                        motion.y = 0;
                    }
                }

                // Расчёт падения
                if (jumpTicks < 10 && !OnGround)
                {
                    // Падаем
                    float fall = motion.y;
                    motion.y -= .16f; // .08
                    motion.y *= .98f;
                    if (motion.y > 0)
                    {
                        jumpDistance += motion.y;
                    }
                    else if (fall >= 0 && motion.y < 0)
                    {
                        fallDistance = 0;
                    }
                }
            }

            return motion;
        }

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected vec3 MotionAngle(float strafe, float forward, float height)
        {
            motion = new vec3(0)
            {
                y = IsFlying ? height : (OnGround && height > 0 ? height : Motion.y)
            };

            if (strafe != 0 || forward != 0)
            {
                float ysin = glm.sin(RotationYaw);
                float ycos = glm.cos(RotationYaw);
                motion.x = ycos * strafe - ysin * forward;
                motion.z = ycos * forward + ysin * strafe;
            }
            return motion;
        }

        /// <summary>
        /// Проверка перемещения со столкновением
        /// </summary>
        protected void MoveCheckCollision(vec3 motion)
        {
            // Без проверки столкновения
            if (NoClip)
            {
                Motion = motion;
                return;
            }

            // Проверка на начало падения
            bool isCheckFall = true;

            AxisAlignedBB aabbEntity = BoundingBox.Clone();
            List<AxisAlignedBB> aabbs = World.Collision.GetCollidingBoundingBoxes(BoundingBox.AddCoord(motion));

            float x = motion.x;
            float y = motion.y;
            float z = motion.z;

            // Находим смещение по Y
            foreach(AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
            aabbEntity = aabbEntity.Offset(new vec3(0, y, 0));

            // счётчик дистанции падения
            if (y < 0) fallDistance -= y;

            // Не прыгаем (момент взлёта)
            bool isNotJump = OnGround || motion.y != y && motion.y < 0f;

            if (!OnGround && motion.y < y)
            {
                // Окончание падения
                OnGround = true;
                isMotion = true;
                IsJumping = false;
                // Если закончили падать, нет смысла проверять начало падения
                isCheckFall = false;
            }

            // Находим смещение по X
            foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
            aabbEntity = aabbEntity.Offset(new vec3(x, 0, 0));

            // Находим смещение по Z
            foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
            aabbEntity = aabbEntity.Offset(new vec3(0, 0, z));

            // Запуск проверки авто прыжка
            if (MvkGlobal.AUTO_JUMP_STEP_HEIGHT > 0f && isCheckFall && isNotJump && (motion.x != x || motion.z != z))
            {
                // Какую высоту можно прыгать
                float stepHeight = MvkGlobal.AUTO_JUMP_STEP_HEIGHT;
                // Кэш для откада, если авто прыжок не допустим
                vec3 monCache = new vec3(x, y, z);

                y = stepHeight;
                aabbs = World.Collision.GetCollidingBoundingBoxes(BoundingBox.AddCoord(new vec3(motion.x, y, motion.z)));
                AxisAlignedBB aabbEntity2 = BoundingBox.Clone();
                AxisAlignedBB aabb = aabbEntity2.AddCoord(new vec3(motion.x, 0, motion.z));

                // Находим смещение по Y
                float y2 = y;
                foreach (AxisAlignedBB axis in aabbs) y2 = axis.CalculateYOffset(aabb, y2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, y2, 0));

                // Находим смещение по X
                float x2 = motion.x;
                foreach (AxisAlignedBB axis in aabbs) x2 = axis.CalculateXOffset(aabbEntity2, x2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(x2, 0, 0));

                // Находим смещение по Z
                float z2 = motion.z;
                foreach (AxisAlignedBB axis in aabbs) z2 = axis.CalculateZOffset(aabbEntity2, z2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, 0, z2));

                AxisAlignedBB aabbEntity3 = BoundingBox.Clone();

                // Находим смещение по Y
                float y3 = y;
                foreach (AxisAlignedBB axis in aabbs) y3 = axis.CalculateYOffset(aabbEntity3, y3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(0, y3, 0));

                // Находим смещение по X
                float x3 = motion.x;
                foreach (AxisAlignedBB axis in aabbs) x3 = axis.CalculateXOffset(aabbEntity3, x3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(x3, 0, 0));

                // Находим смещение по Z
                float z3 = motion.z;
                foreach (AxisAlignedBB axis in aabbs) z3 = axis.CalculateZOffset(aabbEntity3, z3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(0, 0, z3));

                if (x2 * x2 + z2 * z2 > x3 * x3 + z3 * z3)
                {
                    x = x2;
                    z = z2;
                    aabbEntity = aabbEntity2;
                } else
                {
                    x = x3;
                    z = z3;
                    aabbEntity = aabbEntity3;
                }
                y = -stepHeight;

                // Находим итоговое смещение по Y
                foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);

                if (monCache.x * monCache.x + monCache.z * monCache.z >= x * x + z * z)
                {
                    // Нет авто прыжка, откатываем значение обратно
                    x = monCache.x;
                    y = monCache.y;
                    z = monCache.z;
                }
                else
                {
                    // Авто прыжок
                    SetPosition(new vec3(Position.x, Position.y + stepHeight + y, Position.z));
                    // убираем, так как в следующем такте остаётся в Motion.y значение, и срабатывает падение
                    y = 0;
                    isMotion = true;
                    // Если авто прыжок, нет смысла проверять падение
                    isCheckFall = false;
                }
            }
            
            // Этап проверки начала падения
            if (isCheckFall && OnGround)
            {
                // Глубина где проверяем падение и фиксируем начало падения
                float fall = .02f;
                if (!World.Collision.IsCollisionDown(this, new vec3(Position + new vec3(x, y - fall, z))))
                {
                    // Если стоим на земле но под нагами нет блоков, начинаем падать
                    OnGround = false;
                    y = -fall;
                    fallDistance = fall;
                }
            }

            Motion = new vec3(x, y, z);
        }

        public override string ToString()
        {
            //string.Format("j: {0:0.0}{8}{9} v: {1:0.0} h: {2:0.0} {3}{5}{4}{6}{7}",
            //    j, v, h, OnGround ? "__" : "",
            //    HitBox.IsEyesWater ? "[E]" : "", HitBox.IsDownWater ? "[D]" : "", HitBox.IsLegsWater ? "[L]" : "", HitBox.Flow != Pole.Down ? HitBox.Flow.ToString() : "",
            //    IsSprinting ? "[Sp]" : "", IsSneaking ? "[Sn]" : "");

            return string.Format("{0}\r\n{1}{2}{5}{7} jm:{6:0.00} boom:{8:0.00} {3} \r\nPos Serv:{4}",
                strHVJ, OnGround ? "__" : "",
                IsSprinting ? "[Sp]" : "",
                Motion, Position, IsJumping ? "[J]" : "", jumpDistance, IsSneaking ? "[Sn]" : ""
                , fallDistance);
        }
    }
}
