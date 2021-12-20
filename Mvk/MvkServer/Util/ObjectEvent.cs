namespace MvkServer.Util
{
    public delegate void ObjectEventHandler(object sender, ObjectEventArgs e);
    public class ObjectEventArgs
    {
        /// <summary>
        /// Вспомогательный объект при необходимости
        /// </summary>
        public object Tag { get; protected set; }

        public ObjectEventArgs(object obj) => Tag = obj;
    }
}
