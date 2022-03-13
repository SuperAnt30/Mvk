using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using System;
using System.Collections.Generic;

namespace MvkServer.Entity
{
    /// <summary>
    /// Базовый объект сущности
    /// </summary>
    public abstract class EntityBase
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
        /// Позиция объекта
        /// </summary>
        public vec3 Position { get; private set; }
        /// <summary>
        /// Позиция на последнем тике для рендера, клиента
        /// </summary>
        public vec3 LastTickPos { get; set; }
        /// <summary>
        /// Координата объекта на предыдущем тике, используемая для расчета позиции во время процедур рендеринга
        /// </summary>
        public vec3 PositionPrev { get; protected set; }
        /// <summary>
        /// Перемещение объекта
        /// </summary>
        public vec3 Motion { get; protected set; }
        /// <summary>
        /// На земле
        /// </summary>
        public bool OnGround { get; protected set; } = true;
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
        /// Сущность мертва, не активна
        /// </summary>
        public bool IsDead { get; protected set; } = false;
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
        /// Генератор случайных чисел данной сущности
        /// </summary>
        protected Random rand;
        /// <summary>
        /// Для отладки движения
        /// </summary>
        protected vec3 motionDebug = new vec3(0);
        
        public EntityBase(WorldBase world)
        {
            World = world;
            rand = World.Rand;
            SetSize(.5f, 1f);
        }

        /// <summary>
        /// В каком блоке находится
        /// </summary>
        public vec3i GetBlockPos() => new vec3i(Position);
        /// <summary>
        /// Получить ограничительную рамку на выбранной позиции
        /// </summary>
        public AxisAlignedBB GetBoundingBox(vec3 pos) => new AxisAlignedBB(pos - new vec3(Width, 0, Width), pos + new vec3(Width, Height, Width));

        /// <summary>
        /// Получает квадрат расстояния до положения
        /// </summary>
        //public double GetDistanceSq(vec3 pos)
        //{
        //    float x = Position.x - pos.x;
        //    float y = Position.y - pos.y;
        //    float z = Position.z - pos.z;
        //    return x * x + y * y + z * z;
        //}

        /// <summary>
        /// Задать позицию
        /// </summary>
        public bool SetPosition(vec3 pos)
        {
            if (!Position.Equals(pos))
            {
                Position = pos;
                UpBoundingBox();
                ActionAddPosition();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Задать действие для позиции
        /// </summary>
        protected virtual void ActionAddPosition() { }

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
        /// Задать индекс
        /// </summary>
        public void SetEntityId(ushort id) => Id = id;

        /// <summary>
        /// Убить сущность
        /// </summary>
        public void Kill() => SetDead();

        /// <summary>
        /// Обновить ограничительную рамку
        /// </summary>
        protected void UpBoundingBox() => BoundingBox = GetBoundingBox(Position);

        public virtual vec3 GetPositionFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || Position.Equals(LastTickPos)) return Position;
            return LastTickPos + (Position - LastTickPos) * timeIndex;
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

            bool isSneaking = false;
            if (this is EntityLiving entityLiving)
            {
                isSneaking = entityLiving.IsSneaking;
            }

            // Защита от падения с края блока если сидишь и являешься игроком
            if (OnGround && isSneaking && this is EntityPlayer)
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
                if (isSneaking) stepHeight *= 0.5f;

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
                }
                else
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
            FallDetection(y);

            Motion = new vec3(x0 != x ? 0 : x, y, z0 != z ? 0 : z);
            UpPositionMotion();
        }

        /// <summary>
        /// Определяем дистанцию падения
        /// </summary>
        /// <param name="y">позиция Y</param>
        protected virtual void FallDetection(float y) { }

        private void UpPositionMotion()
        {
            motionDebug = Motion;
            SetPosition(Position + Motion);
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// Возвращает истину, если другие Сущности не должны проходить через эту Сущность
        /// </summary>
        public virtual bool CanBeCollidedWith() => false;
        /// <summary>
        /// Возвращает true, если этот объект должен толкать и толкать другие объекты при столкновении
        /// </summary>
        public virtual bool CanBePushed() => false;

        /// <summary>
        /// Получить размер границы столкновения 
        /// </summary>
        public float GetCollisionBorderSize() => .1f;
    }
}
