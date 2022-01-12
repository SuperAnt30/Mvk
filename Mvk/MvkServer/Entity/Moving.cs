using MvkServer.Util;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект отвечающий за направление передвежения сущности, с плавным стартом и финишом
    /// </summary>
    public class Moving
    {
        /// <summary>
        /// Перемещение вперёд
        /// </summary>
        public Smooth Forward { get; protected set; }
        /// <summary>
        /// Перемещение назад
        /// </summary>
        public Smooth Back { get; protected set; }
        /// <summary>
        /// Перемещение вправо
        /// </summary>
        public Smooth Right { get; protected set; }
        /// <summary>
        /// Перемещение влево
        /// </summary>
        public Smooth Left { get; protected set; }
        /// <summary>
        /// Перемещение вверх
        /// </summary>
        public Smooth Up { get; protected set; }
        /// <summary>
        /// Перемещение вниз
        /// </summary>
        public Smooth Down { get; protected set; }
        /// <summary>
        /// Ускорение, только в одну сторону 
        /// </summary>
        public Smooth Sprinting { get; protected set; }

        public Moving()
        {
            Forward = new Smooth(0.1f, .2f);
            Back = new Smooth(0.1f, .2f);
            Right = new Smooth(0.1f, .2f);
            Left = new Smooth(0.1f, .2f);
            Up = new Smooth(.5f);
            Down = new Smooth(0.2f);
            Sprinting = new Smooth(0.2f);
        }

        /// <summary>
        /// Вперёд и назад
        /// </summary>
        public float ForwardAndBack() => Back.Value - Forward.Value;
        /// <summary>
        /// Шаг в сторону
        /// </summary>
        public float Strafe() => Right.Value - Left.Value;
        /// <summary>
        /// Высота вертикального смещения
        /// </summary>
        public float Height() => Up.Value - Down.Value;

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        public void Key(EnumKeyAction key)
        {
            switch (key)
            {
                case EnumKeyAction.ForwardDown: Forward.Begin(); break;
                case EnumKeyAction.BackDown: Back.Begin(); break;
                case EnumKeyAction.RightDown: Right.Begin(); break;
                case EnumKeyAction.LeftDown: Left.Begin(); break;
                case EnumKeyAction.UpDown: Up.Begin(); break;
                case EnumKeyAction.DownDown: Down.Begin(); break;
                case EnumKeyAction.SprintingDown: Sprinting.Begin(); break;
                case EnumKeyAction.ForwardUp: Forward.End(); break;
                case EnumKeyAction.BackUp: Back.End(); break;
                case EnumKeyAction.RightUp: Right.End(); break;
                case EnumKeyAction.LeftUp: Left.End(); break;
                case EnumKeyAction.UpUp: Up.End(); break;
                case EnumKeyAction.DownUp: Down.End(); break;
                case EnumKeyAction.SprintingUp: Sprinting.End(); break;
            }
        }

        /*
        #region Begin

        /// <summary>
        /// Вперёд
        /// </summary>
        public void ForwardBegin() => Forward.Begin();
        /// <summary>
        /// Назад
        /// </summary>
        public void BackBegin() => Back.Begin();
        /// <summary>
        /// Право
        /// </summary>
        public void RightBegin() => Right.Begin();
        /// <summary>
        /// Лево
        /// </summary>
        public void LeftBegin() => Left.Begin();
        /// <summary>
        /// Вверх
        /// </summary>
        public void UpBegin() => Up.Begin();
        /// <summary>
        /// Вниз
        /// </summary>
        public void DownBegin() => Down.Begin();
        /// <summary>
        /// Начать ускорение
        /// </summary>
        public void SprintingBegin() => Sprinting.Begin();

        #endregion

        #region End

        /// <summary>
        /// Вперёд
        /// </summary>
        public void ForwardEnd() => Forward.End();
        /// <summary>
        /// Назад
        /// </summary>
        public void BackEnd() => Back.End();
        /// <summary>
        /// Право
        /// </summary>
        public void RightEnd() => Right.End();
        /// <summary>
        /// Лево
        /// </summary>
        public void LeftEnd() => Left.End();
        /// <summary>
        /// Вверх
        /// </summary>
        public void UpEnd() => Up.End();
        /// <summary>
        /// Вниз
        /// </summary>
        public void DownEnd() => Down.End();
        /// <summary>
        /// Начать ускорение
        /// </summary>
        public void SprintingEnd() => Sprinting.End();

        #endregion
        */

        /// <summary>
        /// Принудительно отключить нажатия перемещения
        /// </summary>
        public void AllEnd()
        {
            Forward.End();
            Back.End();
            Right.End();
            Left.End();
            Up.End();
            Down.End();
            Sprinting.End();
        }

        public void Update()
        {
            Forward.Update();
            Back.Update();
            Right.Update();
            Left.Update();
            Up.Update();
            Down.Update();
            Sprinting.Update();
        }
    }
}
