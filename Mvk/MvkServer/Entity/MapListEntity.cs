using MvkServer.Util;

namespace MvkServer.Entity
{
    public class MapListEntity : MapListKeyId
    {
        /// <summary>
        /// Добавить сущность
        /// </summary>
        public void Add(EntityBase entity) => Add(entity.Id, entity);
        /// <summary>
        /// Удалить сущность
        /// </summary>
        public void Remove(EntityBase entity) => Remove(entity.Id, entity);
        /// <summary>
        /// Проверить наличие сущности
        /// </summary>
        public bool ContainsValue(EntityBase entity) => base.ContainsValue(entity);
        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public new EntityBase FirstRemove() => (EntityBase)base.FirstRemove();
        /// <summary>
        /// Добавить список сущностей
        /// </summary>
        public void AddRange(MapListEntity list)
        {
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    EntityBase entity = list.GetAt(i);
                    Add(entity);
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
        public new EntityBase GetAt(int index) => (EntityBase)base.GetAt(index);
        /// <summary>
        /// Получить сущность по id сущности
        /// </summary>
        public new EntityBase Get(ushort id) => (EntityBase)base.Get(id);
        /// <summary>
        /// Получить список
        /// </summary>
        public EntityBase[] GetList()
        {
            EntityBase[] ar = new EntityBase[list.Count];
            list.CopyTo(ar);
            return ar;
        }
    }
}
