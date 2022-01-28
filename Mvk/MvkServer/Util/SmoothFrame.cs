namespace MvkServer.Util
{
    /// <summary>
    /// Объект плавного перемещения за TPS и FPS
    /// </summary>
    public class SmoothFrame
    {
        /// <summary>
        /// Тикущее значение
        /// </summary>
        public float Value { get; protected set; } = 0f;
        /// <summary>
        /// Значение для конечного кадра прорисовкт
        /// </summary>
        public float ValueFrame { get; protected set; }
        /// <summary>
        /// Последнее значение
        /// </summary>
        protected float valueLast;
        /// <summary>
        /// Значение для финишного расчёта
        /// </summary>
        protected float valueEnd = 0f;

        /// <summary>
        /// Количество требуемых тактов
        /// </summary>
        public int Count { get; protected set; } = 0;

        public SmoothFrame() { }
        public SmoothFrame(float value) => ValueFrame = Value = valueLast = valueEnd = value;

        /// <summary>
        /// Внести изменение
        /// </summary>
        /// <param name="value">Требуемое значение</param>
        /// <param name="count">Каличество тактов TPS до выполнения</param>
        public void Set(float value, int count)
        {
            if (valueEnd != value)
            {
                valueEnd = value;
                Count = count > Count ? count - Count : 1;
            }
        }

        /// <summary>
        /// Обновить в TPS
        /// </summary>
        public void Update()
        {
            if (valueEnd != Value)
            {
                Value = valueLast;
                if (Count > 0)
                {
                    valueLast = (valueEnd - Value) / Count + Value;
                    Count--;
                    return;
                }
            }
            valueLast = valueEnd;
            Count = 0;
        }

        /// <summary>
        /// Получить значение в кадре FPS
        /// </summary>
        /// <param name="interpolation">коэффициент времени от прошлого TPS клиента в диапазоне 0..1, где 0 это финиш, 1 начало</param>
        public bool UpdateFrame(float interpolation)
        {
            bool isAction = IsAction();
            ValueFrame = isAction ? Value + (valueLast - Value) * interpolation : Value;
            return isAction;
        }

        /// <summary>
        /// Активное ли мягкое смещение
        /// </summary>
        public bool IsAction() => Value != valueLast;
    }
}
