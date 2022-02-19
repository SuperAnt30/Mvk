namespace MvkServer.Util
{
    /// <summary>
    /// Карта Id
    /// </summary>
    public class MapListId : MapList
    {
        /// <summary>
        /// Добавить id
        /// </summary>
        public void Add(ushort id) => base.Add(id);
        /// <summary>
        /// Удалить id
        /// </summary>
        public void Remove(ushort id) => base.Remove(id);
        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public new ushort FirstRemove() => (ushort)base.FirstRemove();
    }
}
