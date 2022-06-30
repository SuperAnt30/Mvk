using MvkClient.World;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System.Diagnostics;

namespace MvkClient.Actions
{
    public class Keyboard
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
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
            ClientMain = world.ClientMain;
            stopwatch.Start();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void Down(int key)
        {
            if (key == 32 && !ClientMain.Player.Input.HasFlag(EnumInput.Up))
            {
                long ms = stopwatch.ElapsedMilliseconds;
                stopwatch.Restart();
                if (ms < 300 && key == keyPrev)
                {
                    if (ClientMain.Player.IsCreativeMode)
                    {
                        // Только креатив может летать, покуда
                        if (ClientMain.Player.IsFlying) ClientMain.Player.ModeSurvival();
                        else if (!ClientMain.Player.IsSneaking()) ClientMain.Player.ModeFly();
                    }
                }
            }
            keyPrev = key;

            // одно нажатие
            if (key == 66 && keyF3) World.RenderEntityManager.IsHiddenHitbox = !World.RenderEntityManager.IsHiddenHitbox; // F3+B
            if (key == 67 && keyF3) Debug.IsDrawServerChunk = !Debug.IsDrawServerChunk; // F3+C
            if (key == 71 && keyF3) World.WorldRender.ChunkCursorHiddenShow(); // F3+G
            else if (key == 114) keyF3 = true; // F3
            else if (key == 27 || key == 18) World.ClientMain.Screen.InGameMenu(); // Esc или Alt
            else if (key == 116) ClientMain.Player.ViewCameraNext(); // F5
            else if (key == 118)
            {
                //Debug.IsDrawFrustumCulling = !Debug.IsDrawFrustumCulling; // F7
                //ChunkBase chunk = World.GetChunk(ClientMain.Player.GetChunkPos());
                //chunk.Light.StartRecheckGaps();
            }
            else if (key == 119) Debug.IsDrawVoxelLine = !Debug.IsDrawVoxelLine; // F8
            else if (key == 75) ClientMain.Player.Kill(); // K
            else if (key == 82)
            {
                ChunkBase chunk = World.GetChunk(ClientMain.Player.GetChunkPos());
                for (int y = 0; y < ChunkBase.COUNT_HEIGHT; y++)
                {
                    chunk.ModifiedToRender(y); // R
                }
            }
            //else if (key == 117) // F6
            else ClientMain.Player.InputAdd(KeyActionToInput(key));
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
            else ClientMain.Player.InputRemove(KeyActionToInput(key));
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
