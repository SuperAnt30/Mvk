using MvkClient.Util;
using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;

namespace MvkClient.Entity
{
    /// <summary>
    /// Сущность остальных игроков тикущего клиента
    /// </summary>
    public class EntityPlayerMP : EntityPlayerClient
    {
        public EntityPlayerMP(WorldClient world) : base(world)
        {
            interpolation.Start();
            IsHidden = false;
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            // если нет хп обновлям смертельную картинку
            if (Health <= 0f) DeathUpdate();
            // Счётчик получения урона для анимации
            if (DamageTime > 0) DamageTime--;
            // Для вращении головы
            HeadTurn();
            // Расчёт амплитуды конечностей, при движении
            UpLimbSwing();
            // Просчёт взмаха руки
            UpdateArmSwingProgress();

            // Видна ли сущность, прорисовывать ли её на экране
            //IsHidden = !ClientMain.Player.FrustumCulling.IsBoxInFrustum(
            //    BoundingBox.Offset(ClientWorld.RenderEntityManager.CameraOffset * -1f));

            // Проверка луча игрока к сущности
            bool isRayEye = false;
            // Максимальная дистанция луча
            float maxDist = 32f;

            vec3 pos = ClientMain.Player.Position;

            if (!IsHidden && glm.distance(Position, pos) < maxDist)
            {
                pos.y += ClientMain.Player.GetEyeHeight();
                vec3 dir = ClientMain.Player.RayLook;
                RayCross ray = new RayCross(pos, dir, maxDist);
                isRayEye = ray.CrossLineToRectangle(BoundingBox.Clone());
            }
            IsRayEye = isRayEye;
        }

    }
}
