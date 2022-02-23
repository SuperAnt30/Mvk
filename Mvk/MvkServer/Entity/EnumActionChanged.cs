using System;

namespace MvkServer.Entity
{
    /// <summary>
    /// Перечисление изминений действий
    /// </summary>
    [Flags]
    public enum EnumActionChanged
    {
        /// <summary>
        /// Нет действий
        /// </summary>
        None = 0,
        /// <summary>
        /// Смена позиции
        /// </summary>
        Position = 1,
        /// <summary>
        /// Вращение
        /// </summary>
        Look = 2,
        /// <summary>
        /// Сидеть
        /// </summary>
        IsSneaking = 4,
        /// <summary>
        /// Ускорение
        /// </summary>
        IsSprinting = 8
    }
}
