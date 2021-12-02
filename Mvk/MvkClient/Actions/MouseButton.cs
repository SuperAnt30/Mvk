using System;

namespace MvkClient.Actions
{
    /// <summary>
    /// Зажатие кнопки мыши
    /// </summary>
    [Flags]
    public enum MouseButton
    {
        /// <summary>
        /// Кнопка не зажата
        /// </summary>
        None = 0,
        /// <summary>
        /// Левая кнопка мыши была нажата
        /// </summary>
        Left = 1,
        /// <summary>
        /// Правая кнопка мыши была нажата
        /// </summary>
        Right = 2,
        /// <summary>
        /// Средняя кнопка мыши была нажата
        /// </summary>
        Middle = 4,
    }
}
