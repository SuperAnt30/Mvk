namespace MvkClient.Gui
{
    /// <summary>
    /// Ключи для нажатий кнопки и понимания их действий
    /// </summary>
    public enum EnumScreenKey
    {
        /// <summary>
        /// Нет значения
        /// </summary>
        None,
        /// <summary>
        /// Основное меню
        /// </summary>
        Main,
        /// <summary>
        /// Одинночная игра
        /// </summary>
        SinglePlayer,
        /// <summary>
        /// Сетевая игра
        /// </summary>
        Multiplayere,
        /// <summary>
        /// Соединение
        /// </summary>
        Connection,
        /// <summary>
        /// Опции
        /// </summary>
        Options,
        /// <summary>
        /// Предупреждение
        /// </summary>
        YesNo,
        /// <summary>
        /// Запуск мира, включается загрузка
        /// </summary>
        WorldBegin,
        /// <summary>
        /// Игра, удаляем gui
        /// </summary>
        World,
        /// <summary>
        /// Меню во время игры
        /// </summary>
        InGameMenu,
        /// <summary>
        /// Сохранение мира
        /// </summary>
        WorldSaving
    }
}
