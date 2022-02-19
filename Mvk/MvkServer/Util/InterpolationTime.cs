using MvkServer.Util;
using System.Diagnostics;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект для определения индекса интерполяции TPS (50 мс)
    /// </summary>
    public class InterpolationTime
    {
        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        private Stopwatch stopwatchTps = new Stopwatch();

        private long currentTime;

        /// <summary>
        /// Запуск
        /// </summary>
        public void Start()
        {
            stopwatchTps.Start();
            currentTime = stopwatchTps.ElapsedTicks;
        }
        /// <summary>
        /// Перезапуск
        /// </summary>
        public void Restart()
        {
            currentTime = stopwatchTps.ElapsedTicks;
            //stopwatchTps.Restart();
        }

        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// где 0 это финиш, 1 начало
        /// </summary>
        public float TimeIndex()
        {
            long realTime = stopwatchTps.ElapsedTicks;

            long differenceTime = realTime - currentTime;
            if (differenceTime < 0) differenceTime = 0;

            float f = differenceTime / (float)MvkStatic.TimerFrequencyTps;

           // float f = stopwatchTps.ElapsedTicks / (float)MvkStatic.TimerFrequencyTps;
            if (f > 1f) return 1f;
            if (f < 0) return 0;
            return f;
        }
    }
}
