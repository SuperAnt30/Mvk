using MvkClient.World;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность остальных игроков тикущего клиента
    /// </summary>
    public class EntityPlayerMP : EntityPlayerClient
    {
        public EntityPlayerMP(WorldClient world) : base(world) { }
    }
}
