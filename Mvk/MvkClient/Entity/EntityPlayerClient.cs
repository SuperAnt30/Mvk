using MvkClient.Util;
using MvkClient.World;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
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
        /// <summary>
        /// Скрыта ли сущность на экране
        /// </summary>
        public bool IsHidden { get; protected set; } = true;
        /// <summary>
        /// Проходит ли луч глаз основного игрок к этой сущности
        /// </summary>
        public bool IsRayEye { get; protected set; } = false;
        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        protected InterpolationTime interpolation = new InterpolationTime();

        public EntityPlayerClient(WorldClient world) : base()
        {
            ClientWorld = world;
            World = world;
            ClientMain = world.ClientMain;
            interpolation.Start();
        }

        /// <summary>
        /// Задать данные игрока
        /// </summary>
        public void SetDataPlayer(ushort id, string uuid, string name)
        {
            Name = name;
            UUID = uuid;
            Id = id;
        }
        /// <summary>
        /// Задать место положение игрока, при спавне, телепорте и тп
        /// </summary>
        public void SetPosLook(vec3 pos, float yaw, float pitch)
        {
            SetPosition(pos);
            RotationYawLast = RotationYawPrev = RotationYaw = RotationYawHead = RotationYawHeadPrev = yaw;
            RotationPitchLast = RotationPitchPrev = RotationPitch = pitch;
            PositionPrev = Position;
        }

        /// <summary>
        /// Получить коэффициент времени от прошлого пакета перемещения сервера в диапазоне 0 .. 1
        /// где 0 это начало, 1 финиш
        /// </summary>
        public float TimeIndex() => interpolation.TimeIndex();

        /// <summary>
        /// Задать позицию от сервера
        /// </summary>
        public void SetMotionServer(vec3 pos, float yaw, float pitch, bool sneaking)
        {
            if (IsSneaking != sneaking)
            {
                IsSneaking = sneaking;
                if (IsSneaking) Sitting(); else Standing();
            }
            PositionPrev = Position;
            SetPosition(pos);

            RotationPitchPrev = RotationPitch;
            RotationYawPrev = RotationYaw;
            RotationYawHeadPrev = RotationYawHead;
            SetRotationHead(yaw, RotationYaw, pitch);

            // Проверка толчка
            CheckPush();

            interpolation.Restart();
        }

        /// <summary>
        /// Проверка толчка этой сущности основного игрока
        /// </summary>
        protected void CheckPush()
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
            return Name + "\r\n" + base.ToString();
        }
    }
}
