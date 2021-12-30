namespace MvkServer.Entity
{
    /// <summary>
    /// Хитбокс сущности
    /// </summary>
    public class HitBox
    {
        /// <summary>
        /// Пол ширины
        /// </summary>
        public float Width { get; protected set; } = 0;
        /// <summary>
        /// Высота
        /// </summary>
        public float Heigth { get; protected set; } = 0;
        /// <summary>
        /// Смещение глаз
        /// </summary>
        public float Eyes { get; protected set; } = 0;

        public HitBox(float w, float h, float eyes)
        {
            Width = w;
            Heigth = h;
            Eyes = eyes;
        }
    }
}
