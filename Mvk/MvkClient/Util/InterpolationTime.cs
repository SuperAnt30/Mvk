using MvkServer.Util;
using System.Diagnostics;

namespace MvkClient.Util
{
    ///// <summary>
    ///// Объект для определения индекса интерполяции TPS (50 мс)
    ///// </summary>
    //public class InterpolationTime
    //{
    //    /// <summary>
    //    /// Объект времени c последнего тпс
    //    /// </summary>
    //    protected Stopwatch stopwatchTps = new Stopwatch();

    //    /// <summary>
    //    /// Запуск
    //    /// </summary>
    //    public void Start() => stopwatchTps.Start();
    //    /// <summary>
    //    /// Перезапуск
    //    /// </summary>
    //    public void Restart() => stopwatchTps.Restart();

    //    /// <summary>
    //    /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
    //    /// где 0 это финиш, 1 начало
    //    /// </summary>
    //    public float TimeIndex()
    //    {
    //        float f = stopwatchTps.ElapsedTicks / (float)MvkStatic.TimerFrequencyTps;
    //        if (f > 1f) return 1f;
    //        if (f < 0) return 0;
    //        return f;
    //    }
    //}
}
