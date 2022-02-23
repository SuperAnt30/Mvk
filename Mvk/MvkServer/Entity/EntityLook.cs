using MvkServer.Glm;
using MvkServer.World;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект сущности с вращением
    /// </summary>
    public abstract class EntityLook : EntityBase
    {
        /// <summary>
        /// Поворот вокруг своей оси
        /// </summary>
        public float RotationYaw { get; protected set; }
        /// <summary>
        /// Поворот вверх вниз
        /// </summary>
        public float RotationPitch { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вокруг своей оси
        /// </summary>
        public float RotationYawPrev { get; protected set; }
        /// <summary>
        /// Значение на предыдущем тике поворота вверх вниз
        /// </summary>
        public float RotationPitchPrev { get; protected set; }

        public EntityLook(WorldBase world) : base(world) { }

        #region Frame

        /// <summary>
        /// Получить угол YawBody для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationYawBodyFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationYawPrev == RotationYaw) return RotationYaw;
            return RotationYawPrev + (RotationYaw - RotationYawPrev) * timeIndex;
        }

        /// <summary>
        /// Получить угол Pitch для кадра
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public float GetRotationPitchFrame(float timeIndex)
        {
            if (timeIndex >= 1.0f || RotationPitchPrev == RotationPitch) return RotationPitch;
            return RotationPitchPrev + (RotationPitch - RotationPitchPrev) * timeIndex;
        }

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
        /// Получить вектор направления камеры
        /// </summary>
        /// <param name="timeIndex">Коэфициент между тактами</param>
        public virtual vec3 GetLookFrame(float timeIndex) => GetLookBodyFrame(timeIndex);

        #endregion

        /// <summary>
        /// Вращение головы
        /// </summary>
        public virtual float GetRotationYawHead() => 0;

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
        /// Задать действие вращения
        /// </summary>
        protected virtual void ActionAddLook() { }

        /// <summary>
        /// Задать вращение
        /// </summary>
        protected void SetRotation(float yaw, float pitch)
        {
            RotationYaw = yaw;
            RotationPitch = pitch;
            CheckRotation();
            ActionAddLook();
        }

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
    }
}
