using System.Diagnostics;

namespace MvkServer.Util
{
    public class Profiler
    {
        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }

        protected Stopwatch stopwatch = new Stopwatch();
        protected string profilingSection;
        protected bool profilingEnabled = false;

        public Profiler(Logger log)
        {
            Log = log;
            stopwatch.Start();
        }

        public void StartSection(string name)
        {
            profilingSection = name;
            profilingEnabled = true;
            stopwatch.Restart();
        }

        public void EndSection()
        {
            if (profilingEnabled)
            {
                long time = stopwatch.ElapsedTicks / MvkStatic.TimerFrequency;

                if (time > 100) // больше 100 мс
                {
                    Log.Log("Что-то слишком долго! {0} заняло приблизительно {1} мс", profilingSection, time);
                }
            }
        }

        public void EndStartSection(string name)
        {
            EndSection();
            StartSection(name);
        }
    }
}
