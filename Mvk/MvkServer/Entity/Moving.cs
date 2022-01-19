using MvkServer.Util;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект отвечающий за направление передвежения сущности
    /// </summary>
    public class Moving
    {
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public bool Forward { get; protected set; }
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public bool Back { get; protected set; }
        /// <summary>
        /// Перемещение вправо
        /// </summary>
        public bool Right { get; protected set; }
        /// <summary>
        /// Перемещение влево
        /// </summary>
        public bool Left { get; protected set; }
        /// <summary>
        /// Перемещение вверх
        /// </summary>
        public bool Up { get; protected set; }
        /// <summary>
        /// Перемещение вниз
        /// </summary>
        public bool Down { get; protected set; }
        /// <summary>
        /// Ускорение, только в одну сторону 
        /// </summary>
        public bool Sprinting { get; protected set; }

        /// <summary>
        /// Вперёд и назад
        /// </summary>
        public float ForwardAndBack() => (Back ? 1f : 0f) - (Forward ? 1f : 0f);
        /// <summary>
        /// Шаг в сторону
        /// </summary>
        public float Strafe() => (Right ? 1f : 0f) - (Left ? 1f : 0f);
        /// <summary>
        /// Высота вертикального смещения
        /// </summary>
        public float Height() => (Up ? 1f : 0f) - (Down ? 1f : 0f);

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public void Key(EnumKeyAction key)
        {
            switch (key)
            {
                case EnumKeyAction.ForwardDown: Forward = true; break;
                case EnumKeyAction.BackDown: Back = true; break;
                case EnumKeyAction.RightDown: Right = true; break;
                case EnumKeyAction.LeftDown: Left = true; break;
                case EnumKeyAction.UpDown: Up = true; break;
                case EnumKeyAction.DownDown: Down = true; break;
                case EnumKeyAction.SprintingDown: Sprinting = true; break;
                case EnumKeyAction.ForwardUp: Forward = false; break;
                case EnumKeyAction.BackUp: Back = false; break;
                case EnumKeyAction.RightUp: Right = false; break;
                case EnumKeyAction.LeftUp: Left = false; break;
                case EnumKeyAction.UpUp: Up = false; break;
                case EnumKeyAction.DownUp: Down = false; break;
                case EnumKeyAction.SprintingUp: Sprinting = false; break;
            }
        }

        /// <summary>
        /// Принудительно отключить нажатия перемещения
        /// </summary>
        public void AllEnd()
        {
            Forward = false;
            Back = false;
            Right = false;
            Left = false;
            Up = false;
            Down = false;
            Sprinting = false;
        }
    }
}
