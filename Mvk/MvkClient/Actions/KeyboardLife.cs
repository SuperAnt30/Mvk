using MvkClient.World;

namespace MvkClient.Actions
{
    /// <summary>
    /// Объект хранящий нажатие клавиш перемещения
    /// </summary>
    public class KeyboardLife
    {
        /// <summary>
        /// Нажата ли клавиша идти вперёд
        /// </summary>
        public bool IsForward { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавиша идти вперёд
        /// </summary>
        public bool IsBack { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавиша идти влево
        /// </summary>
        public bool IsLeft { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавиша идти вправо
        /// </summary>
        public bool IsRight { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавиша вниз
        /// </summary>
        public bool IsDown { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавиша вверх
        /// </summary>
        public bool IsUp { get; protected set; } = false;
        /// <summary>
        /// Нажата ли клавища ускорения
        /// </summary>
        public bool IsSprinting { get; protected set; } = false;

        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        public KeyboardLife(WorldClient world) => World = world;

        /// <summary>
        /// Вперёд
        /// </summary>
        public void Forward()
        {
            if (!IsForward)
            {
                World.Player.Mov.Forward();
                IsForward = true;
            }
        }
        /// <summary>
        /// Назад
        /// </summary>
        public void Back()
        {
            if (!IsBack)
            {
                World.Player.Mov.Back();
                IsBack = true;
            }
        }
        /// <summary>
        /// Влево
        /// </summary>
        public void Left()
        {
            if (!IsLeft)
            {
                World.Player.Mov.Left();
                IsLeft = true;
            }
        }
        /// <summary>
        /// Вправо
        /// </summary>
        public void Right()
        {
            if (!IsRight)
            {
                World.Player.Mov.Right();
                IsRight = true;
            }
        }
        /// <summary>
        /// Вниз
        /// </summary>
        public void Down()
        {
            if (!IsDown)
            {
                World.Player.Mov.Down();
                IsDown = true;
            }
        }
        /// <summary>
        /// Вверх
        /// </summary>
        public void Up()
        {
            if (!IsUp)
            {
                World.Player.Mov.Up();
                IsUp = true;
            }
        }
        /// <summary>
        /// Ускорения
        /// </summary>
        public void Sprinting()
        {
            if (!IsSprinting)
            {
                World.Player.Mov.SprintingBegin();
                IsSprinting = true;
            }
        }
        /// <summary>
        /// Отмена вперёд и назад
        /// </summary>
        public void CancelVertical()
        {
            IsForward = false;
            IsBack = false;
            World.Player.Mov.VerticalCancel();
        }
        /// <summary>
        /// Отмена лева и права
        /// </summary>
        public void CancelHorizontal()
        {
            IsRight = false;
            IsLeft = false;
            World.Player.Mov.HorizontalCancel();
        }
        /// <summary>
        /// Вниз отмена
        /// </summary>
        public void CancelDown()
        {
            IsUp = false;
            IsDown = false;
            World.Player.Mov.HeightCancel();
        }
        /// <summary>
        /// Вверх отмена
        /// </summary>
        public void CancelUp()
        {
            IsUp = false;
            IsDown = false;
            World.Player.Mov.HeightCancel();
        }
        /// <summary>
        /// Ускорения отмена
        /// </summary>
        public void CancelSprinting()
        {
            IsSprinting = false;
            World.Player.Mov.SprintingCancel();
        }

        /// <summary>
        /// Принудительно отключить нажатие перемещения
        /// </summary>
        public void CancelAll()
        {
            if (IsRight || IsLeft) CancelHorizontal();
            if (IsForward || IsBack) CancelVertical();
            if (IsDown) CancelDown();
            if (IsUp) CancelUp();
            if (IsSprinting) CancelSprinting();
        }
    }
}
