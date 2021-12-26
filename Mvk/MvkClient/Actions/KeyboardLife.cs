using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using System;

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

        protected void Test(vec3 bias)
        {
            vec3 pos = World.Player.HitBox.Position + bias;
            if (!pos.Equals(World.Player.HitBox.Position))
            {
                World.Player.SetMove(pos);
                World.ClientMain.TrancivePacket(new PacketC20Player(pos));
            }
        }

        /// <summary>
        /// Вперёд
        /// </summary>
        public void Forward()
        {
            //if (!IsForward)
                Test(World.Player.Front);
            IsForward = true;
            //World.Player.StepForward();
            OnMoveChanged();
        }
        /// <summary>
        /// Назад
        /// </summary>
        public void Back()
        {
            if (!IsBack) Test(World.Player.Front * -1);
            IsBack = true;
           // PlCamera.StepBack();
            OnMoveChanged();
        }
        /// <summary>
        /// Влево
        /// </summary>
        public void Left()
        {
            if (!IsLeft) Test(World.Player.Right * -1);
            IsLeft = true;
           // PlCamera.StepLeft();
            OnMoveChanged();
        }
        /// <summary>
        /// Вправо
        /// </summary>
        public void Right()
        {
            if (!IsRight) Test(World.Player.Right);
            IsRight = true;
           // PlCamera.StepRight();
            OnMoveChanged();
        }
        /// <summary>
        /// Вниз
        /// </summary>
        public void Down()
        {
            if (!IsDown) Test(World.Player.Up * -1);
            IsDown = true;
          //  PlCamera.Down();
            OnMoveChanged();
        }
        /// <summary>
        /// Вверх
        /// </summary>
        public void Up()
        {
            if (!IsUp) Test(World.Player.Up);
            IsUp = true;
          //  PlCamera.Jamp();
            OnMoveChanged();
        }
        /// <summary>
        /// Ускорения
        /// </summary>
        public void Sprinting()
        {
            IsSprinting = true;
           // PlCamera.Sprinting();
        }
        /// <summary>
        /// Отмена вперёд и назад
        /// </summary>
        public void CancelVertical()
        {
            IsForward = false;
            IsBack = false;
           // PlCamera.KeyUpVertical();
        }
        /// <summary>
        /// Отмена лева и права
        /// </summary>
        public void CancelHorizontal()
        {
            IsRight = false;
            IsLeft = false;
          //  PlCamera.KeyUpHorizontal();
        }
        /// <summary>
        /// Вниз отмена
        /// </summary>
        public void CancelDown()
        {
            IsDown = false;
           // PlCamera.KeyUpSneaking();
        }
        /// <summary>
        /// Вверх отмена
        /// </summary>
        public void CancelUp()
        {
            IsUp = false;
           // PlCamera.KeyUpJamp();
        }
        /// <summary>
        /// Ускорения отмена
        /// </summary>
        public void CancelSprinting()
        {
            IsSprinting = false;
          //  PlCamera.KeyUpSprinting();
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

        #region Event

        /// <summary>
        /// Событие движение WASD
        /// </summary>
        public event EventHandler MoveChanged;

        /// <summary>
        /// Изменена движение WASD
        /// </summary>
        protected void OnMoveChanged()
        {
            MoveChanged?.Invoke(this, new EventArgs());
        }

        #endregion
    }
}
