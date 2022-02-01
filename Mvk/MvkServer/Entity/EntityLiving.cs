using MvkServer.Entity.Player;
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
        /// Объект ввода кликов клавиатуры
        /// </summary>
        public EnumInput Input { get; protected set; } = EnumInput.None;
        /// <summary>
        /// Счётчик движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwing { get; protected set; } = 0;
        /// <summary>
        /// Движение из-за смещения
        /// </summary>
        public vec3 MotionPush { get; set; } = new vec3(0);

        /// <summary>
        /// Нужна ли амплитуда конечностей
        /// </summary>
        protected bool isLimbSwing = true;
        /// <summary>
        /// Скорость движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        private float limbSwingAmount = 0;
        /// <summary>
        /// Скорость движения на предыдущем тике. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        private float limbSwingAmountPrev = 0;
        /// <summary>
        /// Анимация движения руки 0..1
        /// </summary>
        public float swingProgress = 0;
        /// <summary>
        /// Анимация движения руки предыдущего такта
        /// </summary>
        private float swingProgressPrev = 0;
        /// <summary>
        /// Счётчик тактов для анимации руки 
        /// </summary>
        private int swingProgressInt = 0;
        /// <summary>
        /// Запущен ли счётчик анимации руки
        /// </summary>
        private bool isSwingInProgress = false;

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

        private vec3 motionDebug = new vec3(0);

        #region Input

        /// <summary>
        /// Нет нажатий
        /// </summary>
        public void InputNone() => Input = EnumInput.None;
        /// <summary>
        /// Добавить нажатие
        /// </summary>
        public void InputAdd(EnumInput input) => Input |= input;
        /// <summary>
        /// Убрать нажатие
        /// </summary>
        public void InputRemove(EnumInput input)
        {
            if (Input.HasFlag(input)) Input ^= input;
        }

        #endregion

        public override void Update()
        {
            base.Update();
            swingProgressPrev = swingProgress;

            vec3 motionPrev = Motion;

            bool isMotion = EntityUpdate();

            LivingUpdate();

            // Пометка что было какое-то движение, вращения, бег, сидеть и тп.
            if (isMotion || !motionPrev.Equals(Motion))
            {
                UpdateLiving();
            }

            // Для вращении головы
            HeadTurn(Motion);
        }

        protected bool EntityUpdate()
        {
            bool isMotion = false;

            // метод определения если есть ускорение и мы не на воде, определяем по нижниму блоку какой спавн частиц и спавним их
            // ... func_174830_Y

            // метод проверки нахождения по кализии в воде ли мы, и меняем статус IsWater
            // ... handleWaterMovement

            // определяем горим ли мы, и раз в секунду %20, наносим урон
            // ...

            // определяем в лаве ли мы по кализии
            // ... func_180799_ab

            // если мы ниже -64 по Y убиваем игрока
            // ... kill

            // Если был толчёк, мы его дабавляем и обнуляем
            if (!MotionPush.Equals(new vec3(0)))
            {
                Motion += MotionPush;
                MotionPush = new vec3(0);
            }

            // Обновить положение сидя
            if (!IsFlying && OnGround && Input.HasFlag(EnumInput.Down) && !IsSneaking)
            {
                // Только в выживании можно сесть
                IsSneaking = true;
                Sitting();
                isMotion = true;
            }
            // Если хотим встать
            if (!Input.HasFlag(EnumInput.Down) && IsSneaking)
            {
                // Проверка коллизии вверхней части при положении стоя
                Standing();
                // TODO:: хочется как-то ловить колизию положение встать в MoveCheckCollision
                if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position)))
                {
                    IsSneaking = false;
                    isMotion = true;
                }
                else
                {
                    Sitting();
                }
            }

            // Sprinting
            bool isSprinting = Input.HasFlag(EnumInput.Sprinting | EnumInput.Forward) && !IsSneaking;
            if (IsSprinting != isSprinting)
            {
                IsSprinting = isSprinting;
                isMotion = true;
            }

            // Jumping
            IsJumping = Input.HasFlag(EnumInput.Up);

            return isMotion;
        }

        protected void LivingUpdate()
        {
            // счётчик прыжка
            if (jumpTicks > 0) jumpTicks--;

            // Продумать перемещение по тактам, с параметром newPosRotationIncrements

            // Если нет перемещения по тактам, запускаем трение воздуха
            Motion = new vec3(Motion.x * .98f, Motion.y, Motion.z * .98f);

            // Если мелочь убираем
            Motion = new vec3(
                Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x,
                Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y,
                Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
            );

            // Если блокировка то блокируем кнопки и параметр прыжка
            // ... his.moveStrafing = 0.0F; this.moveForward = 0.0F;

            if (IsFlying)
            {
                //motion.y += vertical * Speed.Vertical * param;
                //        //motion.y *= study;
                float vertical = (Input.HasFlag(EnumInput.Up) ? 1f : 0f) - (Input.HasFlag(EnumInput.Down) ? 1f : 0f);
                float y = Motion.y;
                y += vertical * Speed.Vertical;
                Motion = new vec3(Motion.x, y, Motion.z);
                IsJumping = false;
            }

            // Прыжок, только выживание
            else if (IsJumping)
            {
                // для воды свои правила, плыть вверх
                //... updateAITick
                // для лавы свои
                //... func_180466_bG
                // Для прыжка надо стоять на земле, и чтоб счётик прыжка был = 0
                if (OnGround && jumpTicks == 0)
                {
                    Jump();
                    jumpTicks = 10;
                }
            } else
            {
                jumpTicks = 0;
            }

            float strafe = (Input.HasFlag(EnumInput.Right) ? 1f : 0) - (Input.HasFlag(EnumInput.Left) ? 1f : 0);
            float forward = (Input.HasFlag(EnumInput.Back) ? 1f : 0f) - (Input.HasFlag(EnumInput.Forward) ? 1f : 0f);

            if (IsFlying)
            {
                float y = Motion.y;
                MoveWithHeading(strafe, forward, .6f * Speed.Forward * (IsSprinting ? Speed.Sprinting : 1f));
                Motion = new vec3(Motion.x, y * .6f, Motion.z);
            }
            else
            {
                MoveWithHeading(strafe, forward, .02f);
            }
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        //public override void Update()
        //{
        //    base.Update();
        //    swingProgressPrev = swingProgress;

        //    EntityUpdate();

        //    bool isMotion = false;
        //    // счётчик прыжка
        //    if (jumpTicks > 0) jumpTicks--;

        //    Motion *= .98f; // надо ,98 но для того как в майне надо * ,9616 где-то не учёл
        //    // Если мелочь убираем
        //    Motion = new vec3(
        //        Mth.Abs(Motion.x) < 0.005f ? 0 : Motion.x, 
        //        Mth.Abs(Motion.y) < 0.005f ? 0 : Motion.y, 
        //        Mth.Abs(Motion.z) < 0.005f ? 0 : Motion.z
        //    );

        //    if (!IsFlying) Motion *= .9616f; // для того как в майне надо * ,9616 где-то не учёл

        //    //// Если мелочь убираем
        //    //if (Mth.Abs(Motion.x) < 0.005f) Motion = new vec3(0, Motion.y, Motion.z);
        //    //if (Mth.Abs(Motion.y) < 0.005f) Motion = new vec3(Motion.x, 0, Motion.z);
        //    //if (Mth.Abs(Motion.z) < 0.005f) Motion = new vec3(Motion.x, Motion.y, 0);

        //    // Обновить положение сидя
        //    if (!IsFlying && OnGround && Input.HasFlag(EnumInput.Down) && !IsSneaking)
        //    {
        //        // Только в выживании можно сесть
        //        IsSneaking = true;
        //        Sitting();
        //        isMotion = true;
        //    }

        //    // Sprinting
        //    bool isSprinting = Input.HasFlag(EnumInput.Sprinting | EnumInput.Forward) && !IsSneaking;
        //    if (IsSprinting != isSprinting)
        //    {
        //        IsSprinting = isSprinting;
        //        isMotion = true;
        //    }

        //    // Перемещение, определяем скорости
        //    float strafe = (Input.HasFlag(EnumInput.Right) ? 1f : 0) - (Input.HasFlag(EnumInput.Left) ? 1f : 0);
        //    float forward = (Input.HasFlag(EnumInput.Back) ? 1f : 0f) - (Input.HasFlag(EnumInput.Forward) ? 1f : 0f);
        //    float height = IsFlying ? ((Input.HasFlag(EnumInput.Up) ? 1f : 0f) - (Input.HasFlag(EnumInput.Down) ? 1f : 0f))
        //        : Input.HasFlag(EnumInput.Up) ? 1f : 0f;

            
        //    // Если был толчёк, мы его дабавляем и обнуляем
        //    if (!MotionPush.Equals(new vec3(0)))
        //    {
        //        Motion += MotionPush;// * .2f;
        //        MotionPush = new vec3(0);
        //    }

        //    if (IsFlying)
        //    {
        //        // Определение вертикального перемещения
        //        //motion.y += vertical * Speed.Vertical * param;
        //        //motion.y *= study;

        //        IsJumping = false;
        //    }
        //    else
        //    {
        //        // Определение прыжка и высотного Y значения
        //        IsJumping = Input.HasFlag(EnumInput.Up);
        //        float y = OnGround ? -.2f : Motion.y - .16f;
        //        //if (OnGround) motion.y = -0.2f;
        //        //else motion.y = Motion.y - .16f;
        //        Motion = new vec3(Motion.x, y, Motion.z);

        //    }

        //    // Прыжок, только выживание
        //    if (IsJumping)
        //    {
        //        // для воды свои правила, плыть вверх
        //        //...
        //        // Для прыжка надо стоять на земле, и чтоб счётик прыжка был = 0
        //        if (OnGround && jumpTicks == 0)
        //        {
        //            vec3 motionJump = Jump();
        //            //motion.x += motionJump.x;
        //            //motion.y = motionJump.y;
        //            //motion.z += motionJump.z;
        //            Motion = new vec3(Motion.x + motionJump.x, motionJump.y, Motion.z + motionJump.z);
        //            jumpTicks = 10;
        //        }
        //    }

        //    bool onGroundOld = OnGround;

        //    vec3 motion = MoveWithHeading(strafe, forward);//, height);
        //    if (onGroundOld != OnGround) isMotion = true;
        //    Motion = motion;

            

            
        //    // Коллизия перемещения
        //    //MoveCheckCollision(motion);
            

        //    // Если хотим встать
        //    if (!Input.HasFlag(EnumInput.Down) && IsSneaking)
        //    {
        //        // Проверка коллизии вверхней части при положении стоя
        //        Standing();
        //        // TODO:: хочется как-то ловить колизию положение встать в MoveCheckCollision
        //        if (NoClip || !World.Collision.IsCollisionBody(this, new vec3(Position + Motion)))
        //        {
        //            IsSneaking = false;
        //            isMotion = true;
        //        }
        //        else
        //        {
        //            Sitting();
        //        }
        //    }

        //    // Если мелочь убираем
        //    if (Mth.Abs(Motion.x) < 0.005f) Motion = new vec3(0, Motion.y, Motion.z);
        //    if (Mth.Abs(Motion.y) < 0.005f) Motion = new vec3(Motion.x, 0, Motion.z);
        //    if (Mth.Abs(Motion.z) < 0.005f) Motion = new vec3(Motion.x, Motion.y, 0);

        //    // Пометка что было какое-то движение, вращения, бег, сидеть и тп.
        //    if (isMotion || Motion.x != 0 || Motion.y != 0 || Motion.z != 0)
        //    {
        //        UpdateLiving();
        //    }

        //    // Для вращении головы
        //    HeadTurn(motion);
        //}

        /// <summary>
        /// Проверка колизии по вектору движения
        /// </summary>
        /// <param name="motion">вектор движения</param>
        public void UpMoveCheckCollision(vec3 motion)
        {
            MoveEntity(motion);
        }

        /// <summary>
        /// Обновление в каждом тике, если были требования по изминению позицыи, вращения, бег, сидеть и тп.
        /// </summary>
        protected virtual void UpdateLiving() { }// => SetPosition(Position + Motion);

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected virtual void HeadTurn(vec3 motion) { }

        /// <summary>
        /// Конвертация от направления движения в XYZ координаты с кооректировками скоростей
        /// </summary>
        protected void MoveWithHeading(float strafe, float forward, float jumpMovementFactor)
        {
            vec3 motion = new vec3();

            // делим на три части, вода, лава, остальное

            // расматриваем остальное
            {
                // Коэффициент трения блока
                float study = 0.91f; // для воздух
                if (OnGround) study = 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f

                float param = 0.16277136f / (study * study * study);

                // трение, по умолчанию параметр ускорения падения вниз 
                float friction = jumpMovementFactor;
                if (OnGround)
                {
                    // если на земле, определяем скорость, можно в отдельном методе, у каждого моба может быть свои параметры

                    float speed = Mth.Max(Speed.Strafe * Mth.Abs(strafe), Speed.Forward * Mth.Abs(forward));
                    if (IsSprinting && forward < 0 && !IsSneaking)
                    {
                        // Бег 
                        speed *= Speed.Sprinting;
                    }
                    else if (!IsFlying && (forward != 0 || strafe != 0) && IsSneaking)
                    {
                        // Крадёмся
                        speed *= Speed.Sneaking;
                    }

                    // корректировка скорости, с трением
                    friction = speed * param;
                }

                motion = MotionAngle(strafe, forward, friction);

                // Тут надо корректировать леcтницу, вверх вниз Motion.y
                // ...

                // Проверка столкновения
                MoveEntity(motion);

                motion = Motion;
                // если есть горизонтальное столкновение и это лестница, то 
                // ... Motion.y = 0.2f;

                // Параметр падение 
                motion.y -= .16f;

                motion.y *= .98f;
                motion.x *= study;
                motion.z *= study;
            }

            // Тут расчёт амплитуды движения
            // ... 

            Motion = motion;
        }

        /// <summary>
        /// Задать смещение, с обрезкой малых чисел
        /// </summary>
        //protected void SetMotion(vec3 motion)
        //{
        //    // Если мелочь убираем
        //    if (Mth.Abs(motion.x) < 0.005f) motion.x = 0;
        //    if (Mth.Abs(motion.y) < 0.005f) motion.y = 0;
        //    if (Mth.Abs(motion.z) < 0.005f) motion.z = 0;
        //    Motion = motion;
        //}

        /// <summary>
        /// Значения для првжка
        /// </summary>
        protected void Jump()
        {
            // Стартовое значение прыжка, чтоб на 6 так допрыгнут наивысшую точку в 2,5 блока
            vec3 motion = new vec3(0, .84f, 0);
            if (IsSprinting)
            {
                // Если прыжок с бегом, то скорость увеличивается
                motion.x += glm.sin(RotationYaw) * 0.4f;
                motion.z -= glm.cos(RotationYaw) * 0.4f;
            }
            Motion = new vec3(Motion.x + motion.x, motion.y, Motion.z + motion.z);
        }

        /// <summary>
        /// Определение вращения
        /// </summary>
        protected vec3 MotionAngle(float strafe, float forward, float friction)
        {
            vec3 motion = Motion;

            float sf = strafe * strafe + forward * forward;
            if (sf >= 0.0001f)
            {
                sf = Mth.Sqrt(sf);
                if (sf < 1f) sf = 1f;
                sf = friction / sf;
                strafe *= sf;
                forward *= sf;
                float yaw = GetRotationYaw();
                float ysin = glm.sin(yaw);
                float ycos = glm.cos(yaw);
                motion.x += ycos * strafe - ysin * forward;
                motion.z += ycos * forward + ysin * strafe;
            }
            return motion;
        }

        /// <summary>
        /// Получить градус поворота по Yaw
        /// </summary>
        protected virtual float GetRotationYaw() => RotationYaw;

        protected void UpPositionMotion()
        {
            motionDebug = Motion;
            SetPosition(Position + Motion);
        }

        /// <summary>
        /// Проверка перемещения со столкновением
        /// </summary>
        protected void MoveEntity(vec3 motion)
        {
            // Без проверки столкновения
            if (NoClip)
            {
                Motion = motion;
                UpPositionMotion();
            }

            AxisAlignedBB boundingBox = BoundingBox.Clone();
            AxisAlignedBB aabbEntity = boundingBox.Clone();
            List<AxisAlignedBB> aabbs;

            float x0 = motion.x;
            float y0 = motion.y;
            float z0 = motion.z;

            float x = x0;
            float y = y0;
            float z = z0;

            // Защита от падения с края блока если сидишь и являешься игроком
            if (OnGround && IsSneaking && this is EntityPlayer)
            {
                // TODO::2022-02-01 замечена бага, иногда падаешь! По Х на 50000
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

                float stepHeight = StepHeight;
                // Если сидим авто прыжок в двое ниже
                if (IsSneaking) stepHeight *= 0.5f;

                y = stepHeight;
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
                    SetPosition(Position + new vec3(0, y + stepHeight, 0));
                    y = 0;
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
                if (IsFlying)
                {
                    ModeSurvival();
                    fallDistance = 0f;
                }
                else if (fallDistance > 0f)
                {
                    // Упал
                    fallDistanceResult = fallDistance;
                    fallDistance = 0f;
                }
            }

            Motion = new vec3(x0 != x ? 0 : x, y, z0 != z ? 0 : z);
            UpPositionMotion();
        }

        /// <summary>
        /// Скорость движения для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public float GetLimbSwingAmountFrame(float timeIndex)
        {
            if (timeIndex == 1.0f) limbSwingAmountPrev = limbSwingAmount;
            if (limbSwingAmount.Equals(limbSwingAmountPrev)) return limbSwingAmount;
            return limbSwingAmountPrev + (limbSwingAmount - limbSwingAmountPrev) * timeIndex;
        }

        /// <summary>
        /// Получить анимацию руки для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public virtual float GetSwingProgressFrame(float timeIndex)
        {
            if (isSwingInProgress)
            {
                if (timeIndex == 1.0f) swingProgressPrev = swingProgress;
                if (swingProgress.Equals(swingProgressPrev)) return swingProgress;
                return swingProgressPrev + (swingProgress - swingProgressPrev) * timeIndex;
            }
            return 0;
        }

        /// <summary>
        /// Расчёт амплитуды конечностей, при движении
        /// </summary>
        protected void UpLimbSwing()
        {
            if (isLimbSwing)
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
        }

        /// <summary>
        /// Скакой скоростью анимируется удар рукой, в тактах, менять можно от инструмента, чар и навыков
        /// </summary>
        private int GetArmSwingAnimationEnd() => 6; 

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public void SwingItem()
        {
            if (!isSwingInProgress || swingProgressInt >= GetArmSwingAnimationEnd() / 2 || swingProgressInt < 0)
            {
                swingProgressInt = -1;
                isSwingInProgress = true;

                //if (this.worldObj instanceof WorldServer)
                //{
                //    ((WorldServer)this.worldObj).getEntityTracker().sendToAllTrackingEntity(this, new S0BPacketAnimation(this, 0));
                //}
            }
        }

        /// <summary>
        /// Обновляет счетчики прогресса взмаха руки и прогресс анимации. 
        /// </summary>
        protected void UpdateArmSwingProgress()
        {
            int asa = GetArmSwingAnimationEnd();

            if (isSwingInProgress)
            {
                swingProgressInt++;
                if (swingProgressInt >= asa)
                {
                    swingProgressInt = 0;
                    isSwingInProgress = false;
                }
            }
            else
            {
                swingProgressInt = 0;
            }

            swingProgress = (float)swingProgressInt / (float)asa;
        }

        public override string ToString()
        {
            vec3 m = motionDebug;
            m.y = 0;
            vec3 my = new vec3(0, motionDebug.y, 0);

            return string.Format("XYZ {7} ch:{12}\r\n{0:0.000} | {13:0.000} м/c\r\nyaw:{8:0.00} H:{9:0.00} pitch:{10:0.00} \r\n{1}{2}{6}{4} boom:{5:0.00}\r\nMotion:{3}\r\n{11}",
                glm.distance(m) * 10f, // 0
                OnGround ? "__" : "", // 1
                IsSprinting ? "[Sp]" : "", // 2
                Motion, // 3
                IsJumping ? "[J]" : "", // 4
                fallDistanceResult, // 5
                IsSneaking ? "[Sn]" : "", // 6
                Position, // 7
                glm.degrees(RotationYaw), // 8
                0,//glm.degrees(RotationYawHead), // 9
                glm.degrees(RotationPitch), // 10
                IsCollidedHorizontally, // 11
                GetChunkPos(), // 12
                glm.distance(my) * 10f
                );
        }
    }
}
