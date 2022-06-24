namespace MvkClient.Gui
{
    /// <summary>
    /// Перечень фона
    /// </summary>
    public enum EnumBackground
    {
        /// <summary>
        /// Фон загрузки
        /// </summary>
        Loading,
        /// <summary>
        /// Фон в меню вне игры
        /// </summary>
        Menu,
        /// <summary>
        /// Фон в главном меню
        /// </summary>
        TitleMain,
        /// <summary>
        /// Фон в игре, при открыти любого окна, инвентарь и прочее
        /// </summary>
        GameWindow,
        /// <summary>
        /// Фона быть не должно, прозрачно
        /// </summary>
        Game,
        /// <summary>
        /// Фон в игре, при смерте
        /// </summary>
        GameOver
    }
}
