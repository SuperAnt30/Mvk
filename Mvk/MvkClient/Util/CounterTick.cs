using System;
using System.Diagnostics;

namespace MvkClient.Util
{
    //public class CounterTick
    //{
    //    /// <summary>
    //    /// Результат количества тиков за последнюю секунду
    //    /// </summary>
    //    public int CountTick { get; protected set; } = 0;
    //    /// <summary>
    //    /// Тут хранится время, прошедшее с последнего кадра
    //    /// </summary>
    //    protected long lastTime;
    //    /// <summary>
    //    /// Объект для точного замера времени
    //    /// </summary>
    //    protected Stopwatch stopwatch = new Stopwatch();
    //    /// <summary>
    //    /// наши фпс
    //    /// </summary>
    //    protected int framesPerSecond = 0;

    //    public CounterTick()
    //    {
    //        stopwatch.Start();
    //        lastTime = stopwatch.ElapsedTicks;
    //    }

    //    /// <summary>
    //    /// Эта функция рассчитывает FPS и выводит их
    //    /// </summary>
    //    public void CalculateFrameRate()
    //    {
    //        //Ниже мы создадим несколько статичных переменных, т.к. хотим, чтобы они сохраняли своё
    //        //значение после завершения работы ф-ии. Мы могли бы сделать их глобальными, но это будет
    //        //излишним.

    //        //Тут мы получаем текущий tick в милисекундах
    //        long currentTime = stopwatch.ElapsedTicks;

    //        //Увеличиваем счетчик кадров
    //        framesPerSecond++;

    //        //Теперь вычтем из текущего времени последнее запомненное время. Если результат больше 1000 мл секунд,
    //        //это значит, что секунда прошла и нужно вывести новый FPS.
    //        if (currentTime - lastTime >= Stopwatch.Frequency)
    //        {
    //            //Устанавливаем lastTime в текущее время. Теперь оно будет использоватся как предидущее время
    //            //для след. секунды.
    //            lastTime = currentTime;

    //            // Установим FPS для вывода:
    //            CountTick = framesPerSecond;

    //            // Сбросим FPS временного счётчика
    //            framesPerSecond = 0;

    //            OnTick();
    //        }
    //    }

    //    /// <summary>
    //    /// Событие такта
    //    /// </summary>
    //    public event EventHandler Tick;
    //    protected void OnTick() => Tick?.Invoke(this, new EventArgs());
    //}
}
