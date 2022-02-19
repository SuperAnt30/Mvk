using MvkServer.Util;

namespace MvkServer.Entity
{
    public class MapListEntity : MapListKeyId
    {
        /// <summary>
        /// Добавить сущность
        /// </summary>
        public void Add(EntityLiving entity) => Add(entity.Id, entity);
        /// <summary>
        /// Удалить сущность
        /// </summary>
        public void Remove(EntityLiving entity) => Remove(entity.Id, entity);
        /// <summary>
        /// Проверить наличие сущности
        /// </summary>
        public bool ContainsValue(EntityLiving entity) => base.ContainsValue(entity);
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
                    EntityLiving entity = list.GetAt(i);
                    Add(entity);
                }
            }
        }
        /// <summary>
        /// Удалить список сущностей
        /// </summary>
        public void RemoveRange(MapListEntity list) => base.RemoveRange(list);
        /// <summary>
        /// Получить сущность по порядковому номеру
        /// </summary>
        public new EntityLiving GetAt(int index) => (EntityLiving)base.GetAt(index);
        /// <summary>
        /// Получить сущность по id сущности
        /// </summary>
        public new EntityLiving Get(ushort id) => (EntityLiving)base.Get(id);
        /// <summary>
        /// Получить список
        /// </summary>
        public EntityLiving[] GetList()
        {
            EntityLiving[] ar = new EntityLiving[list.Count];
            list.CopyTo(ar);
            return ar;
        }
    }
}
