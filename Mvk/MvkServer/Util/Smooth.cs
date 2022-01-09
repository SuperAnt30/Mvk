namespace MvkServer.Util
{
    /// <summary>
    /// Объект плавного перемещения 0..1
    /// </summary>
    public class Smooth
    {
        /// <summary>
        /// Значение 0..1
        /// </summary>
        public float Value { get; protected set; } = 0f;
        /// <summary>
        /// Полный цикл, пока не отработает плавный старт, только потом плавный конец
        /// </summary>
        public bool IsFullCycle { get; set; } = false;// true;

        /// <summary>
        /// Шаг в начале, за 1/20 секунду
        /// </summary>
        protected float stepBegin = 0.1f;
        /// <summary>
        /// Шаг в конце, за 1/20 секунду
        /// </summary>
        protected float stepEnd = 0.1f;
        /// <summary>
        /// Запущено действие начала
        /// </summary>
        protected bool begin = false;
        /// <summary>
        /// Запущено действие конец
        /// </summary>
        protected bool end = false;
        /// <summary>
        /// Есть ли действие
        /// </summary>
        protected bool action = false;

        public Smooth() { }
        public Smooth(float step)
        {
            stepBegin = step;
            stepEnd = step;
        }
        public Smooth(float stepBegin, float stepEnd)
        {
            this.stepBegin = stepBegin;
            this.stepEnd = stepEnd;
        }

        /// <summary>
        /// Действие начало
        /// </summary>
        public bool GetBegin() => begin;
        /// <summary>
        /// Действие завершения
        /// </summary>
        public bool GetEnd() => end;

        /// <summary>
        /// Запуск 
        /// </summary>
        public void Begin()
        {
            if (!action)
            {
                Value = 0;
                action = true;
            }
            begin = true;
            end = false;
        }

        /// <summary>
        /// Остановка
        /// </summary>
        public void End()
        {
            if (action)
            {
                if (!IsFullCycle) begin = false;
                end = true;
            }
        }

        /// <summary>
        /// Обновление в один такт
        /// </summary>
        public void Update()
        {
            if (action)
            {
                if (begin)
                {
                    Value += stepBegin;
                    if (Value > 1f)
                    {
                        Value = 1f;
                        begin = false;
                    }
                }
                else if (end)
                {
                    Value -= stepEnd;
                    if (Value < 0)
                    {
                        Value = 0;
                        end = false;
                        action = false;
                    }
                }
            }
        }
    }
}
