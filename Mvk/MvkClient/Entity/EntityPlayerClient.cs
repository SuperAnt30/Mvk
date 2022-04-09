using MvkClient.World;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность игрока для клиента
    /// </summary>
    public abstract class EntityPlayerClient : EntityPlayer
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }

        public EntityPlayerClient(WorldClient world) : base(world)
        {
            ClientWorld = world;
            ClientMain = world.ClientMain;
        }

        /// <summary>
        /// Задать данные игрока
        /// </summary>
        public void SetDataPlayer(ushort id, string uuid, bool isCreativeMode, string name)
        {
            base.name = name;
            UUID = uuid;
            Id = id;
            IsCreativeMode = isCreativeMode;
        }

        /// <summary>
        /// Проверка толчка этой сущности основного игрока
        /// </summary>
        public void CheckPush()
        {
            vec3 posPrev = PositionPrev;
            vec3 pos = Position;
            AxisAlignedBB aabb = ClientMain.Player.BoundingBox.Clone();
            // Толчёк происходит в момент когда прошлое положение было без колизи, а уже новое с колизией
            if (!aabb.IntersectsWith(GetBoundingBox(posPrev)) && aabb.IntersectsWith(GetBoundingBox(pos)))
            {
                // Толчёк сущности entity по вектору
                ClientMain.Player.MotionPush += pos - posPrev;
            }
        }

        public override string ToString()
        {
            return name + "\r\n" + base.ToString();
        }
    }
}
