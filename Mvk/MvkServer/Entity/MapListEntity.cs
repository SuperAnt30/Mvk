using MvkServer.Util;
using System.Collections;

namespace MvkServer.Entity
{
    public class MapListEntity : MapList
    {
        /// <summary>
        /// Добавить сущность
        /// </summary>
        public void Add(EntityLiving entity) => Add(entity.Id, entity);
        /// <summary>
        /// Удалить сущность
        /// </summary>
        public void Remove(EntityLiving entity)
        {
            if (map.ContainsKey(entity.Id)) map.Remove(entity.Id);
            if (list.Contains(entity)) list.Remove(entity);
        }
        /// <summary>
        /// Проверить наличие сущности
        /// </summary>
        public bool Contains(EntityLiving entity) => base.Contains(entity);
        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public new EntityLiving FirstRemove() => (EntityLiving)base.FirstRemove();
        /// <summary>
        /// Добавить список сущностей
        /// </summary>
        public void AddRange(MapListEntity list)
        {
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    Add(list.GetAt(i));
                }
            }
        }
        /// <summary>
        /// Удалить список сущностей
        /// </summary>
        public void RemoveRange(MapListEntity list)
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
        /// Получить сущность по порядковому номеру
        /// </summary>
        public EntityLiving GetAt(int index)
        {
            if (index >= 0 && index < Count) return (EntityLiving)list[index];
            return null;
        }

        /// <summary>
        /// Получить сущность по id сущности
        /// </summary>
        public object Get(ushort id)
        {
            if (map.ContainsKey(id)) return map[id];
            return null;
        }

        /// <summary>
        /// Получить список
        /// </summary>
        public EntityLiving[] GetList()
        {
            EntityLiving[] ar = new EntityLiving[list.Count];
            int i = 0;
            foreach (EntityLiving entity in list)
            {
                ar[i] = entity;
                i++;
            }
            return ar;
        }
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

        public override string ToString() => Count.ToString();
    }
}
