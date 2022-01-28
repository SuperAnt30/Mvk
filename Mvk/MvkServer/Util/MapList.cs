using MvkServer.Glm;
using System.Collections;
using System.Collections.Generic;

namespace MvkServer.Util
{
    /// <summary>
    /// Карта загрузок чанка
    /// </summary>
    public class MapList
    {
        protected Hashtable map = new Hashtable();
        protected List<vec2i> list = new List<vec2i>();

        /// <summary>
        /// Добавить чанк
        /// </summary>
        public void Add(vec2i pos)
        {
            if (!map.ContainsKey(pos))
            {
                map.Add(pos, true);
                list.Add(pos);
            }
        }
        /// <summary>
        /// Удалить чанк
        /// </summary>
        public void Remove(vec2i pos)
        {
            if (map.ContainsKey(pos))
            {
                map.Remove(pos);
                list.Remove(pos);
            }
        }

        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public vec2i FirstRemove()
        {
            vec2i pos = list[0];
            list.RemoveAt(0);
            map.Remove(pos);
            return pos;
        }

        /// <summary>
        /// Клон карты
        /// </summary>
        public Hashtable CloneMap() => map.Clone() as Hashtable;

        /// <summary>
        /// Очистить
        /// </summary>
        public void Clear()
        {
            map.Clear();
            list.Clear();
        }

        /// <summary>
        /// Количество элементов
        /// </summary>
        public int Count => list.Count;
    }
}
