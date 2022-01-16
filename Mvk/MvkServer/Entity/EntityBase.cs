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

        #endregion

        // TODO:: Enity.getLook // Enity.func_174806_f // Enity.func_174824_e // Enity.func_174822_a

        #region Get Methods

        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i GetBlockPos() => new vec3i(Position);
        /// <summary>
        /// В каком чанке находится
        /// </summary>
        public vec2i GetChunkPos() => new vec2i(Mth.Floor(Position.x) >> 4, Mth.Floor(Position.z) >> 4);
        /// <summary>
        /// Позиция псевдо чанка
        /// </summary>
        public int GetChunkY() => Mth.Floor(Position.y) >> 4;
        /// <summary>
        /// Получить ограничительную рамку на выбранной позиции
        /// </summary>
        public AxisAlignedBB GetBoundingBox(vec3 pos) => new AxisAlignedBB(pos - new vec3(Width, 0, Width), pos + new vec3(Width, Height, Width));
        /// <summary>
        /// Высота глаз
        /// </summary>
        public float GetEyeHeight() => Height * 0.85f;

        #endregion

        #region Set Methods

        /// <summary>
        /// Задать вращение
        /// </summary>
        public void SetRotation(float yaw, float pitch)
        {
            RotationYaw = yaw;
            RotationPitch = pitch;
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
        /// Положение стоя
        /// </summary>
        protected virtual void Standing() => SetSize(.6f, 3.6f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected virtual void Sitting() => SetSize(.6f, 2.6f);

        #endregion

        /// <summary>
        /// Обновить ограничительную рамку
        /// </summary>
        protected void UpBoundingBox() => BoundingBox = GetBoundingBox(Position);

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public virtual void Update()
        {
            PositionPrev = Position;
            RotationYawLast = RotationYaw;
            RotationPitchLast = RotationPitch;
        }
    }
}
