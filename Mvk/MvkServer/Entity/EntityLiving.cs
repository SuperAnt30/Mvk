using MvkServer.Entity.Mob;
using MvkServer.Glm;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности, отвечает за движение вращение и прочее
    /// </summary>
    public abstract class EntityLiving : EntityLook
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
        /// Скорость движения. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        public float LimbSwingAmount { get; protected set; } = 0;
        /// <summary>
        /// Движение из-за смещения
        /// </summary>
        public vec3 MotionPush { get; set; } = new vec3(0);
        /// <summary>
        /// Бежим
        /// </summary>
        public bool IsSprinting { get; protected set; } = false;
        /// <summary>
        /// Результат сидеть
        /// </summary>
        public bool IsSneaking { get; protected set; } = false;
        /// <summary>
        /// Прыгаем
        /// </summary>
        public bool IsJumping { get; protected set; } = false;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как «мертвая», то есть иметь в мире труп.
        /// </summary>
        public int DeathTime { get; protected set; } = 0;
        /// <summary>
        /// Уровень здоровья
        /// </summary>
        public float Health { get; protected set; }
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как травмированная, то есть маргает красным
        /// </summary>
        public int DamageTime { get; protected set; } = 0;
        /// <summary>
        /// Значение поворота вокруг своей оси с сервера
        /// </summary>
        public float RotationYawServer { get; protected set; }
        /// <summary>
        /// Значение поворота вверх вниз с сервера
        /// </summary>
        public float RotationPitchServer { get; protected set; }
        /// <summary>
        /// Сколько тиков эта сущность пробежала с тех пор, как была жива 
        /// </summary>
        public int TicksExisted { get; private set; } = 0;
        /// <summary>
        /// Флаг спавна, пометка начального спавна игрока 
        /// ПОКА НЕ ЗНАЮ ЗАЧЕМ я это сделал!!!
        /// </summary>
        public bool FlagSpawn { get; set; } = false;
        /// <summary>
        /// Пометка что было движение и подобное для сервера, чтоб отправлять пакеты
        /// </summary>
        public EnumActionChanged ActionChanged { get; set; } = EnumActionChanged.None;
        /// <summary>
        /// Объект скорости сущности
        /// </summary>
        public EntitySpeed Speed { get; protected set; }

        /// <summary>
        /// Имя
        /// </summary>
        protected string name = "";
        /// <summary>
        /// Скорость движения на предыдущем тике. Влияет на то, где в данный момент находятся ноги и руки при качании. 
        /// </summary>
        protected float limbSwingAmountPrev = 0;
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
        protected float fallDistanceResult = 0;

        public EntityLiving(WorldBase world) : base(world)
        {
            Health = GetHelathMax();
            Standing();
            SpeedSurvival();
        }

        /// <summary>
        /// Получить название для рендеринга
        /// </summary>
        public override string GetName() => name == "" ? "entity." + Type.ToString() : name;

        /// <summary>
        /// Возвращает true, если эта вещь названа
        /// </summary>
        public override bool HasCustomName() => name != "";

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected virtual float GetHelathMax() => 10;

        #region Input

        /// <summary>
        /// Нет нажатий
        /// </summary>
        public virtual void InputNone() => Input = EnumInput.None;
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

        #region Action

        /// <summary>
        /// Нет действий
        /// </summary>
        public void ActionNone() => ActionChanged = EnumActionChanged.None;
        /// <summary>
        /// Добавить действие
        /// </summary>
        public void ActionAdd(EnumActionChanged input) => ActionChanged |= input;
        /// <summary>
        /// Убрать действие
        /// </summary>
        public void ActionRemove(EnumActionChanged input)
        {
            if (ActionChanged.HasFlag(input)) ActionChanged ^= input;
        }

        /// <summary>
        /// Задать действие для позиции
        /// </summary>
        protected override void ActionAddPosition() => ActionAdd(EnumActionChanged.Position);
        /// <summary>
        /// Задать действие для вращения
        /// </summary>
        protected override void ActionAddLook() => ActionAdd(EnumActionChanged.Look);

        #endregion

        /// <summary>
        /// Надо ли обрабатывать LivingUpdate, для мобов на сервере, и игроки у себя
        /// </summary>
        protected virtual bool IsLivingUpdate()
        {
            bool server = World is WorldServer;
            bool player = this is EntityChicken;
            return server && player;
        }

        public override void Update()
        {
            TicksExistedMore();

            base.Update();

            EntityUpdate();

            if (!IsDead)
            {
                if (IsLivingUpdate())
                {
                    // Если надо управление физики
                    LivingUpdate();
                }

                // Расчёт амплитуды движения 
                UpLimbSwing();
                // Просчёт взмаха руки
                UpdateArmSwingProgress();
            }
        }

        protected void EntityUpdate()
        {
            // метод определения если есть ускорение и мы не на воде, определяем по нижниму блоку какой спавн частиц и спавним их
            // ... func_174830_Y

            // метод проверки нахождения по кализии в воде ли мы, и меняем статус IsWater
            // ... handleWaterMovement

            // определяем горим ли мы, и раз в секунду %20, наносим урон
            // ...

            // определяем в лаве ли мы по кализии
            // ... func_180799_ab

            // Частички при беге блока
            ParticleBlockSprinting();

            // Было ли падение
            Fall();

            // если мы ниже -128 по Y убиваем игрока
            if (Position.y < -128) Kill();

            // если нет хп обновлям смертельную картинку
            if (Health <= 0f && DeathTime != -1) DeathUpdate();

            // Счётчик получения урона для анимации
            if (DamageTime > 0) DamageTime--;

            // Если был толчёк, мы его дабавляем и обнуляем
            if (!MotionPush.Equals(new vec3(0)))
            {
                vec3 motionPush = MotionPush;
                MotionPush = new vec3(0);
                // Защита от дабл прыжка, и если сущность летает, нет броска
                if (Motion.y > 0 || IsFlying) motionPush.y = 0;
                Motion += motionPush;
            }
        }

        /// <summary>
        /// Обновляет активное действие, и возращает strafe, forward, vertical через vec3
        /// </summary>
        /// <returns>strafe, forward</returns>
        protected vec2 UpdateEntityActionState()
        {
            float strafe = 0f;
            float forward = 0f;

            if (IsFlying)
            {
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
            }
            else
            {
                jumpTicks = 0;
            }

            strafe = (Input.HasFlag(EnumInput.Right) ? 1f : 0) - (Input.HasFlag(EnumInput.Left) ? 1f : 0);
            forward = (Input.HasFlag(EnumInput.Back) ? 1f : 0f) - (Input.HasFlag(EnumInput.Forward) ? 1f : 0f);


            // Обновить положение сидя
            bool isSneaking = IsSneaking;
            if (!IsFlying && OnGround && Input.HasFlag(EnumInput.Down) && !IsSneaking)
            {
                // Только в выживании можно сесть
                IsSneaking = true;
                Sitting();
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
                }
                else
                {
                    Sitting();
                }
            }
            if (isSneaking != IsSneaking)
            {
                ActionAdd(EnumActionChanged.IsSneaking);
            }

            // Sprinting
            bool isSprinting = Input.HasFlag(EnumInput.Sprinting | EnumInput.Forward) && !IsSneaking;
            if (IsSprinting != isSprinting)
            {
                IsSprinting = isSprinting;
                ActionAdd(EnumActionChanged.IsSprinting);
            }

            // Jumping
            IsJumping = Input.HasFlag(EnumInput.Up);

            return new vec2(strafe, forward);
        }

        /// <summary>
        /// Метод отвечает за жизнь сущности, точнее её управление, перемещения, мобы Ai
        /// должен работать у клиента для EntityPlayerSP и на сервере для мобов
        /// так же может работать у клиента для всех сущностей эффектов вне сервера.
        /// </summary>
        protected virtual void LivingUpdate()
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

            float strafe = 0f;
            float forward = 0f;

            if (!IsMovementBlocked())
            {
                // Если нет блокировки
                vec2 sf = UpdateEntityActionState();
                strafe = sf.x;
                forward = sf.y;
            }

           // if (Health <= 0) InputNone();

            // Тут правильнее сделать метод updateEntityActionState 
            // где SP присваивает strafe и forward
            // или сервер Ai для мобов

            if (IsFlying)
            {
                float y = Motion.y;
                MoveWithHeading(strafe, forward, .6f * Speed.Forward * (IsSprinting ? Speed.Sprinting : 1f));
                Motion = new vec3(Motion.x, y * .6f, Motion.z);
            }
            else
            {
                MoveWithHeading(strafe, forward, .04f);
            }
        }

        /// <summary>
        /// Проверка колизии по вектору движения
        /// </summary>
        /// <param name="motion">вектор движения</param>
        public void UpMoveCheckCollision(vec3 motion)
        {
            MoveEntity(motion);
        }

        /// <summary>
        /// Мертвые и спящие существа не могут двигаться
        /// </summary>
        protected bool IsMovementBlocked() => Health <= 0f;

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected virtual void HeadTurn() { }

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
                //float study = .954f;// 0.91f; // для воздух
                //if (OnGround) study = 0.739F;// 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f
                float study = 0.91f; // для воздух
                if (OnGround) study = 0.546f; // трение блока, определить на каком блоке стоим (.6f блок) * .91f

                //float param = 0.403583419f / (study * study * study);
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
                //motion.y -= .08f;

                motion.y *= .98f;
                motion.x *= study;
                motion.z *= study;
            }

            Motion = motion;
        }

        /// <summary>
        /// Значения для првжка
        /// </summary>
        protected void Jump()
        {
            // Стартовое значение прыжка, чтоб на 6 так допрыгнут наивысшую точку в 2,5 блока
            vec3 motion = new vec3(0, .84f, 0);
            //vec3 motion = new vec3(0, .42f, 0);
            if (IsSprinting)
            {
                // Если прыжок с бегом, то скорость увеличивается
                motion.x += glm.sin(RotationYaw) * 0.4f;
                motion.z -= glm.cos(RotationYaw) * 0.4f;
                //motion.x += glm.sin(RotationYaw) * 0.2f;
                //motion.z -= glm.cos(RotationYaw) * 0.2f;
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

        #region Frame

        /// <summary>
        /// Скорость движения для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public float GetLimbSwingAmountFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || LimbSwingAmount.Equals(limbSwingAmountPrev)) return LimbSwingAmount;
            return limbSwingAmountPrev + (LimbSwingAmount - limbSwingAmountPrev) * timeIndex;
        }

        /// <summary>
        /// Получить анимацию руки для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public virtual float GetSwingProgressFrame(float timeIndex)
        {
            if (isSwingInProgress)
            {
                if (timeIndex >= 1.0f || swingProgress.Equals(swingProgressPrev)) return swingProgress;
                return swingProgressPrev + (swingProgress - swingProgressPrev) * timeIndex;
            }
            return 0;
        }

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public virtual float GetEyeHeightFrame() => GetEyeHeight();

        #endregion

        /// <summary>
        /// Высота глаз
        /// </summary>
        public virtual float GetEyeHeight() => Height * 0.85f;

        /// <summary>
        /// Расчёт амплитуды конечностей, при движении
        /// </summary>
        protected void UpLimbSwing()
        {
            limbSwingAmountPrev = LimbSwingAmount;
            float xx = Position.x - PositionPrev.x;
            float zz = Position.z - PositionPrev.z;
            float xxzz = xx * xx + zz * zz;
            float xz = Mth.Sqrt(xxzz) * 1.4f;
            if (xz > 1.0f) xz = 1.0f;
            LimbSwingAmount += (xz - LimbSwingAmount) * 0.4f;
            LimbSwing += LimbSwingAmount;
        }

        /// <summary>
        /// Скакой скоростью анимируется удар рукой, в тактах, менять можно от инструмента, чар и навыков
        /// </summary>
        private int GetArmSwingAnimationEnd() => 6; 

        /// <summary>
        /// Размахивает предметом, который держит игрок
        /// </summary>
        public virtual void SwingItem()
        {
            if (!isSwingInProgress || swingProgressInt >= GetArmSwingAnimationEnd() / 2 || swingProgressInt < 0)
            {
                swingProgressInt = -1;
                isSwingInProgress = true;

                if (World is WorldServer)
                {
                    ((WorldServer)World).Tracker.SendToAllTrackingEntity(this, new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem));
                    //((WorldServer)World).Players.ResponsePacketAll(new PacketS0BAnimation(Id, PacketS0BAnimation.EnumAnimation.SwingItem), Id);
                }
            }
        }

        /// <summary>
        /// Обновляет счетчики прогресса взмаха руки и прогресс анимации. 
        /// </summary>
        protected void UpdateArmSwingProgress()
        {
            swingProgressPrev = swingProgress;

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

        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetMotionServer(vec3 pos, float yaw, float pitch, bool sneaking, bool onGround, bool sprinting)
        {
            if (IsSneaking != sneaking)
            {
                IsSneaking = sneaking;
                if (IsSneaking) Sitting(); else Standing();
            }
            IsSprinting = sprinting;
            RotationYawServer = yaw;
            RotationPitchServer = pitch;
            SetMotionServer(pos, onGround);
        }

        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public virtual void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            SetRotation(yaw, pitch);
            RotationYawServer = RotationYawPrev = RotationYaw;
            RotationPitchServer = RotationPitchPrev = RotationPitch;
            PositionServer = PositionPrev = LastTickPos = Position;
        }

        /// <summary>
        /// Дополнительное обновление сущности в клиентской части в зависимости от сущности
        /// </summary>
        protected virtual void UpdateEntityRotation() => SetRotation(RotationYawServer, RotationPitchServer);

        /// <summary>
        /// Управляет таймером смерти сущности, сферой опыта и созданием частиц
        /// </summary>
        protected void DeathUpdate()
        {
            DeathTime++;

            if (DeathTime >= 20)
            {
                for (int i = 0; i < 100; i++)
                {
                    World.SpawnParticle(EnumParticle.Test,
                    Position + new vec3(((float)rand.NextDouble() - .5f) * Width,
                    .1f,
                    ((float)rand.NextDouble() - .5f) * Width),
                    new vec3(0));
                }
                DeathTime = -1;
                SetDead();

                //Health = 20;
                //    int var1;

                //    if (!this.worldObj.isRemote && (this.recentlyHit > 0 || this.isPlayer()) && this.func_146066_aG() && this.worldObj.getGameRules().getGameRuleBooleanValue("doMobLoot"))
                //    {
                //        var1 = this.getExperiencePoints(this.attackingPlayer);

                //        while (var1 > 0)
                //        {
                //            int var2 = EntityXPOrb.getXPSplit(var1);
                //            var1 -= var2;
                //            this.worldObj.spawnEntityInWorld(new EntityXPOrb(this.worldObj, this.posX, this.posY, this.posZ, var2));
                //        }
                //    }

                //    this.setDead();

                //    for (var1 = 0; var1 < 20; ++var1)
                //    {
                //        double var8 = this.rand.nextGaussian() * 0.02D;
                //        double var4 = this.rand.nextGaussian() * 0.02D;
                //        double var6 = this.rand.nextGaussian() * 0.02D;
                //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, this.posY + (double)(this.rand.nextFloat() * this.height), this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width, var8, var4, var6, new int[0]);
                //    }
            }
        }

        /// <summary>
        /// Возобновить сущность
        /// </summary>
        public virtual void Respawn()
        {
            Health = GetHelathMax();
            DeathTime = 0;
            fallDistanceResult = 0f;
            fallDistance = 0f;
            Motion = new vec3(0);
            IsDead = false;
        }

        /// <summary>
        /// Задать новое значение здоровья
        /// </summary>
        public void SetHealth(float health) => Health = health < 0 ? 0 : health;

        /// <summary>
        /// Начать анимацию боли
        /// </summary>
        public void PerformHurtAnimation() => DamageTime = 5; // количество тактов

        /// <summary>
        /// Определяем дистанцию падения
        /// </summary>
        /// <param name="y">позиция Y</param>
        protected override void FallDetection(float y)
        {
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
        }

        /// <summary>
        /// Падение
        /// </summary>
        protected virtual void Fall() { }

        /// <summary>
        /// Добавить тик жизни к сущности
        /// </summary>
        protected void TicksExistedMore() => TicksExisted++;

        /// <summary>
        /// Обновление сущности в клиентской части
        /// </summary>
        public override void UpdateClient()
        {
            base.UpdateClient();
            RotationPitchPrev = RotationPitch;
            RotationYawPrev = RotationYaw;
            UpdateEntityRotation();
        }

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected virtual void Standing() => SetSize(.6f, 3.6f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected virtual void Sitting() => SetSize(.6f, 2.99f);

        /// <summary>
        /// Активация режима полёта
        /// </summary>
        public void ModeFly()
        {
            if (!IsFlying)
            {
                IsFlying = true;
                Standing();
                SpeedFly();
            }
        }

        /// <summary>
        /// Активация режима выживания
        /// </summary>
        public void ModeSurvival()
        {
            if (IsFlying)
            {
                IsFlying = false;
                SpeedSurvival();
            }
        }

        /// <summary>
        /// Скорость для режима полёта
        /// </summary>
        protected virtual void SpeedFly() => Speed = new EntitySpeed(.2f, .2f, .3f, 5.0f);

        /// <summary>
        /// Скорость для режима выживания
        /// </summary>
        protected virtual void SpeedSurvival() => Speed = new EntitySpeed(.2f);//3837f);

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public override bool CanBeCollidedWith() => !IsDead;

        /// <summary>
        /// Частички при беге блока
        /// </summary>
        private void ParticleBlockSprinting()
        {
            if (IsSprinting)// && !IsInWater())
            {
                ParticleBlockDown(Position, 1);
            }
        }
        /// <summary>
        /// Частички при падении
        /// </summary>
        public void ParticleFall(float distance)
        {
            if (distance > 2)
            {
                int count = (int)distance + 2;
                if (count > 20) count = 20;
                ParticleBlockDown(Position, count);
            }
        }

        /// <summary>
        /// Частички под ногами, бег или падение
        /// </summary>
        protected void ParticleBlockDown(vec3 pos, int count)
        {
            BlockPos blockPos = new BlockPos(pos.x, pos.y - 0.20002f, pos.z);
            BlockBase block = World.GetBlock(blockPos);
            if (block.IsParticle)
            {
                for (int i = 0; i < count; i++)
                {
                    World.SpawnParticle(EnumParticle.Digging,
                    pos + new vec3(((float)rand.NextDouble() - .5f) * Width,
                    .1f,
                    ((float)rand.NextDouble() - .5f) * Width), 
                    new vec3(0),
                    (int)block.EBlock);
                }
            }
        }

        /// <summary>
        /// Создает частицу взрыва вокруг местоположения Сущности
        /// </summary>
        public void SpawnExplosionParticle()
        {
            //if (this.worldObj.isRemote)
            //{
            //    for (int var1 = 0; var1 < 20; ++var1)
            //    {
            //        double var2 = this.rand.nextGaussian() * 0.02D;
            //        double var4 = this.rand.nextGaussian() * 0.02D;
            //        double var6 = this.rand.nextGaussian() * 0.02D;
            //        double var8 = 10.0D;
            //        this.worldObj.spawnParticle(EnumParticleTypes.EXPLOSION_NORMAL, this.posX + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var2 * var8, this.posY + (double)(this.rand.nextFloat() * this.height) - var4 * var8, this.posZ + (double)(this.rand.nextFloat() * this.width * 2.0F) - (double)this.width - var6 * var8, var2, var4, var6, new int[0]);
            //    }
            //}
            //else
            //{
            //    this.worldObj.setEntityState(this, (byte)20);
            //}
        }

        /// <summary>
        /// Визуализирует частицы сломанных предметов, используя заданный ItemStack
        /// </summary>
        /// <param name="p_70669_1_"></param>
        public void RenderBrokenItemStack(EnumBlock p_70669_1_)
        {
        //    this.playSound("random.break", 0.8F, 0.8F + this.worldObj.rand.nextFloat() * 0.4F);

        //    for (int var2 = 0; var2< 5; ++var2)
        //    {
        //        Vec3 var3 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.1D, Math.random() * 0.1D + 0.1D, 0.0D);
        //var3 = var3.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var3 = var3.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //double var4 = (double)(-this.rand.nextFloat()) * 0.6D - 0.3D;
        //Vec3 var6 = new Vec3(((double)this.rand.nextFloat() - 0.5D) * 0.3D, var4, 0.6D);
        //var6 = var6.rotatePitch(-this.rotationPitch* (float) Math.PI / 180.0F);
        //var6 = var6.rotateYaw(-this.rotationYaw* (float) Math.PI / 180.0F);
        //var6 = var6.addVector(this.posX, this.posY + (double)this.getEyeHeight(), this.posZ);
        //        this.worldObj.spawnParticle(EnumParticleTypes.ITEM_CRACK, var6.xCoord, var6.yCoord, var6.zCoord, var3.xCoord, var3.yCoord + 0.05D, var3.zCoord, new int[] { Item.getIdFromItem(p_70669_1_.getItem()) });
        //    }
        }

        // Визуализирует частицы сломанных предметов, используя заданный ItemStack
        // renderBrokenItemStack
    }
}
