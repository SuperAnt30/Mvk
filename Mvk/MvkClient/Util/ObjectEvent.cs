namespace MvkClient.Util
{
    public delegate void ObjectEventHandler(object sender, ObjectEventArgs e);
    public class ObjectEventArgs
    {
        /// <summary>
        /// Вспомогательный объект при необходимости
        /// </summary>
        public object Tag { get; protected set; }
        /// <summary>
        /// Ключ запроса объекта
        /// </summary>
        public ObjectKey Key { get; protected set; }

        public ObjectEventArgs(ObjectKey key) => Key = key;
        public ObjectEventArgs(ObjectKey key, object obj) : this(key) => Tag = obj;
    }

    /// <summary>
    /// Перечень ключей запросов
    /// </summary>
    public enum ObjectKey
    {
        /// <summary>
        /// Такт загрузки
        /// </summary>
        LoadTick,
        /// <summary>
        /// Такт загрузки текстуры
        /// </summary>
        LoadTickTexture,
        /// <summary>
        /// Закончена загрузка
        /// </summary>
        LoadingStop
    }
}
