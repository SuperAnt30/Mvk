namespace MvkServer.Network
{
    /// <summary>
    /// Перечесление статуса запроса
    /// </summary>
    public enum StatusNet
    {
        /// <summary>
        /// Соединились
        /// </summary>
        Connect,
        /// <summary>
        /// Разъеденились
        /// </summary>
        Disconnect,
        /// <summary>
        /// Разъединяемся
        /// </summary>
        Disconnecting,
        /// <summary>
        /// Получили ответ
        /// </summary>
        Receive,
        /// <summary>
        /// Загрузка
        /// </summary>
        Loading
    }
}
