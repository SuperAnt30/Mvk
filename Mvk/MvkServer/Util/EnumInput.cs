using System;

namespace MvkServer.Util
{
    [Flags]
    public enum EnumInput
    {
        /// <summary>
        /// Нет перемещений
        /// </summary>
        None = 0,
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        Forward = 1,
        /// <summary>
        /// Перемещение назад
        /// </summary>
        Back = 2,
        /// <summary>
        /// Перемещение вправо
        /// </summary>
        Right = 4,
        /// <summary>
        /// Перемещение влево
        /// </summary>
        Left = 8,
        /// <summary>
        /// Перемещение вверх
        /// </summary>
        Up = 16,
        /// <summary>
        /// Перемещение вниз
        /// </summary>
        Down = 32,
        /// <summary>
        /// Ускорение, только в одну сторону 
        /// </summary>
        Sprinting = 64
    }
}
