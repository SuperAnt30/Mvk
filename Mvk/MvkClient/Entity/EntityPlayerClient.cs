using MvkServer.Entity.Player;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность игрока для клиента
    /// </summary>
    public class EntityPlayerClient : EntityPlayer
    {
        /// <summary>
        /// Задать id и имя игрока
        /// </summary>
        public void SetUUID(string name, string uuid)
        {
            Name = name;
            UUID = uuid;
        }
    }
}
