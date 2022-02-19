using System.Collections;

namespace MvkServer.Util
{
    /// <summary>
    /// Карта Id
    /// </summary>
    public abstract class MapListKeyId : MapList
    {
        /// <summary>
        /// Проверить наличие объект по id
        /// </summary>
        public bool ContainsId(ushort id) => map.ContainsKey(id);

        /// <summary>
        /// Проверить наличие объекта
        /// </summary>
        public bool ContainsValue(object obj) => map.ContainsValue(obj);

        /// <summary>
        /// Получить объект по id сущности
        /// </summary>
        public object Get(ushort id) => map.ContainsKey(id) ? map[id] : null;

        /// <summary>
        /// Получить список id
        /// </summary>
        public ushort[] GetListId()
        {
            ushort[] ar = new ushort[map.Keys.Count];
            int i = 0;
            foreach (DictionaryEntry de in map)
            {
                ar[i] = (ushort)de.Key;
                i++;
            }
            return ar;
        }
    }
}
