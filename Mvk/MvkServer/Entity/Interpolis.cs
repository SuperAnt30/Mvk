namespace MvkServer.Entity
{
    /// <summary>
    /// Абстрактный клас интерполяции
    /// </summary>
    //public abstract class Interpolis
    //{
    //    /// <summary>
    //    /// Количество тактов на действие, 20 = 1 секунда
    //    /// </summary>
    //    protected int countTact = 20;
    //    /// <summary>
    //    /// Тикущий так
    //    /// </summary>
    //    protected long tack = 0;

    //    protected bool aсtionEnd = false;

    //    /// <summary>
    //    /// Задать количество тактов на действие, 20 = 1 секунда
    //    /// </summary>
    //    public void CountTack(int countTact) => this.countTact = countTact;

    //    /// <summary>
    //    /// Коэффициент действия
    //    /// </summary>
    //    public float ActionCoefficient() => countTact > 0 ? tack / (float)countTact : 0;

    //    /// <summary>
    //    /// Активное действие
    //    /// </summary>
    //    public bool IsAction => tack > 0;

    //    /// <summary>
    //    /// Активировать действие
    //    /// </summary>
    //    public void ActionRun() => tack = countTact;

    //    /// <summary>
    //    /// Остановить действие
    //    /// </summary>
    //    public void ActionStop() => tack = 0;

    //    /// <summary>
    //    /// Обновление каждый такт
    //    /// </summary>
    //    public virtual void Update()
    //    {
    //        if (IsAction)
    //        {
    //            tack--;
    //        }
    //    }
    //}
}
