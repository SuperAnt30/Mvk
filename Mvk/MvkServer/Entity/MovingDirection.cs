
using System;

namespace MvkServer.Entity
{
    /// <summary>
    /// Ключевой объект перемещения движения конкретного направления
    /// </summary>
    //public class MovingDirection : Interpolis
    //{
    //    /// <summary>
    //    /// Перемещение -1..0..1
    //    /// </summary>
    //    public float Moving { get; protected set; }

    //    protected void SetMoving(float m)
    //    {
    //        Moving = m;
    //        OnLookAtChanged();
    //    }

    //    /// <summary>
    //    /// Значение является положительным
    //    /// </summary>
    //    public bool Plus => Moving > 0;

    //    /// <summary>
    //    /// Значение является отрицательным
    //    /// </summary>
    //    public bool Minus => Moving < 0;

    //    /// <summary>
    //    /// Плюс
    //    /// </summary>
    //    public bool MovingPlus()
    //    {
    //        if (Moving <= 0 || !IsAction)
    //        {
    //            SetMoving(.01f);
    //            ActionRun();
    //            return true;
    //        }
    //        return false;
    //    }
    //    /// <summary>
    //    /// Минус
    //    /// </summary>
    //    public bool MovingMinus()
    //    {
    //        if (Moving >= 0 || !IsAction)
    //        {
    //            SetMoving(-.01f);
    //            ActionRun();
    //            return true;
    //        }
    //        return false;
    //    }

    //    /// <summary>
    //    /// Ноль
    //    /// </summary>
    //    public bool Zero()
    //    {
    //        if (!IsZero)
    //        {
    //            if (Moving > 0) SetMoving(1f);
    //            if (Moving < 0) SetMoving(-1f);
    //            ActionStop();
    //            return true;
    //        }
    //        return false;
    //    }

    //    /// <summary>
    //    /// Стоим ли мы
    //    /// </summary>
    //    public bool IsZero => Moving == 0;

    //    public override void Update()
    //    {
    //        if (IsAction)
    //        {
    //            if (Moving > 0)
    //            {
    //                SetMoving(1f - ActionCoefficient());
    //            }
    //            else if (Moving < 0)
    //            {
    //                SetMoving(-1f + ActionCoefficient());
    //            }
    //            if (Moving >= 1f || Moving <= -1f) ActionStop();
    //        }
    //        base.Update();
    //    }

    //    #region event

    //    /// <summary>
    //    /// Событие изменена позиция камеры
    //    /// </summary>
    //    public event EventHandler LookAtChanged;

    //    /// <summary>
    //    /// Изменена позиция камеры
    //    /// </summary>
    //    protected void OnLookAtChanged()
    //    {
    //        LookAtChanged?.Invoke(this, new EventArgs());
    //    }

    //    #endregion
    //}
}
