using MvkServer.Util;

namespace MvkServer.Entity
{
    public class MapListEntityTrackerEntry : MapListKeyId
    {
        /// <summary>
        /// Добавить трек
        /// </summary>
        public void Add(EntityTrackerEntry entity) => Add(entity.TrackedEntity.Id, entity);
        /// <summary>
        /// Удалить трек
        /// </summary>
        public void Remove(EntityTrackerEntry entity) => Remove(entity.TrackedEntity.Id, entity);
        /// <summary>
        /// Проверить наличие трека
        /// </summary>
        public bool ContainsValue(EntityTrackerEntry entity) => base.ContainsValue(entity);
        /// <summary>
        /// Получить первое значение по списку и удалить его
        /// </summary>
        public new EntityTrackerEntry FirstRemove() => (EntityTrackerEntry)base.FirstRemove();
        /// <summary>
        /// Получить трек по порядковому номеру
        /// </summary>
        public new EntityTrackerEntry GetAt(int index) => (EntityTrackerEntry)base.GetAt(index);
        /// <summary>
        /// Получить трек по id
        /// </summary>
        public new EntityTrackerEntry Get(ushort id) => (EntityTrackerEntry)base.Get(id);
        /// <summary>
        /// Получить список
        /// </summary>
        public EntityTrackerEntry[] GetList()
        {
            EntityTrackerEntry[] ar = new EntityTrackerEntry[list.Count];
            list.CopyTo(ar);
            return ar;
        }
    }
}
