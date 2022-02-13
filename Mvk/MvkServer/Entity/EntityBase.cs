using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity
{
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public class EntityBase
    {
        /// <summary>
        /// Объект мира
        /// </summary>
        public WorldBase World { get; protected set; }
        /// <summary>
        /// Порядковый номер сущности на сервере, с момента запуска сервера
        /// </summary>
        public ushort Id { get; protected set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; protected set; } = "";
        /// <summary>
        /// Позиция объекта
        /// </summary>
        public vec3 Position { get; private set; }
        /// <summary>
        /// Поворот вокруг своей оси
        /// </summary>
        public float RotationYaw { get; protected set; }
        /// <summary>
        /// Поворот вверх вниз
        /// </summary>
        public float RotationPitch { get; protected set; }
        /// <summary>
        /// Перемещение объекта
        /// </summary>
        public vec3 Motion { get; protected set; }
        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround { get; protected set; } = true;
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
        /// Летает ли сущность
        /// </summary>
        public bool IsFlying { get; protected set; } = false;
        /// <summary>
        /// Будет ли эта сущность проходить сквозь блоки
        /// </summary>
        public bool NoClip { get; protected set; } = false;
        /// <summary>
        /// Ограничивающая рамка
        /// </summary>
        public AxisAlignedBB BoundingBox { get; protected set; }
        /// <summary>
        /// Пол ширина сущности
        /// </summary>
        public float Width { get; protected set; }
        /// <summary>
        /// Высота сущности
        /// </summary>
        public float Height { get; protected set; }
        /// <summary>
        /// Как высоко эта сущность может подняться при столкновении с блоком, чтобы попытаться преодолеть его
        /// </summary>
        public float StepHeight { get; protected set; }
        /// <summary>
        /// Объект скорости сущности
        /// </summary>
        public EntitySpeed Speed { get; protected set; }
        /// <summary>
        /// Тип сущности
        /// </summary>
        public EnumEntities Type { get; protected set; } = EnumEntities.None;
        /// <summary>
        /// Сущность мертва, не активна
        /// </summary>
        public bool IsDead { get; protected set; } = false;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как «мертвая», то есть иметь в мире труп.
        /// </summary>
        public int DeathTime { get; protected set; } = 0;
        /// <summary>
        /// Уровень здоровья
        /// </summary>
        public float Health { get; protected set; } = 20;
        /// <summary>
        /// Оставшееся время эта сущность должна вести себя как травмированная, то есть маргает красным
        /// </summary>
        public int DamageTime { get; protected set; } = 0;

        /// <summary>
        /// Истинно, если после перемещения этот объект столкнулся с чем-то по оси X или Z. 
        /// </summary>
        public bool IsCollidedHorizontally { get; protected set; } = false;
        /// <summary>
        /// Истинно, если после перемещения этот объект столкнулся с чем-то по оси Y 
        /// </summary>
        public bool IsCollidedVertically { get; protected set; } = false;
        /// <summary>
        /// Истинно, если после перемещения эта сущность столкнулась с чем-то либо вертикально, либо горизонтально 
        /// </summary>
        public bool IsCollided { get; protected set; } = false;

        /// <summary>
        /// Был ли эта сущность добавлена в чанк, в котором он находится? 
        /// </summary>
        public bool AddedToChunk { get; set; } = false;
        /// <summary>
        /// Позиция в чанке
        /// </summary>
        public vec2i PositionChunk { get; private set; }
        /// <summary>
        /// Позиция псевдо чанка
        /// </summary>
        public int PositionChunkY { get; private set; }

        /// <summary>
        /// Сколько тиков эта сущность пробежала с тех пор, как была жива 
        /// </summary>
        public int TicksExisted { get; private set; } = 0;
        /// <summary>
        /// Флаг спавна, пометка начального спавна игрока 
        /// </summary>
        public bool FlagSpawn { get; set; } = false;

        /// <summary>
        /// Пометка что было движение и подобное для сервера, чтоб отправлять пакеты
        /// </summary>
        protected bool isMotionServer = false;
        // TODO:: можно подумать о флаге для данного параметра, типа pos, Sneaking, yawHead, yawBody, pitch

        #region PervLast

        /// <summary>
        /// Последнее значение поворота вокруг своей оси
        /// </summary>
        public float RotationYawLast { get; protected set; }
        /// <summary>
        /// Последнее значение поворота вверх вниз
        /// </summary>
        public float RotationPitchLast { get; protected set; }
        /// <summary>
        /// Координата объекта на предыдущем тике, используемая для расчета позиции во время процедур рендеринга
        /// </summary>
        public vec3 PositionPrev { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вокруг своей оси
        /// </summary>
        public float RotationYawPrev { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вверх вниз
        /// </summary>
        public float RotationPitchPrev { get; protected set; }

        #endregion

        #region Get Methods

        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i GetBlockPos() => new vec3i(Position);
        /// <summary>
        /// Получить ограничительную рамку на выбранной позиции
        /// </summary>
        public AxisAlignedBB GetBoundingBox(vec3 pos) => new AxisAlignedBB(pos - new vec3(Width, 0, Width), pos + new vec3(Width, Height, Width));
        /// <summary>
        /// Высота глаз
        /// </summary>
        public float GetEyeHeight() => Height * 0.85f;

        /// <summary>
        /// Получить координаты в каком чанке находится по текущей Position
        /// </summary>
        public vec2i GetChunkPos() => new vec2i(Mth.Floor(Position.x) >> 4, Mth.Floor(Position.z) >> 4);
        /// <summary>
        /// Получить координату псевдо чанка находится по текущей Position
        /// </summary>
        public int GetChunkY() => Mth.Floor(Position.y) >> 4;

        #endregion

        

        #region Set Methods

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotation(float yaw, float pitch)
        {
            RotationYaw = yaw;
            RotationPitch = pitch;
            CheckRotation();
        }

        /// <summary>
        /// Задать позицию
        /// </summary>
        public bool SetPosition(vec3 pos)
        {
            if (!Position.Equals(pos))
            {
                Position = pos;
                UpBoundingBox();
                isMotionServer = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Заменить размер хитбокс сущности
        /// </summary>
        protected void SetSize(float width, float height)
        {
            Height = height;
            Width = width;
            UpBoundingBox();
        }

        /// <summary>
        /// Будет уничтожен следующим тиком
        /// </summary>
        public void SetDead() => IsDead = true;

        /// <summary>
        /// Задать новое значение здоровья
        /// </summary>
        public void SetHealth(float health) => Health = health < 0 ? 0 : health;

        /// <summary>
        /// Начать анимацию боли
        /// </summary>
        public void PerformHurtAnimation() => DamageTime = 5; // количество тактов

        /// <summary>
        /// Задать плоожение в чанке
        /// </summary>
        public void SetPositionChunk(int x, int y, int z)
        {
            PositionChunk = new vec2i(x, z);
            PositionChunkY = y;
        }

        /// <summary>
        /// Задать индекс
        /// </summary>
        public void SetEntityId(ushort id) => Id = id;

        /// <summary>
        /// Убить сущность
        /// </summary>
        public void Kill() => SetDead();

        #endregion

        /// <summary>
        /// Добавить тик жизни к сущности
        /// </summary>
        public void TicksExistedMore() => TicksExisted++;

        /// <summary>
        /// Проверить градусы
        /// </summary>
        protected virtual void CheckRotation()
        {
            while (RotationYaw - RotationYawPrev < -glm.pi) RotationYawPrev -= glm.pi360;
            while (RotationYaw - RotationYawPrev >= glm.pi) RotationYawPrev += glm.pi360;
            while (RotationPitch - RotationPitchPrev < -glm.pi) RotationPitchPrev -= glm.pi360;
            while (RotationPitch - RotationPitchPrev >= glm.pi) RotationPitchPrev += glm.pi360;
        }

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
        protected virtual void SpeedFly() => Speed = new EntitySpeed(.2f, .2f, .3f, 2.0f);

        /// <summary>
        /// Скорость для режима выживания
        /// </summary>
        protected virtual void SpeedSurvival() => Speed = new EntitySpeed(.2f);//3837f);

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected virtual void Standing() => SetSize(.6f, 3.6f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected virtual void Sitting() => SetSize(.6f, 2.99f);

        /// <summary>
        /// Обновить ограничительную рамку
        /// </summary>
        protected void UpBoundingBox() => BoundingBox = GetBoundingBox(Position);

        /// <summary>
        /// Получить вектор направления по поворотам
        /// </summary>
        protected vec3 GetRay(float yaw, float pitch)
        {
            //float var3 = glm.cos(-yaw - glm.pi);
            //float var4 = glm.sin(-yaw - glm.pi);
            //float var5 = -glm.cos(-pitch);
            //float var6 = glm.sin(-pitch);
            //return new vec3(var4 * var5, var6, var3 * var5);

            float pitchxz = glm.cos(pitch);
            return new vec3(glm.sin(yaw) * pitchxz, glm.sin(pitch), -glm.cos(yaw) * pitchxz);
        }

        /// <summary>
        /// Обновить значения позиции чанка по тикущим значениям
        /// </summary>
        public void UpPositionChunk()
        {
            PositionChunkY = GetChunkY();
            PositionChunk = GetChunkPos();
        }


        #region Frame

        /// <summary>
        /// Получить вектор направления камеры
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual vec3 GetLookFrame(float timeIndex) => GetLookBodyFrame(timeIndex);

        /// <summary>
        /// Получить вектор направления камеры тела
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual vec3 GetLookBodyFrame(float timeIndex) 
            => GetRay(GetRotationYawFrame(timeIndex), GetRotationPitchFrame(timeIndex));

        /// <summary>
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual float GetRotationYawFrame(float timeIndex) => GetRotationYawBodyFrame(timeIndex);

        /// <summary>
        /// Получить угол YawBody для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationYawBodyFrame(float timeIndex)
        {
            if (timeIndex == 1.0f) RotationYawPrev = RotationYaw;
            if (RotationYawPrev == RotationYaw) return RotationYaw;
            return RotationYawPrev + (RotationYaw - RotationYawPrev) * timeIndex;
        }

        /// <summary>
        /// Получить угол Pitch для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationPitchFrame(float timeIndex)
        {
            if (timeIndex == 1.0f) RotationPitchPrev = RotationPitch;
            if (RotationPitchPrev == RotationPitch) return RotationPitch;
            return RotationPitchPrev + (RotationPitch - RotationPitchPrev) * timeIndex;
        }

        /// <summary>
        /// Получить позицию сущности для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        public virtual vec3 GetPositionFrame(float timeIndex)
        {
            if (timeIndex == 1.0f)  PositionPrev = Position;
            if (Position.Equals(PositionPrev)) return Position;
            return PositionPrev + (Position - PositionPrev) * timeIndex;
        }

        /// <summary>
        /// Получить позицию глаз сущности для кадра
        /// </summary>
        /// <param name="timeIndex">коэфициент между тактами</param>
        //public vec3 GetPositionEyeFrame(float timeIndex) => GetPositionFrame(timeIndex) + new vec3(0, GetEyeHeightFrame(), 0);

        /// <summary>
        /// Высота глаз для кадра
        /// </summary>
        public virtual float GetEyeHeightFrame() => GetEyeHeight();

        #endregion

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public virtual void Update()
        {
            PositionPrev = Position;
            RotationYawPrev = RotationYaw;
            RotationPitchPrev = RotationPitch;
        }

        /// <summary>
        /// Управляет таймером смерти сущности, сферой опыта и созданием частиц
        /// </summary>
        protected void DeathUpdate()
        {
            DeathTime++;

            if (DeathTime >= 20)
            {
                SetDead();
                //DeathTime = 0;
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

        //public void Respawn()
        //{
        //    Health = 20;
        //    DeathTime = 0;
        //    IsDead = false;
        //}


    }
}
