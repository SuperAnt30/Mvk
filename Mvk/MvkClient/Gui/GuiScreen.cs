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
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            if (screen != null) screen.KeyPress(key);
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
        /// Главного меню
        /// </summary>
        public void MainMenu() => Exchange(new ScreenMainMenu(ClientMain));
        /// <summary>
        /// Сохранение мира
        /// </summary>
        public void SavingWorld() => Exchange(new ScreenWorldSaving(ClientMain));
        /// <summary>
        /// Окно ошибки
        /// </summary>
        public void ScreenError(string text) => Exchange(new ScreenError(ClientMain, text));
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
            switch(e.Key)
            {
                case EnumScreenKey.Options: Exchange(new ScreenOptions(ClientMain, e.Where)); break;
                case EnumScreenKey.Main: MainMenu(); break;
                case EnumScreenKey.SinglePlayer: Exchange(new ScreenSingle(ClientMain, e.Slot)); break;
                case EnumScreenKey.Multiplayere: Exchange(new ScreenMultiplayere(ClientMain)); break;
                case EnumScreenKey.Connection: ClientMain.LoadWorldNet(e.Tag.ToString()); break;
                case EnumScreenKey.YesNo: Exchange(new ScreenYesNo(ClientMain, e.Text, e.Where, e.Slot)); break;
                case EnumScreenKey.WorldBegin:
                    // Запуск загрузки мира
                    Exchange(new ScreenWorldLoading(ClientMain, e.Slot));
                    ClientMain.LoadWorld(e.Slot);
                    break;
                case EnumScreenKey.World: Delete(); break;
                case EnumScreenKey.InGameMenu: InGameMenu(); break;
                case EnumScreenKey.WorldSaving: SavingWorld();  break;
            }
        }
    }
}
