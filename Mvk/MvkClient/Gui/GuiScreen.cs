using MvkClient.Actions;
using MvkClient.Setitings;

namespace MvkClient.Gui
{
    public class GuiScreen
    {
        /// <summary>
        /// Активный скрин
        /// </summary>
        protected Screen screen;

        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        public GuiScreen(Client client)
        {
            ClientMain = client;
            screen = new ScreenBeginLoading(client);
        }

        /// <summary>
        /// Пустой ли скрин
        /// </summary>
        public bool IsEmptyScreen() => screen == null;

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void Resized()
        {
            if (screen != null) screen.Resized();
        }

        /// <summary>
        /// Прорисовка нужного скрина если это надо
        /// </summary>
        public void DrawScreen()
        {
            if (screen != null) screen.Draw();
        }

        /// <summary>
        /// Запуск первого скрина
        /// </summary>
        public void Begin() => screen.Initialize();

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        public void MouseMove(int x, int y)
        {
            if (screen != null) screen.MouseMove(x, y);
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public void MouseDown(MouseButton button, int x, int y)
        {
            if (screen != null) screen.MouseDown(button, x, y);
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y)
        {
            if (screen != null) screen.MouseUp(button, x, y);
        }

        /// <summary>
        /// Заменить экран на другое меню
        /// </summary>
        public void Exchange(Screen screenNew)
        {
            if (screen != null) screen.Delete();
            ClientMain.SetWishFps(20);
            screen = screenNew;
            screen.Finished += MenuScreen_Finished;
            screen.Initialize();
        }

        /// <summary>
        /// Активация меню во время игры
        /// </summary>
        public void InGameMenu() => Exchange(new ScreenInGameMenu(ClientMain));

        /// <summary>
        /// Удалить экран
        /// </summary>
        protected void Delete()
        {
            screen.Delete();
            screen = null;
            // Задать фпс
            ClientMain.SetWishFps(Setting.Fps);
        }

        #region Loading

        /// <summary>
        /// Задать максимальную загрузку компонентов
        /// </summary>
        public void LoadingSetMax(int max)
        {
            if (screen.GetType() == typeof(ScreenBeginLoading))
            {
                ((ScreenBeginLoading)screen).SetMax(max);
            }
            else if (screen.GetType() == typeof(ScreenWorldLoading))
            {
                ((ScreenWorldLoading)screen).SetMax(max);
            }
        }
        /// <summary>
        /// Шаг загрузки
        /// </summary>
        public void LoadingStep()
        {
            if (screen.GetType() == typeof(ScreenBeginLoading))
            {
                ((ScreenBeginLoading)screen).Step();
            }
            else if (screen.GetType() == typeof(ScreenWorldLoading))
            {
                ((ScreenWorldLoading)screen).Step();
            }
        }
        /// <summary>
        /// Окончание загрузки, переходим в главное меню
        /// </summary>
        public void LoadingMainEnd() => Exchange(new ScreenMainMenu(ClientMain));

        /// <summary>
        /// Окончание загрузки, переходим в главное меню
        /// </summary>
        public void LoadingWorldEnd() => Delete();

        #endregion

        private void MenuScreen_Finished(object sender, ScreenEventArgs e)
        {
            EnumScreenKey key = e.Key;
            if (key == EnumScreenKey.Options)
            {
                Exchange(new ScreenOptions(ClientMain, e.Where));
            }
            else if (key == EnumScreenKey.Main)
            {
                Exchange(new ScreenMainMenu(ClientMain));
            }
            else if (key == EnumScreenKey.SinglePlayer)
            {
                Exchange(new ScreenSingle(ClientMain, e.Slot));
            }
            else if (key == EnumScreenKey.YesNo)
            {
                Exchange(new ScreenYesNo(ClientMain, e.Text, e.Where, e.Slot));
            }
            else if (key == EnumScreenKey.WorldBegin)
            {
                // Запуск загрузки мира
                Exchange(new ScreenWorldLoading(ClientMain, e.Slot));
                ClientMain.LoadWorld(e.Slot);
            }
            else if (key == EnumScreenKey.World)
            {
                Delete();
            }
            else if (key == EnumScreenKey.InGameMenu)
            {
                InGameMenu();
            }

        }
    }
}
