using System;
using System.Diagnostics;
using System.Threading;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект тактов в секунду
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// Запущен ли поток
        /// </summary>
        public bool IsRuningFps { get; protected set; } = false;
        /// <summary>
        /// Значение желаемого тика
        /// </summary>
        public int WishTick { get; protected set; } = 20;
        /// <summary>
        /// Получает частоту таймера в виде количества тактов в милисекунду
        /// </summary>
        public static long Frequency { get; protected set; } = 10000;

        private long interval;
        private long sleepFps;
        /// <summary>
        /// Максимальный fps
        /// </summary>
        private bool isMax = false;

        public Ticker()
        {
            Frequency = Stopwatch.Frequency / 1000;
            SetWishTick(WishTick);
        }

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishTick(int tick)
        {
            if (tick > 250)
            {
                isMax = true;
            }
            else
            {
                isMax = false;
                WishTick = tick;
                interval = Stopwatch.Frequency / WishTick;
                sleepFps = interval / Frequency;
            }
        }

        /// <summary>
        /// Запуск
        /// </summary>
        public void Start()
        {
            Thread myThread = new Thread(RunThreadTick);
            IsRuningFps = true;
            myThread.Start();
        }

        /// <summary>
        /// Останавливаем
        /// </summary>
        public void Stoping() => IsRuningFps = false;

        /// <summary>
        /// Метод запуска для отдельного потока, такты
        /// </summary>
        protected void RunThreadTick()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long lastTime = stopwatch.ElapsedTicks;
            while (IsRuningFps)
            {
                if (isMax)
                {
                    OnTick();
                }
                else
                {
                    long currentTime = stopwatch.ElapsedTicks;
                    long cl = currentTime - lastTime;
                    if (cl >= interval)
                    {
                        lastTime = currentTime;
                        OnTick();
                        currentTime = stopwatch.ElapsedTicks;
                        cl = (currentTime - lastTime) / Frequency;
                        // С минус 1 точнее бъёт такт, но нагрузка на проц возрастает
                        int sleep = (int)(sleepFps - cl);// - 1;
                        if (sleep > 0) Thread.Sleep(sleep);
                    }
                }
            }
            OnCloseded();
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event EventHandler Tick;
        protected virtual void OnTick() => Tick?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        protected virtual void OnCloseded() => Closeded?.Invoke(this, new EventArgs());
    }
}
