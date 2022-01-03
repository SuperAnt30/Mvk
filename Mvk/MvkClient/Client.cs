using MvkAssets;
using MvkClient.Actions;
using MvkClient.Audio;
using MvkClient.Gui;
using MvkClient.Network;
using MvkClient.Renderer;
using MvkClient.Util;
using MvkClient.World;
using MvkServer;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Util;
using SharpGL;
using System;
using System.Diagnostics;

namespace MvkClient
{
    public class Client
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient World { get; protected set; }
        /// <summary>
        /// Объект звуков
        /// </summary>
        public AudioBase Sample { get; protected set; } = new AudioBase();
        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; protected set; } = 0;
        /// <summary>
        /// Screen Gui
        /// </summary>
        public GuiScreen Screen { get; private set; }

        /// <summary>
        /// Тикер Fps
        /// </summary>
        protected Ticker tickerFps;
        /// <summary>
        /// Тикер Fps
        /// </summary>
        protected Ticker tickerTps;
        /// <summary>
        /// Локальный сервер
        /// </summary>
        protected LocalServer locServer;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected ProcessClientPackets packets;
        /// <summary>
        /// Закрывается ли окно
        /// </summary>
        protected bool isClosing = false;
        /// <summary>
        /// Пауза в игре
        /// </summary>
        protected bool isGamePaused = false;
        /// <summary>
        /// Объект времени с момента запуска проекта
        /// </summary>
        private static Stopwatch stopwatch = new Stopwatch();

        #region EventsWindow

        /// <summary>
        /// Создание окна
        /// #1
        /// </summary>
        public void Initialize()
        {
            Sample.Initialize();
            glm.Initialized();
            MvkStatic.Initialized();
            Screen = new GuiScreen(this);
            Screen.Changed += Screen_Changed;
            packets = new ProcessClientPackets(this);

            tickerFps = new Ticker();
            tickerFps.Tick += TickerFps_Tick;
            tickerFps.Closeded += (sender, e) => OnCloseded();

            tickerTps = new Ticker();
            tickerTps.SetWishTick(20);
            tickerTps.Tick += TickerTps_Tick;

            locServer = new LocalServer();
            locServer.ObjectKeyTick += Server_ObjectKeyTick;
            locServer.RecievePacket += (sender, e) => packets.ReceiveBufferClient(e.Packet.Bytes);
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
            Screen.LoadingSetMax(loading.Count);
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
            if (locServer.IsStartWorld)
            {
                ExitingWorld("");
                return true;
            }
            if (tickerFps.IsRuning)
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
            stopwatch.Start();
            GLWindow.Initialize(gl);
            Screen.Begin();
        }

        /// <summary>
        /// Прорисовка каждого кадра
        /// </summary>
        public void GLDraw() => GLWindow.Draw(this);

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void GLResized(int width, int height)
        {
            GLWindow.Resized(width, height);
            Screen.Resized();
            if (IsGamePlay) World.Player.UpProjection();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void KeyDown(int key)
        {
            //locServer.TrancivePacket(new PacketTFFTest(key.ToString()));
            Debug.DInt = key;
            if (key == 114) // F3
            {
                Debug.IsDraw = !Debug.IsDraw;
            }
            
            if (IsGamePlayAction())
            {
                if (key == 27) // Esc
                {
                    Screen.InGameMenu();
                }
                else
                {
                    switch(key)
                    {
                        case 65: World.KeyLife.Left(); break;
                        case 68: World.KeyLife.Right(); break;
                        case 87: World.KeyLife.Forward(); break;
                        case 83: World.KeyLife.Back(); break;
                        case 32: World.KeyLife.Up(); break;
                        case 16: World.KeyLife.Down(); break;
                        case 17: World.KeyLife.Sprinting(); break;
                    }
                }
            }
        }
        /// <summary>
        /// Нажата клавиша в char формате
        /// </summary>
        public void KeyPress(char key)
        {
            Screen.KeyPress(key);
        }

        /// <summary>
        /// Отпущена клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void KeyUp(int key)
        {
            if (key == 65 || key == 68) World.KeyLife.CancelHorizontal();
            else if (key == 87 || key == 83) World.KeyLife.CancelVertical();
            else if (key == 32 || key == 16) World.KeyLife.CancelUpDown();
            else if (key == 17) World.KeyLife.CancelSprinting();
        }

        /// <summary>
        /// Нажатие клавиши мышки
        /// </summary>
        public void MouseDown(MouseButton button, int x, int y)
        {
            Screen.MouseDown(button, x, y);
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y)
        {
            Screen.MouseUp(button, x, y);
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
            if (IsGamePlayAction())
            {
                World.Player.MouseMove(deltaX, deltaY);
                return true;
            }
            Screen.MouseMove(x, y);
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
        public void ThreadReceive(ObjectKeyEventArgs e)
        {
            switch(e.Key)
            {
                case ObjectKey.LoadStep: Screen.LoadingStep(); break; // Шаг для загрузчиков
                case ObjectKey.LoadStepTexture:  // Шаг загрузки текстуры
                    GLWindow.Texture.InitializeKey(e.Tag as BufferedImage);
                    Screen.LoadingStep();
                    break;
                case ObjectKey.LoadedMain: Screen.LoadingMainEnd(); break; // Закончена первоночальная загрузка
                case ObjectKey.ServerStoped: // Мир сервера остановлен
                    if (e.Tag == null || e.Tag.ToString() == "")
                    {
                        if (isClosing) WindowClosing(); else Screen.MainMenu();
                    }
                    else Screen.ScreenError(e.Tag.ToString()); // выход с ошибкой
                    break;
                case ObjectKey.Error: Screen.ScreenError(e.Tag.ToString()); break;// Ошибка
                case ObjectKey.RenderDebug: Debug.RenderDebug(); break;
            }
        }

        /// <summary>
        /// Загрузить сетевой мир
        /// </summary>
        /// <param name="ip">адрес</param>
        public void LoadWorldNet(string ip)
        {
            Screen.ScreenProcess(Language.T("gui.process"));
            locServer.StartServerNet(ip);
            World = new WorldClient(this);
        }
        /// <summary>
        /// Загрузить мир
        /// </summary>
        /// <param name="slot">Номер слота</param>
        public void LoadWorld(int slot)
        {
            locServer.StartServer(slot);
            World = new WorldClient(this);

            //TODO:: надо отсюда начать запускать сервер, который создаст мир, и продублирует на клиенте мир.
            // Продумать tps только на стороне сервера, но должна быть сенхронизация с клиентом
            // Синхронизация времени раз в секунду
        }

        private void Server_ObjectKeyTick(object sender, ObjectKeyEventArgs e)
        {
            if (e.Key == ObjectKey.LoadCountWorld)
            {
                Screen.LoadingSetMax((int)e.Tag);
            }
            else
            {
                OnThreadSend(e);
            }
        }

        /// <summary>
        /// Мир загружен один игрок
        /// </summary>
        public void LoadedWorld()
        {
            // ставим экран загрузки
            Screen.ScreenProcess(Language.T("gui.process"));
        }

        /// <summary>
        /// Выход с мира
        /// </summary>
        /// <param name="error">ошибка</param>
        public void ExitingWorld(string error)
        {
            tickerTps.Stoping();
            StringDebugTps();
            // ставим экран сохранения
            Screen.ScreenProcess(Language.T("gui.saving"));
            // отправялем на сервер, выход мира, с возможной ошибкой
            locServer.ExitingWorld(error);
            World.StopWorldDelete();
            World = null;
        }

        /// <summary>
        /// Убрать Gui, переход в режим игры при старте
        /// </summary>
        public void GameModeBegin(uint timer)
        {
            TickCounter = timer;
            Screen.GameMode();
            tickerTps.Start();
        }

        

        /// <summary>
        /// Отправить пакет на сервер
        /// </summary>
        public void TrancivePacket(IPacket packet) => locServer.TrancivePacket(packet);

        /// <summary>
        /// Запущен ли локальный сервер
        /// </summary>
        public bool IsServerLocalRun() => locServer.IsLoacl ? locServer.IsStartWorld : false;
        /// <summary>
        /// Открыта ли сеть
        /// </summary>
        public bool IsOpenNet() => locServer.IsOpenNet();
        /// <summary>
        /// Открыть сеть
        /// </summary>
        public void OpenNet() => locServer.OpenNet();
        /// <summary>
        /// Режим игры
        /// </summary>
        public bool IsGamePlay => tickerTps.IsRuning;
        /// <summary>
        /// Режим игры, режим активного управления 3d
        /// </summary>
        public bool IsGamePlayAction() => IsGamePlay && Screen.IsEmptyScreen();

        /// <summary>
        /// Дебага, формируется по запросу
        /// </summary>
        protected void StringDebugTps() => Debug.strClient = (!IsGamePlay || World == null) ? "" : World.ToStringDebug();

        /// <summary>
        /// Изменить таймер
        /// </summary>
        //public void SetTickCounter(uint timer) => TickCounter = timer;

        /// <summary>
        /// Такт каждого ФПС
        /// </summary>
        private void TickerFps_Tick(object sender, EventArgs e)
        {
            if (!Screen.IsEmptyScreen())
            {
                // GUI что-то есть
                OnThreadSend(new ObjectKeyEventArgs(ObjectKey.RenderDebug));
            }
            OnDraw();
        }

        /// <summary>
        /// Локальный ТПС 20
        /// </summary>
        private void TickerTps_Tick(object sender, EventArgs e)
        {
            if (!isGamePaused)
            {
                TickCounter++;

                World.Tick();

                if (TickCounter % 4 == 0)
                {
                    // лог статистика за это время

                    if (MvkGlobal.IS_DRAW_DEBUG_CHUNK)
                    {
                        // отладка чанков
                        Debug.ListChunks.listChunkPlayer = World.ChunkPr.GetListDebug();
                        Debug.ListChunks.isRender = true;
                    }
                }
            }
            
            StringDebugTps();
            if (Screen.IsEmptyScreen())
            {
                // GUI нет
                OnThreadSend(new ObjectKeyEventArgs(ObjectKey.RenderDebug));
            }
        }

        /// <summary>
        /// Изменён GUI скрин
        /// </summary>
        private void Screen_Changed(object sender, EventArgs e)
        {
            // определить паузу
            isGamePaused = IsGamePlay && Screen.IsScreenPause() && !locServer.IsOpenNet();
            locServer.SetGamePauseSingle(isGamePaused);
        }

        /// <summary>
        /// Получить время в милисекундах с момента запуска проекта
        /// </summary>
        public static long Time() => stopwatch.ElapsedMilliseconds;

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
        public event ObjectKeyEventHandler ThreadSend;
        protected virtual void OnThreadSend(ObjectKeyEventArgs e) => ThreadSend?.Invoke(this, e);

        #endregion
    }
}
