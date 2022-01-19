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
        /// Счётчик движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwing { get; protected set; } = 0;
        /// <summary>
        /// Вращение головы
        /// </summary>
        public float RotationYawHead { get; protected set; }
        /// <summary>
        /// Вращение головы на предыдущем тике
        /// </summary>
        public float RotationYawHeadPrev { get; protected set; }

        /// <summary>
        /// Скорость движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        private float limbSwingAmount = 0;
        /// <summary>
        /// Скорость движения на предыдущем тике. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        private float limbSwingAmountPrev = 0;
        /// <summary>
        /// Любое изменение в движении
        /// </summary>
        protected bool isMotion = true;
        /// <summary>
        /// Количество тактов для запрета повторного прыжка
        /// </summary>
        private int jumpTicks = 0;
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
            RotationYawHeadPrev = RotationYawHead;

            // счётчик прыжка
            if (jumpTicks > 0) jumpTicks--;

            // Обновить положение сидя
            if (!IsFlying && OnGround && Mov.Down && !IsSneaking)
            {
                // Только в выживании можно сесть
                IsSneaking = true;
                Sitting();
                isMotion = true;
            }

            // Sprinting
            bool isSprinting = Mov.Sprinting && Mov.Forward && !IsSneaking;
            if (IsSprinting != isSprinting)
            {
                IsSprinting = isSprinting;
                isMotion = true;
            }

            // Перемещение, определяем скорости
            vec3 motion = MoveWithHeading(
                Mov.Strafe() * Speed.Strafe,
                Mov.ForwardAndBack() * Speed.Forward,
                IsFlying ? Mov.Height() * Speed.Vertical : Mov.Up ? 1f : 0
            );

            // Коллизия перемещения
            MoveCheckCollision(motion);

            // Если хотим встать
            if (!Mov.Down && IsSneaking)
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
                vec3 pos = Position + Motion;
                SetPosition(pos);
            }

            // Расчёт амплитуды конечностей, при движении
            UpLimbSwing();
            // Для вращении головы
            HeadTurn(motion);
        }

        

        /// <summary>
        /// Конвертация от направления движения в XYZ координаты с кооректировками скоростей
        /// </summary>
        protected vec3 MoveWithHeading(float strafe, float forward, float vertical)
        {
            // Ускорение или крастись
            if (IsSprinting && forward < 0 && !IsSneaking)
            {
                // Бег 
                forward *= Speed.Sprinting;
            }
            else if (!IsFlying && (forward != 0 || strafe != 0) && IsSneaking)
            {
                // Крадёмся
                forward *= .3f;
                strafe *= .3f;
            }

            strHVJ = string.Format("strafe:{0:0.00} forward:{1:0.00} height:{2:0.00}", strafe, forward, vertical);

            // Конвертация из направлений в xyz
            vec3 motion = MotionAngle(strafe, forward);

            // Определение прыжка и высотного Y значения
            if (IsFlying)
            {
                motion.y = vertical;
                IsJumping = false;
            }
            else
            {
                IsJumping = vertical == 1f;
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
                //float ysin = glm.sin(RotationYaw);
                //float ycos = glm.cos(RotationYaw);
                float ysin = glm.sin(RotationYawHead);
                float ycos = glm.cos(RotationYawHead);
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
            // TODO:: иногда на отрицательных проходит стенку колизия!!! 
            AxisAlignedBB boundingBox = BoundingBox.Expand(new vec3(0.001f, 0, 0.001f));

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

        /// <summary>
        /// Скорость движения для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public float GetLimbSwingAmountFrame(float timeIndex)
        {
            if (timeIndex == 1.0f || limbSwingAmount.Equals(limbSwingAmountPrev))
            {
                return limbSwingAmount;
            }
            else
            {
                return limbSwingAmountPrev + (limbSwingAmount - limbSwingAmountPrev) * timeIndex;
            }
        }

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotationHead(float yawHead, float yawBody, float pitch)
        {
            //SetRotation(yaw, pitch);
            RotationYawHead = yawHead;
            RotationYaw = yawBody;
            RotationPitch = pitch;
            CheckRotation();
        }

        /// <summary>
        /// Проверить градусы
        /// </summary>
        protected override void CheckRotation()
        {
            base.CheckRotation();
            while (RotationYawHead - RotationYawHeadPrev < -glm.pi) RotationYawHeadPrev -= glm.pi360;
            while (RotationYawHead - RotationYawHeadPrev >= glm.pi) RotationYawHeadPrev += glm.pi360;
        }

        /// <summary>
        /// Получить вектор направления камеры от головы
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex) => GetLookFrame(RotationYawHead, RotationYawHeadPrev, timeIndex);

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected void HeadTurn(vec3 motion)
        {
            float yawOffset;
            // Определяем двигается ли сущность
            bool movingForward = Mov.Forward || Mov.Back || Mov.Left || Mov.Right;
            if (movingForward)
            {
                // Определяем угол направления в зависимости куда движемся
                yawOffset = glm.atan2(motion.z, motion.x);
                if (Mov.Forward) yawOffset += glm.pi90;
                else if (Mov.Back) yawOffset -= glm.pi90;
                else if (Mov.Left) yawOffset += glm.pi135;
                else if (Mov.Right) yawOffset += glm.pi45;

                // Плавность поворота тела когда перемещаемся, до 15 градусов за такт
                RotationYaw = UpdateRotation(RotationYaw, yawOffset, .26f) % glm.pi360;
            }
            else
            {
                // Если не движется берём угол головы
                yawOffset = RotationYawHead;
            
                // Если не перемещаемся находим дельту поворота между телом и головой
                float delta = RotationYawHead - RotationYaw;
                while (delta < -glm.pi) delta += glm.pi360;
                while (delta >= glm.pi) delta -= glm.pi360;

                // Смещаем тело если дельта выше 60 градусов
                float angleR = 1.05f; // 60гр
                if (delta > angleR) RotationYaw = (RotationYaw + delta - angleR) % glm.pi360;
                else if (delta < -angleR) RotationYaw = (RotationYaw + delta + angleR) % glm.pi360;
            }
        }

        /// <summary>
        /// Расчёт амплитуды конечностей, при движении
        /// </summary>
        protected void UpLimbSwing()
        {
            limbSwingAmountPrev = limbSwingAmount;
            float xx = Position.x - PositionPrev.x;
            float zz = Position.z - PositionPrev.z;
            float xxzz = xx * xx + zz * zz;
            float xz = Mth.Sqrt(xxzz) * 2.0f;
            if (xz > 1.0f) xz = 1.0f;
            limbSwingAmount += (xz - limbSwingAmount) * 0.4f;
            LimbSwing += limbSwingAmount;
        }

        /// <summary>
        /// Обновить поворот с ограниченым смещением
        /// </summary>
        /// <param name="angle">Входящий угол в радианах</param>
        /// <param name="angleOffset">Смещение угла в радианах</param>
        /// <param name="increment">Увеличение в радианах</param>
        /// <returns>Новое значение</returns>
        private float UpdateRotation(float angle, float angleOffset, float increment)
        {
            float offset = glm.wrapAngleToPi(angleOffset - angle);

            if (offset > increment) offset = increment;
            if (offset < -increment) offset = -increment;

            return angle + offset;
        }


        public override string ToString()
        {
            return string.Format("XYZ {7}\r\nyaw:{8:0.00} H:{9:0.00} pitch:{10:0.00} \r\n{1}{2}{6}{4} boom:{5:0.00}\r\nMotion:{3}\r\nL:{11:0.000}",
                strHVJ, // 0
                OnGround ? "__" : "", // 1
                IsSprinting ? "[Sp]" : "", // 2
                Motion, // 3
                IsJumping ? "[J]" : "", // 4
                fallDistanceResult, // 5
                IsSneaking ? "[Sn]" : "", // 6
                Position, // 7
                glm.degrees(RotationYaw), // 8
                glm.degrees(RotationYawHead), // 9
                glm.degrees(RotationPitch), // 10
                limbSwingAmount // 11
                );
        }
    }
}
