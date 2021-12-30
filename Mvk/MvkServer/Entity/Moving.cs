namespace MvkServer.Entity
{
    public class Moving
    {
        /// <summary>
        /// Перемещение лево право
        /// </summary>
        public float Horizontal { get; protected set; } = 0;
        /// <summary>
        /// Перемещение вперёд назад
        /// </summary>
        public float Vertical { get; protected set; } = 0;
        /// <summary>
        /// Перемещение вверх или вниз, прыжок или присел
        /// </summary>
        public float Height { get; protected set; } = 0;
        /// <summary>
        /// Ускорение, только в одну сторону 
        /// </summary>
        public float Sprinting { get; protected set; } = 0;

        /// <summary>
        /// Вперёд
        /// </summary>
        public bool Forward()
        {
            if (Vertical != 1f)
            {
                Vertical = 1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Назад
        /// </summary>
        public bool Back()
        {
            if (Vertical != -1f)
            {
                Vertical = -1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Право
        /// </summary>
        public bool Right()
        {
            if (Horizontal != 1f)
            {
                Horizontal = 1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Лево
        /// </summary>
        public bool Left()
        {
            if (Horizontal != -1f)
            {
                Horizontal = -1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Вверх
        /// </summary>
        public bool Up()
        {
            if (Height != 1f)
            {
                Height = 1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Вниз
        /// </summary>
        public bool Down()
        {
            if (Height != -1f)
            {
                Height = -1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Начать ускорение
        /// </summary>
        public bool SprintingBegin()
        {
            if (Sprinting != 1f)
            {
                Sprinting = 1f;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Вперёд и Назад отменено
        /// </summary>
        public bool VerticalCancel()
        {
            if (Vertical != 0)
            {
                Vertical = 0;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Лево и право отменено
        /// </summary>
        public bool HorizontalCancel()
        {
            if (Horizontal != 0)
            {
                Horizontal = 0;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Вверх и Вниз отменено
        /// </summary>
        public bool HeightCancel()
        {
            if (Height != 0)
            {
                Height = 0;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Отменить ускорение
        /// </summary>
        public bool SprintingCancel()
        {
            if (Sprinting != 0)
            {
                Sprinting = 0;
                return true;
            }
            return false;
        }
    }
}
