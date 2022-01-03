using MvkClient.World;

namespace MvkClient.Actions
{
    /// <summary>
    /// Объект нажатие клавиш перемещения
    /// </summary>
    public class KeyboardLife
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        public KeyboardLife(WorldClient world) => World = world;

        /// <summary>
        /// Вперёд
        /// </summary>
        public void Forward() => World.Player.Mov.Forward();
        /// <summary>
        /// Назад
        /// </summary>
        public void Back() => World.Player.Mov.Back();
        /// <summary>
        /// Влево
        /// </summary>
        public void Left() => World.Player.Mov.Left();
        /// <summary>
        /// Вправо
        /// </summary>
        public void Right() => World.Player.Mov.Right();
        /// <summary>
        /// Вниз || присесть
        /// </summary>
        public void Down() => World.Player.Mov.Down();
        /// <summary>
        /// Вверх || прыжок
        /// </summary>
        public void Up() => World.Player.Mov.Up();
        /// <summary>
        /// Ускорения
        /// </summary>
        public void Sprinting() => World.Player.Mov.SprintingBegin();
        /// <summary>
        /// Отмена вперёд и назад
        /// </summary>
        public void CancelVertical() => World.Player.Mov.VerticalCancel();
        /// <summary>
        /// Отмена лева и права
        /// </summary>
        public void CancelHorizontal() => World.Player.Mov.HorizontalCancel();
        /// <summary>
        /// Отмена вверх вниз
        /// </summary>
        public void CancelUpDown() => World.Player.Mov.HeightCancel();
        /// <summary>
        /// Ускорения отмена
        /// </summary>
        public void CancelSprinting() => World.Player.Mov.SprintingCancel();

        /// <summary>
        /// Принудительно отключить нажатие перемещения
        /// </summary>
        public void CancelAll()
        {
            CancelHorizontal();
            CancelVertical();
            CancelUpDown();
            CancelSprinting();
        }
    }
}
