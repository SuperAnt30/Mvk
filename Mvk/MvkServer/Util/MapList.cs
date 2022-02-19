using System.Collections;
using System.Collections.Generic;

namespace MvkServer.Util
{
    public abstract class MapList
    {
        protected Hashtable map = new Hashtable();
        protected List<object> list = new List<object>();

        /// <summary>
        /// Получить объект по порядковому номеру
        /// </summary>
        protected object GetAt(int index)
        {
            if (index >= 0 && index < Count) return list[index];
            return null;
        }

        /// <summary>
        /// Добавить
        /// </summary>
        protected void Add(object obj)
        {
            if (!map.ContainsKey(obj))
            {
                map.Add(obj, true);
                list.Add(obj);
            }
        }

        /// <summary>
        /// Добавить
        /// </summary>
        protected void Add(object key, object obj)
        {
            if (!list.Contains(obj))
            {
                if (map.ContainsKey(key))
                {
                    list.Remove(map[key]);
                    map.Remove(key);
                }
                map.Add(key, obj);
                list.Add(obj);
            }
        }

        /// <summary>
        /// Удалить
        /// </summary>
        protected void Remove(object obj)
        {
            if (map.ContainsKey(obj))
            {
                map.Remove(obj);
                list.Remove(obj);
            }
        }

        /// <summary>
        /// Удалить
        /// </summary>
        protected void Remove(object key, object obj)
        {
            if (map.ContainsKey(key))
            {
                map.Remove(key);
            }
            if (list.Contains(obj))
            {
                list.Remove(obj);
            }
        }

        /// <summary>
        /// Удалить список
        /// </summary>
        public void RemoveRange(MapList list)
        {
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Remove(list.GetAt(i));
                }
            }
        }

        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        protected object FirstRemove()
        {
            object obj = list[0];
            list.RemoveAt(0);
            map.Remove(obj);
            return obj;
        }

        /// <summary>
        /// Проверить наличие
        /// </summary>
        protected bool Contains(object obj) => map.ContainsKey(obj);


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

        /// <summary>
        /// Пустой ли список
        /// </summary>
        public bool IsEmpty() => list.Count == 0;


        public override string ToString() => Count.ToString();
    }
}
