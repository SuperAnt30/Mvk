using MvkClient.World;
using MvkServer.Util;
using System.Diagnostics;

namespace MvkClient.Actions
{
    public class Keyboard
    {
        // <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }

        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private static Stopwatch stopwatch = new Stopwatch();
        /// <summary>
        /// Какая клавиша была нажата ранее
        /// </summary>
        private int keyPrev;
        /// <summary>
        /// Нажатая ли F3
        /// </summary>
        private bool keyF3 = false;

        public Keyboard(WorldClient world)
        {
            World = world;
            stopwatch.Start();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Down(int key)
        {
            if (key == 32 && !World.Player.Input.HasFlag(EnumInput.Up))
            {
                long ms = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                if (ms < 300 && key == keyPrev)
                {
                    // дабл клик пробела
                    if (World.Player.IsFlying) World.Player.ModeSurvival();
                    else if (!World.Player.IsSneaking) World.Player.ModeFly();
                }
            }
            keyPrev = key;

            // одно нажатие
            if (key == 66 && keyF3) World.RenderEntityManager.IsHiddenHitbox = !World.RenderEntityManager.IsHiddenHitbox; // F3+B
            else if (key == 114) keyF3 = true; // F3
            else if (key == 27 || key == 18) World.ClientMain.Screen.InGameMenu(); // Esc или Alt
            else if (key == 116) World.Player.ViewCameraNext(); // F5
            //else if (key == 117) // F6
            else World.Player.InputAdd(KeyActionToInput(key));
        }

        /// <summary>
        /// Отпущена клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Up(int key)
        {
            if (key == 114) // F3
            {
                keyF3 = false;
                if (keyPrev == 114) Debug.IsDraw = !Debug.IsDraw; // Нажимали только F3
            }
            else World.Player.InputRemove(KeyActionToInput(key));
        }

        /// <summary>
        /// Получить ключ действие клавиши по индексу нажатой клавиши
        /// </summary>
        private EnumInput KeyActionToInput(int key)
        {
            switch (key)
            {
                case 65: return EnumInput.Left;
                case 68: return EnumInput.Right;
                case 87: return EnumInput.Forward;
                case 83: return EnumInput.Back;
                case 32: return EnumInput.Up;
                case 16: return EnumInput.Down;
                case 17: return EnumInput.Sprinting;
            }
            return EnumInput.None;
        }
    }
}
