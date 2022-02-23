using MvkServer.Util;
using System;
using System.Diagnostics;
using System.Threading;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект тактов и кадров в секунду
    /// </summary>
    public class Ticker
    {
        /// <summary>
        /// Запущен ли поток
        /// </summary>
        public bool IsRuning { get; private set; } = false;
        /// <summary>
        /// Желаемое количество кадров в секунду
        /// </summary>
        public int WishFrame { get; private set; } = 60;
        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// </summary>
        public float Interpolation { get; private set; } = 0;

        /// <summary>
        /// Желаемое количество тактов в секунду
        /// </summary>
        private readonly int wishTick = 20;
        /// <summary>
        /// Интервал в тиках для кадра
        /// </summary>
        private long intervalFrame;
        /// <summary>
        /// Интервал в тиках для такта
        /// </summary>
        private readonly long intervalTick;
        /// <summary>
        /// Время для сна без нагрузки для кадра
        /// </summary>
        private long sleepFrame;
        /// <summary>
        /// Время для сна без нагрузки для такта
        /// </summary>
        private readonly long sleepTick;
        /// <summary>
        /// Максимальный fps
        /// </summary>
        private bool isMax = false;

        public Ticker()
        {
            SetWishFrame(WishFrame);
            intervalTick = Stopwatch.Frequency / wishTick;
            sleepTick = intervalTick / MvkStatic.TimerFrequency;
        }

        /// <summary>
        /// Задать желаемый фпс
        /// </summary>
        public void SetWishFrame(int frame)
        {
            if (frame > 250)
            {
                isMax = true;
            }
            else
            {
                isMax = false;
                WishFrame = frame;
                intervalFrame = Stopwatch.Frequency / WishFrame;
                sleepFrame = intervalFrame / MvkStatic.TimerFrequency;
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
            long lastTimeFrame = stopwatch.ElapsedTicks;
            long lastTimeTick = lastTimeFrame;

            long currentTimeBegin, currentTime, cl;
            int sleepFrame = 0;
            int sleepTick = 0;

            while (IsRuning)
            {
                currentTimeBegin = stopwatch.ElapsedTicks;

                // Проверяем такт
                cl = currentTimeBegin - lastTimeTick;
                if (cl < 0) cl = 0;

                if (cl >= intervalTick)
                {
                    lastTimeTick = currentTimeBegin;
                    OnTick();
                    currentTime = stopwatch.ElapsedTicks;
                    cl = (currentTime - lastTimeTick) / MvkStatic.TimerFrequency;
                    sleepTick = (int)(this.sleepTick - cl);
                    Interpolation = 0;
                }
                else
                {
                    //  Пересчитываем остаточный сон такта
                    sleepTick = (int)(this.sleepTick - (cl / MvkStatic.TimerFrequency));
                    Interpolation = cl / (float)MvkStatic.TimerFrequencyTps;
                    if (Interpolation > 1f) Interpolation = 1f;
                    if (Interpolation < 0) Interpolation = 0f;
                }

                // Проверяем кадр
                if (isMax)
                {
                    OnFrame();
                }
                else
                {
                    cl = currentTimeBegin - lastTimeFrame;
                    if (cl < 0) cl = 0;

                    if (cl >= intervalFrame)
                    {
                        lastTimeFrame = currentTimeBegin;
                        OnFrame();
                        currentTime = stopwatch.ElapsedTicks;
                        cl = (currentTime - lastTimeFrame) / MvkStatic.TimerFrequency;
                        sleepFrame = (int)(this.sleepFrame - cl);
                    } else
                    {
                        //  Пересчитываем остаточный сон кадра
                        sleepFrame = (int)(this.sleepFrame - (cl / MvkStatic.TimerFrequency));
                    }

                    // Находим на именьшое засыпание и засыпаем
                    int sleep = Mth.Min(sleepFrame, sleepTick);
                    if (sleep > 0) Thread.Sleep(sleep);
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
        /// Событие кадра
        /// </summary>
        public event EventHandler Frame;
        protected virtual void OnFrame() => Frame?.Invoke(this, new EventArgs());

        /// <summary>
        /// Событие закрыть
        /// </summary>
        public event EventHandler Closeded;
        protected virtual void OnCloseded() => Closeded?.Invoke(this, new EventArgs());
    }
}
