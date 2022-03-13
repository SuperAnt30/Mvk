using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Карта загрузок векторов vec2i
    /// </summary>
    public class MapListVec2i : MapList
    {
        /// <summary>
        /// Добавить вектор
        /// </summary>
        public void Add(vec2i pos) => base.Add(pos);

        public void Insert(vec2i pos)
        {
            if (!map.ContainsKey(pos))
            {
                map.Add(pos, true);
                list.Insert(0, pos);
            }
        }
        /// <summary>
        /// Удалить вектор
        /// </summary>
        public void Remove(vec2i pos) => base.Remove(pos);
        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public new vec2i FirstRemove() => (vec2i)base.FirstRemove();
        /// <summary>
        /// Проверить наличие вектор
        /// </summary>
        public bool Contains(vec2i pos) => base.Contains(pos);
    }
}
