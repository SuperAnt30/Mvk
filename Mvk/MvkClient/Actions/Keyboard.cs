using MvkServer.Util;

namespace MvkClient.Actions
{
    public class Keyboard
    {
        /// <summary>
        /// Получить ключь действие клавиши по индексу нажатой клавиши
        /// </summary>
        public static EnumKeyAction KeyActionToDown(int key)
        {
            switch (key)
            {
                case 65: return EnumKeyAction.LeftDown;
                case 68: return EnumKeyAction.RightDown;
                case 87: return EnumKeyAction.ForwardDown;
                case 83: return EnumKeyAction.BackDown;
                case 32: return EnumKeyAction.UpDown;
                case 16: return EnumKeyAction.DownDown;
                case 17: return EnumKeyAction.SprintingDown;
            }
            return EnumKeyAction.None;
        }

        /// <summary>
        /// Получить ключь действие клавиши по индексу отпущенной клавиши
        /// </summary>
        public static EnumKeyAction KeyActionToUp(int key)
        {
            switch (key)
            {
                case 65: return EnumKeyAction.LeftUp;
                case 68: return EnumKeyAction.RightUp;
                case 87: return EnumKeyAction.ForwardUp;
                case 83: return EnumKeyAction.BackUp;
                case 32: return EnumKeyAction.UpUp;
                case 16: return EnumKeyAction.DownUp;
                case 17: return EnumKeyAction.SprintingUp;
            }
            return EnumKeyAction.None;
        }
    }
}
