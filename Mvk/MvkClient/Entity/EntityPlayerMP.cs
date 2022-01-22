using MvkClient.World;

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
        }

    }
}
