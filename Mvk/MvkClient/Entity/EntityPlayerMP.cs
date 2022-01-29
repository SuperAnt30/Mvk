using MvkClient.Util;
using MvkClient.World;
using MvkServer.Glm;

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
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            // Расчёт амплитуды конечностей, при движении
            UpLimbSwing();

            // Видна ли сущность, прорисовывать ли её на экране
            IsHidden = !ClientWorld.Player.FrustumCulling.IsBoxInFrustum(
                BoundingBox.Offset(ClientWorld.RenderEntityManager.CameraOffset * -1f));
        }

    }
}
