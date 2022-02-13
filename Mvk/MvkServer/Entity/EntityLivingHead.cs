using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект жизни сущьности c головой, отвечает за движение вращение и прочее
    /// </summary>
    public abstract class EntityLivingHead : EntityLiving
    {
        /// <summary>
        /// Вращение головы
        /// </summary>
        public float RotationYawHead { get; protected set; }
        /// <summary>
        /// Вращение головы на предыдущем тике
        /// </summary>
        public float RotationYawHeadPrev { get; protected set; }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            RotationYawHeadPrev = RotationYawHead;
            base.Update();
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
            isMotionServer = true;
        }

        #region Frame

        /// <summary>
        /// Получить угол Yaw для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override float GetRotationYawFrame(float timeIndex)
        {
            if (timeIndex == 1.0f) RotationYawHeadPrev = RotationYawHead;
            if (RotationYawHeadPrev == RotationYawHead) return RotationYawHead;
            return RotationYawHeadPrev + (RotationYawHead - RotationYawHeadPrev) * timeIndex;
        }

        /// <summary>
        /// Получить вектор направления камеры от головы
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public override vec3 GetLookFrame(float timeIndex)
            => GetRay(GetRotationYawFrame(timeIndex), GetRotationPitchFrame(timeIndex));

        #endregion

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
        /// Получить градус поворота по Yaw
        /// </summary>
        protected override float GetRotationYaw() => RotationYawHead;

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected override void HeadTurn()
        {
            float yawOffset = RotationYaw;

            if (swingProgress > 0)
            {
                // Анимация движении руки
                yawOffset = RotationYawHead;
            }
            else
            {
                float xDis = Position.x - PositionPrev.x;
                float zDis = Position.z - PositionPrev.z;
                float movDis = xDis * xDis + zDis * zDis;
                if (movDis > 0.0025f)
                {
                    // Движение, высчитываем угол направления
                    yawOffset = glm.atan2(zDis, xDis) + glm.pi90;
                    // Реверс для бега назад
                    float yawRev = glm.wrapAngleToPi(yawOffset - RotationYaw);
                    if (yawRev < -1.8f || yawRev > 1.8f) yawOffset += glm.pi;
                }
            } 
            
            float yaw2 = glm.wrapAngleToPi(yawOffset - RotationYaw);
            RotationYaw += yaw2 * .3f;
            float yaw3 = glm.wrapAngleToPi(RotationYawHead - RotationYaw);

            float angleR = glm.pi45;
            if (yaw3 < -angleR) yaw3 = -angleR;
            if (yaw3 > angleR) yaw3 = angleR;

            RotationYaw = RotationYawHead - yaw3;

            // Смещаем тело если дельта выше 60 градусов
            if (yaw3 * yaw3 > 1.1025f) RotationYaw += yaw3 * .2f;

            RotationYaw = glm.wrapAngleToPi(RotationYaw);

            CheckRotation();
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
            vec3 m = motionDebug;
            m.y = 0;
            vec3 my = new vec3(0, motionDebug.y, 0);

            return string.Format("XYZ {7} ch:{12}\r\n{0:0.000} | {13:0.000} м/c\r\nHealth: {14:0.00}\r\nyaw:{8:0.00} H:{9:0.00} pitch:{10:0.00} \r\n{1}{2}{6}{4} boom:{5:0.00}\r\nMotion:{3}\r\n{11}",
                glm.distance(m) * 10f, // 0
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
                IsCollidedHorizontally, // 11
                GetChunkPos(), // 12
                glm.distance(my) * 10f, // 13
                Health // 14
                );
        }
    }
}
