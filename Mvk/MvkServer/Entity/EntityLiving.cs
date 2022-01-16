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
        /// Ключевой объект перемещения движения
        /// </summary>
        public Moving Mov { get; protected set; } = new Moving();
        /// <summary>
        /// Любое изменение в движении
        /// </summary>
        protected bool isMotion = true;
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        protected int jumpTicks = 0;
        /// <summary>
        /// Дистанция падения
        /// </summary>
        private float fallDistance = 0;
        /// <summary>
        /// Результат падения, отладка!!!
        /// </summary>
        private float fallDistanceResult = 0;
        /// <summary>
        /// Отладка!!!
        /// </summary>
        private string strHVJ = "";

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
            bool isSprinting = Mov.Sprinting.Value > 0 && !Mov.Forward.IsZero();
            if (IsSprinting != isSprinting)
            {
                IsSprinting = isSprinting;
                isMotion = true;
            }

            // Обновить положение сидя
            if (!IsFlying && OnGround && Mov.Down.GetBegin() && !IsSneaking)
            {
                // Только в выживании можно сесть
                IsSneaking = true;
                Sitting();
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
                Standing();
                // TODO:: хочется как-то ловить колизию положение встать в MoveCheckCollision
                if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position + Motion)))
                {
                    IsSneaking = false;
                    isMotion = true;
                }
                else
                {
                    Sitting();
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
            // Определение скорости направлений
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

            // Ускорение или крастись
            if (IsSprinting && forward < 0 && !IsSneaking)
            {
                // Бег 
                if (IsFlying)
                {
                    forward *= 1.0f + Mov.Sprinting.Value;
                } else
                {
                    forward *= 1.0f + 0.3f * Mov.Sprinting.Value;
                }
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && IsSneaking)
            {
                // Скорость если крадёмся
                forward *= .3f;
                strafe *= .3f;
            }

            strHVJ = string.Format("strafe:{0:0.00} forward:{1:0.00} height:{2:0.00}", strafe, forward, height);

            // Конвертация из направлений в xyz
            vec3 motion = MotionAngle(strafe, forward);//, height);

            // Определение прыжка и высотного Y значения
            if (IsFlying)
            {
                motion.y = height;
                IsJumping = false;
            }
            else
            {
                IsJumping = height == 1f;
                if (OnGround) motion.y = -0.2f;
                else motion.y = Motion.y - .16f;
            }

            // Если мелось убираем
            if (Mth.Abs(motion.x) < 0.005f) motion.x = 0;
            if (Mth.Abs(motion.y) < 0.005f) motion.y = 0;
            if (Mth.Abs(motion.z) < 0.005f) motion.z = 0;

            // Мягкость
            motion.x *= .5f;
            motion.y *= .98f;
            motion.z *= .5f;

            // Прыжок, только выживание
            if (IsJumping)
            {
                // для воды свои правила, плыть вверх
                //...
                // Для прыжка надо стоять на земле, и что счётик прыжка был = 0
                if (OnGround && jumpTicks == 0)
                {
                    vec3 motionJump = Jump();
                    motion.x += motionJump.x;
                    motion.y = motionJump.y;
                    motion.z += motionJump.z;
                    jumpTicks = 10;
                }
            }

            return motion;
        }

        /// <summary>
        /// Значения для првжка
        /// </summary>
        protected vec3 Jump()
        {
            // Стартовое значение прыжка, чтоб на 6 так допрыгнут наивысшую точку в 2,5 блока
            vec3 motion = new vec3(0, .84f, 0);
            if (IsSprinting)
            {
                // Если прыжок с бегом, то скорость увеличивается на 20%
                motion.x += glm.sin(RotationYaw) * 0.2f;
                motion.z -= glm.cos(RotationYaw) * 0.2f;
            }
            return motion;
        }

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected vec3 MotionAngle(float strafe, float forward)
        {
            vec3 motion = Motion;

            if (strafe != 0 || forward != 0)
            {
                float ysin = glm.sin(RotationYaw);
                float ycos = glm.cos(RotationYaw);
                motion.x += ycos * strafe - ysin * forward;
                motion.z += ycos * forward + ysin * strafe;
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

            // Расширение из-за огруглении float
            AxisAlignedBB boundingBox = BoundingBox.Expand(new vec3(0.00001f, 0, 0.00001f));

            AxisAlignedBB aabbEntity = boundingBox.Clone();
            List<AxisAlignedBB> aabbs;

            float x0 = motion.x;
            float y0 = motion.y;
            float z0 = motion.z;

            float x = x0;
            float y = y0;
            float z = z0;

            // Защита от падения с края блока если сидишь
            // TODO:: проверку добавть только для Игроков, не падать с края!!!
            if (OnGround && IsSneaking)
            {
                // Шаг проверки смещения
                float step = 0.05f;
                for (; x != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBox.Offset(new vec3(x, -1, 0))).Count == 0; x0 = x)
                {
                    if (x < step && x >= -step) x = 0f;
                    else if (x > 0f) x -= step;
                    else x += step;
                }
                for (; z != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBox.Offset(new vec3(0, -1, z))).Count == 0; z0 = z)
                {
                    if (z < step && z >= -step) z = 0f;
                    else if (z > 0f) z -= step;
                    else z += step;
                }
                for (; x != 0f && z0 != 0f && World.Collision.GetCollidingBoundingBoxes(boundingBox.Offset(new vec3(x0, -1, z0))).Count == 0; z0 = z)
                {
                    if (x < step && x >= -step) x = 0f;
                    else if (x > 0f) x -= step;
                    else x += step;
                    x0 = x;
                    if (z < step && z >= -step) z = 0f;
                    else if (z > 0f) z -= step;
                    else z += step;
                }
            }

            aabbs = World.Collision.GetCollidingBoundingBoxes(boundingBox.AddCoord(new vec3(x, y, z)));

            // Находим смещение по Y
            foreach (AxisAlignedBB axis in aabbs) y = axis.CalculateYOffset(aabbEntity, y);
            aabbEntity = aabbEntity.Offset(new vec3(0, y, 0));

            // Не прыгаем (момент взлёта)
            bool isNotJump = OnGround || motion.y != y && motion.y < 0f;

            // Находим смещение по X
            foreach (AxisAlignedBB axis in aabbs) x = axis.CalculateXOffset(aabbEntity, x);
            aabbEntity = aabbEntity.Offset(new vec3(x, 0, 0));

            // Находим смещение по Z
            foreach (AxisAlignedBB axis in aabbs) z = axis.CalculateZOffset(aabbEntity, z);
            aabbEntity = aabbEntity.Offset(new vec3(0, 0, z));

            // Запуск проверки авто прыжка
            if (StepHeight > 0f && isNotJump && (x0 != x || z0 != z))
            {
                // Кэш для откада, если авто прыжок не допустим
                vec3 monCache = new vec3(x, y, z);

                y = StepHeight;
                aabbs = World.Collision.GetCollidingBoundingBoxes(boundingBox.AddCoord(new vec3(x0, y, z0)));
                AxisAlignedBB aabbEntity2 = boundingBox.Clone();
                AxisAlignedBB aabb = aabbEntity2.AddCoord(new vec3(x0, 0, z0));

                // Находим смещение по Y
                float y2 = y;
                foreach (AxisAlignedBB axis in aabbs) y2 = axis.CalculateYOffset(aabb, y2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, y2, 0));

                // Находим смещение по X
                float x2 = x0;
                foreach (AxisAlignedBB axis in aabbs) x2 = axis.CalculateXOffset(aabbEntity2, x2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(x2, 0, 0));

                // Находим смещение по Z
                float z2 = z0;
                foreach (AxisAlignedBB axis in aabbs) z2 = axis.CalculateZOffset(aabbEntity2, z2);
                aabbEntity2 = aabbEntity2.Offset(new vec3(0, 0, z2));

                AxisAlignedBB aabbEntity3 = boundingBox.Clone();

                // Находим смещение по Y
                float y3 = y;
                foreach (AxisAlignedBB axis in aabbs) y3 = axis.CalculateYOffset(aabbEntity3, y3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(0, y3, 0));

                // Находим смещение по X
                float x3 = x0;
                foreach (AxisAlignedBB axis in aabbs) x3 = axis.CalculateXOffset(aabbEntity3, x3);
                aabbEntity3 = aabbEntity3.Offset(new vec3(x3, 0, 0));

                // Находим смещение по Z
                float z3 = z0;
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
                y = -StepHeight;

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
                    y += StepHeight;
                }
            }

            IsCollidedHorizontally = x0 != x || z0 != z;
            IsCollidedVertically = y0 != y;
            OnGround = IsCollidedVertically && y0 < 0.0f;
            IsCollided = IsCollidedHorizontally || IsCollidedVertically;

            // Определение дистанции падения, и фиксаия падения
            if (y < 0f) fallDistance -= y;
            if (OnGround)
            {
                if (fallDistance > 0f)
                {
                    // Упал
                    fallDistanceResult = fallDistance;
                    fallDistance = 0f;
                }
            }

            // Обновить значения перемещения для следующего такта
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
                Motion, Position, IsJumping ? "[J]" : "", fallDistanceResult, IsSneaking ? "[Sn]" : ""
                , fallDistance);
        }
    }
}
