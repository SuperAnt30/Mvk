using MvkServer.Util;
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
        public bool IsRuning { get; protected set; } = false;
        /// <summary>
        /// Значение желаемого тика
        /// </summary>
        public int WishTick { get; protected set; } = 20;
        

        private long interval;
        private long sleep;
        /// <summary>
        /// Максимальный fps
        /// </summary>
        private bool isMax = false;

        public Ticker() => SetWishTick(WishTick);

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
                sleep = interval / MvkStatic.TimerFrequency;
            }
        }

        /// <summary>
        /// Запуск
        /// </summary>
        public void Start()
        {
            Thread myThread = new Thread(RunThreadTick);
            IsRuning = true;
            myThread.Start();
        }

        /// <summary>
        /// Останавливаем
        /// </summary>
        public void Stoping() => IsRuning = false;

        /// <summary>
        /// Метод запуска для отдельного потока, такты
        /// </summary>
        protected void RunThreadTick()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            long lastTime = stopwatch.ElapsedTicks;
            while (IsRuning)
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
                        cl = (currentTime - lastTime) / MvkStatic.TimerFrequency;
                        // С минус 1 точнее бъёт такт, но нагрузка на проц возрастает
                        int sleep = (int)(this.sleep - cl);// - 1;
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
