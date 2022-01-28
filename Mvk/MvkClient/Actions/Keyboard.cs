using MvkClient.World;
using MvkServer.Entity;
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
            if (key == 9) World.ClientMain.MouseGamePlay(); // Tab
            //else if (key == 18) World.Player.InputNone(); // Alt
            else if (key == 27 || key == 18) World.ClientMain.Screen.InGameMenu(); // Esc
            else if (key == 116) World.Player.ViewCameraNext(); // F5
            else if (key == 117) World.RenderEntityManager.IsHiddenHitbox = !World.RenderEntityManager.IsHiddenHitbox; // F6
            else World.Player.InputAdd(KeyActionToInput(key));
        }

        /// <summary>
        /// Отпущена клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Up(int key)
        {
            if (key == 18) World.Player.InputNone(); // Alt
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
