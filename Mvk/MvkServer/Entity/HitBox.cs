using MvkServer.Glm;

namespace MvkServer.Entity
{
    /// <summary>
    /// Хитбокс сущности
    /// </summary>
    public struct HitBox
    {
        private readonly float width;
        private readonly float height;
        private readonly float eyes;

        public HitBox(float w, float h, float eyes)
        {
            width = w;
            height = h;
            this.eyes = eyes;
        }
        /// <summary>
        /// Пол ширины
        /// </summary>
        public float GetWidth() => width;
        /// <summary>
        /// Высота
        /// </summary>
        public float GetHeight() => height;
        /// <summary>
        /// Смещение глаз
        /// </summary>
        public float GetEyes() => eyes;
        /// <summary>
        /// Высота от глаз и выше
        /// </summary>
        public float GetUpEyes() => height - eyes;

        public vec3 VecWidth() => new vec3(width, 0, width);
        public vec3 VecAll() => new vec3(width, height, width);


        /// <summary>
        /// Заменить хзитбокс
        /// </summary>
        public HitBox SetHeightEyes(float height, float eyes) => new HitBox(width, height, eyes);

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(HitBox))
            {
                var vec = (HitBox)obj;
                if (height == vec.GetHeight() && width == vec.width && eyes == vec.eyes)
                    return true;
            }

            return false;
        }

        #region ToString support

        public override string ToString()
        {
            return string.Format("W{0:0.00}; H{1:0.00}; E{2:0.00}", width, height, eyes);
        }

        #endregion
    }
}
