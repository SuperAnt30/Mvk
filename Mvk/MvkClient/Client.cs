using MvkClient.Actions;
using MvkClient.Audio;
using MvkClient.Gui;
using MvkClient.Network;
using MvkClient.Renderer;
using MvkClient.Util;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using SharpGL;
using System;

namespace MvkClient
{
    public class Client
    {
        /// <summary>
        /// Объект звуков
        /// </summary>
        public AudioBase Sample { get; protected set; } = new AudioBase();
        /// <summary>
        /// Тикер Fps
        /// </summary>
        protected Ticker tickerFps;
        /// <summary>
        /// Screen Gui
        /// </summary>
        protected GuiScreen screen;
        /// <summary>
        /// Сервер в потоке
        /// </summary>
        protected ThreadServer server;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected ProcessClientPackets packets;
        /// <summary>
        /// Закрывается ли окно
        /// </summary>
        protected bool isClosing = false;

        #region EventsWindow

        /// <summary>
        /// Создание окна
        /// #1
        /// </summary>
        public void Initialize()
        {
            Sample.Initialize();
            glm.Initialized();
            screen = new GuiScreen(this);
            packets = new ProcessClientPackets(this);

            tickerFps = new Ticker();
            tickerFps.Tick += (sender, e) => OnDraw();
            tickerFps.Closeded += (sender, e) => OnCloseded();

            server = new ThreadServer();
            server.ObjectKeyTick += Server_ObjectKeyTick;
            server.RecievePacket += (sender, e) => packets.LocalReceivePacket(e.Packet.Bytes);
        }

        /// <summary>
        /// Загружено окно
        /// #3
        /// </summary>
        public void WindowLoad()
        {
            tickerFps.Start();

            // Загрузка
            Loading loading = new Loading(this);
            screen.LoadingSetMax(loading.Count);
            loading.Tick += (sender, e) => OnThreadSend(e);
            loading.LoadStart();
        }

        /// <summary>
        /// Начало закрытия окна
        /// </summary>
        /// <returns>true - отменить закрытие</returns>
        public bool WindowClosing()
        {
            isClosing = true;
            if (server.IsStartWorld)
            {
                ExitingWorld();
                return true;
            }
            if (tickerFps.IsRuningFps)
            {
                tickerFps.Stoping();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Деактивация окна
        /// </summary>
        public void WindowDeactivate()
        {

        }

        /// <summary>
        /// Контрол OpenGL в окне стал активным
        /// </summary>
        public void WindowGLEnter()
        {

        }

        /// <summary>
        /// Инициализировать, первый запуск OpenGL
        /// #2
        /// </summary>
        public void GLInitialize(OpenGL gl)
        {
            GLWindow.Initialize(gl);
            screen.Begin();
        }

        /// <summary>
        /// Прорисовка каждого кадра
        /// </summary>
        public void GLDraw()
        {
            GLWindow.DrawBegin();
            // тут игра
            //System.Threading.Thread.Sleep(15);
            // тут gui
            screen.DrawScreen();

            GLWindow.DrawEnd();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void GLResized(int width, int height)
        {
            GLWindow.Resized(width, height);
            screen.Resized();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void KeyDown(int key)
        {
            server.TrancivePacket(new PacketTest(key.ToString()));
            Debug.DInt = key;
            if (key == 114) // F3
            {
                Debug.IsDraw = !Debug.IsDraw;
            }
            if (key == 27) // Esc
            {
                if (screen.IsEmptyScreen()) screen.InGameMenu();
            }
        }
        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            screen.KeyPress(key);
        }

        /// <summary>
        /// Отпущена клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void KeyUp(int key)
        {

        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public void MouseDown(MouseButton button, int x, int y)
        {
            screen.MouseDown(button, x, y);
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y)
        {
            screen.MouseUp(button, x, y);
        }

        /// <summary>
        /// Перемещение мышки
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="deltaX">растояние по X от центра</param>
        /// <param name="deltaY">растояние по Y от центра</param>
        /// <returns>true - сбросить на центр</returns>
        public bool MouseMove(int x, int y, int deltaX, int deltaY)
        {
            screen.MouseMove(x, y);
            return false;
        }

        /// <summary>
        /// Вращение колёсика мыши
        /// </summary>
        /// <param name="delta">смещение</param>
        public void MouseWheel(int delta, int x, int y)
        {

        }

        #endregion

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishFps(int fps) => tickerFps.SetWishTick(fps);
        /// <summary>
        /// Получить желаемый фпс
        /// </summary>
        public int GetWishFps() => tickerFps.WishTick;

        /// <summary>
        /// Получить события из других пакетов
        /// </summary>
        public void ThreadReceive(ObjectEventArgs e)
        {
            switch(e.Key)
            {
                case ObjectKey.LoadStep: screen.LoadingStep(); break; // Шаг для загрузчиков
                case ObjectKey.LoadStepTexture:  // Шаг загрузки текстуры
                    GLWindow.Texture.InitializeKey(e.Tag as BufferedImage);
                    screen.LoadingStep();
                    break;
                case ObjectKey.LoadingStopMain: screen.LoadingMainEnd(); break; // Закончена первоночальная загрузка
                case ObjectKey.LoadingStopWorld: LoadedWorld(); break; // Мир загружен
                case ObjectKey.ServerStoped: // Мир сервера остановлен
                    if (isClosing) WindowClosing(); else screen.MainMenu();
                    break;
                case ObjectKey.Error: screen.ScreenError(e.Tag.ToString()); break;// Ошибка
            }
        }

        /// <summary>
        /// Загрузить сетевой мир
        /// </summary>
        /// <param name="ip">адрес</param>
        public void LoadWorldNet(string ip)
        {
            server.StartServerNet(ip);
        }
        /// <summary>
        /// Загрузить мир
        /// </summary>
        /// <param name="slot">Номер слота</param>
        public void LoadWorld(int slot)
        {
            server.StartServer(slot);

            //TODO:: надо отсюда начать запускать сервер, который создаст мир, и продублирует на клиенте мир.
            // Продумать tps только на стороне сервера, но должна быть сенхронизация с клиентом
            // Синхронизация времени раз в секунду
        }

        private void Server_ObjectKeyTick(object sender, ObjectEventArgs e)
        {
            if (e.Key == ObjectKey.LoadingCountWorld)
            {
                screen.LoadingSetMax((int)e.Tag);
            }
            else
            {
                OnThreadSend(e);
            }
        }

        /// <summary>
        /// Мир загружен
        /// </summary>
        public void LoadedWorld() => screen.LoadingWorldEnd();


        /// <summary>
        /// Выходим с мира
        /// </summary>
        public void ExitingWorld()
        {
            screen.SavingWorld();
            server.ExitingWorld();
        }

        #region Event

        /// <summary>
        /// Событие прорисовка кадра
        /// </summary>
        public event EventHandler Draw;
        protected virtual void OnDraw() => Draw?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        protected virtual void OnCloseded() => Closeded?.Invoke(this, new EventArgs());

        /// <summary>
        /// Из потока в основной поток
        /// </summary>
        public event ObjectEventHandler ThreadSend;
        protected virtual void OnThreadSend(ObjectEventArgs e) => ThreadSend?.Invoke(this, e);

        #endregion
    }
}
