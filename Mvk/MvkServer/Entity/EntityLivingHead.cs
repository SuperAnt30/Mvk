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
        protected override float GetRotationYawFrame(float timeIndex)
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
        /// Определение вращения
        /// </summary>
        protected override vec3 MotionAngle(float strafe, float forward)
        {
            vec3 motion = Motion;

            if (strafe != 0 || forward != 0)
            {
                float ysin = glm.sin(RotationYawHead);
                float ycos = glm.cos(RotationYawHead);
                motion.x += ycos * strafe - ysin * forward;
                motion.z += ycos * forward + ysin * strafe;
            }
            return motion;
        }

        /// <summary>
        /// Поворот тела от движения и поворота головы 
        /// </summary>
        protected override void HeadTurn(vec3 motion)
        {
            float yawOffset;
            // Определяем двигается ли сущность
            bool bf = Input.HasFlag(EnumInput.Forward);
            bool bb = Input.HasFlag(EnumInput.Back);
            bool bl = Input.HasFlag(EnumInput.Left);
            bool br = Input.HasFlag(EnumInput.Right);
            //bool movingForward = Mov.Forward || Mov.Back || Mov.Left || Mov.Right;

            if (bf || bb || bl || br)
            {
                // Определяем угол направления в зависимости куда движемся
                yawOffset = glm.atan2(motion.z, motion.x);
                if (bf) yawOffset += glm.pi90;
                else if (bb) yawOffset -= glm.pi90;
                else if (bl) yawOffset += glm.pi135;
                else if (br) yawOffset += glm.pi45;

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
    }
}
