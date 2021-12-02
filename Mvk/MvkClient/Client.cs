using MvkClient.Actions;
using MvkClient.Audio;
using MvkClient.Renderer;
using MvkClient.Util;
using MvkServer.Glm;
using SharpGL;
using System;

namespace MvkClient
{
    public class Client
    {
        /// <summary>
        /// Объект звуков
        /// </summary>
        protected AudioBase audio = new AudioBase();
        /// <summary>
        /// Счётчик FPS
        /// </summary>
        protected CounterTick counterFps;
        /// <summary>
        /// Тикер Fps
        /// </summary>
        protected Ticker tickerFps;
        /// <summary>
        /// Загрузчик
        /// </summary>
        protected LoadingScreenRenderer loadingScreen;

        protected bool isLoading = true;


        #region EventsWindow

        /// <summary>
        /// Создание окна
        /// #1
        /// </summary>
        public void Initialize()
        {
            audio.Initialize();
            glm.Initialized();
            loadingScreen = new LoadingScreenRenderer();

            counterFps = new CounterTick();
            counterFps.Tick += CounterFps_Tick;

            tickerFps = new Ticker();
            tickerFps.Tick += (sender, e) => OnDraw();
            tickerFps.Closeded += (sender, e) => OnCloseded();
        }

        /// <summary>
        /// Загружено окно
        /// #3
        /// </summary>
        public void WindowLoad()
        {
            tickerFps.Start();

            // Загрузка
            Loading loading = new Loading();
            loadingScreen.Start(loading.Count);
            loading.Tick += (sender, e) => OnThreadSend(new ObjectEventArgs(ObjectKey.LoadTick));
            loading.LoadStart();
        }

        /// <summary>
        /// Получить события из других пакетов
        /// </summary>
        public void ThreadReceive(ObjectEventArgs e)
        {
            if (e.Key == ObjectKey.LoadTick)
            {
                if (!loadingScreen.Next())
                {
                    // TODO:: запус меню
                    OnCloseded();
                }
            }
        }

        private void CounterFps_Tick(object sender, EventArgs e) => Debug.Fps = counterFps.CountTick;

        /// <summary>
        /// Начало закрытия окна
        /// </summary>
        /// <returns>true - отменить закрытие</returns>
        public bool WindowClosing()
        {
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
        }

        /// <summary>
        /// Прорисовка каждого кадра
        /// </summary>
        public void GLDraw()
        {
            GLWindow.DrawBegin();
            counterFps.CalculateFrameRate();
            loadingScreen.Draw();
            GLWindow.DrawEnd();
        }

        /// <summary>
        /// Изменён размер окна
        /// </summary>
        public void GLResized(int width, int height)
        {
            GLWindow.Resized(width, height);
            loadingScreen.Resized();
        }

        /// <summary>
        /// Нажата клавиша
        /// </summary>
        /// <param name="key">индекс клавиши</param>
        public void KeyDown(int key)
        {
            Debug.DInt = key;
            if (key == 114) // F3
            {
                Debug.IsDraw = !Debug.IsDraw;
            }
            if (key == 107) // +
            {
                if (tickerFps.WishTick < 1000) tickerFps.SetWishTick(tickerFps.WishTick + 10);
                Debug.DInt = tickerFps.WishTick;
            }
            if (key == 109) // -
            {
                if (tickerFps.WishTick > 10) tickerFps.SetWishTick(tickerFps.WishTick - 10);
                Debug.DInt = tickerFps.WishTick;
            }
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

            //TestSound();
        }

        /// <summary>
        /// Отпущена клавиша мышки
        /// </summary>
        public void MouseUp(MouseButton button, int x, int y)
        {

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

        public void TestSound()
        {
            audio.PlaySound("other.door_open");
        }

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
    }
}
