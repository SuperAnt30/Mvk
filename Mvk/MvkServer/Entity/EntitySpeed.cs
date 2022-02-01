namespace MvkServer.Entity
{
    /// <summary>
    /// Объект скоростей сущьности
    /// </summary>
    public struct EntitySpeed
    {
        /// <summary>
        /// Скорость шага в один такт
        /// </summary>
        public float Forward;
        /// <summary>
        /// Скорость бокового шага в один такт
        /// </summary>
        public float Strafe;
        /// <summary>
        /// Скорость вертикального перемещения для полёта в один такт
        /// </summary>
        public float Vertical;
        /// <summary>
        /// Скорость ускорения бега
        /// </summary>
        public float Sprinting;
        /// <summary>
        /// Крадёмся
        /// </summary>
        public float Sneaking;

        public EntitySpeed(float forward)
        {
            Forward = Strafe = Vertical = forward;
            Sprinting = 1.3f;
            Sneaking = 0.3f;
        }
        public EntitySpeed(float forward, float strafe, float vertical) : this(forward, strafe, vertical, 1.3f) { }
        
        public EntitySpeed(float forward, float strafe, float vertical, float sprinting)
        {
            Forward = forward;
            Strafe = strafe;
            Vertical = vertical;
            Sprinting = sprinting;
            Sneaking = 0.3f;
        }

        #region ToString support

        public override string ToString()
        {
            return string.Format("F:{0:0.00} S:{1:0.00} V:{2:0.00} ->{3:0.00}", Forward, Strafe, Vertical, Sprinting);
        }

        #endregion
    }
}
