namespace MvkClient.Util
{
    public delegate void ObjectKeyEventHandler(object sender, ObjectKeyEventArgs e);
    public class ObjectKeyEventArgs
    {
        /// <summary>
        /// Вспомогательный объект при необходимости
        /// </summary>
        public object Tag { get; protected set; }
        /// <summary>
        /// Ключ запроса объекта
        /// </summary>
        public ObjectKey Key { get; protected set; }

        public ObjectKeyEventArgs(ObjectKey key) => Key = key;
        public ObjectKeyEventArgs(ObjectKey key, object obj) : this(key) => Tag = obj;
    }

    /// <summary>
    /// Перечень ключей запросов
    /// </summary>
    public enum ObjectKey
    {
        /// <summary>
        /// Шаг загрузки
        /// </summary>
        LoadStep,
        /// <summary>
        /// Шаг загрузки текстуры
        /// </summary>
        LoadStepTexture,
        /// <summary>
        /// Закончена основная загрузка
        /// </summary>
        LoadedMain,
        /// <summary>
        /// Количество тактов в загрузчике
        /// </summary>
        LoadCountWorld,
        /// <summary>
        /// Сервер остановлен
        /// </summary>
        ServerStoped,
        /// <summary>
        /// Ошибка
        /// </summary>
        Error,
        /// <summary>
        /// Рендер отладки
        /// </summary>
        RenderDebug,
        /// <summary>
        /// Режим игры, скрыт курсор
        /// </summary>
        GameMode,
        /// <summary>
        /// Режим GUI, конец игры
        /// </summary>
        GameOver
    }
}
