using System;

namespace MvkClient
{
    /// <summary>
    /// Объект подготовки загрузки звуковых файлов в буфер, текстур и прочего
    /// </summary>
    public class Loading
    {
        /// <summary>
        /// Количество процессинга
        /// </summary>
        public int Count { get; protected set; } = 270;

        /// <summary>
        /// Запуск загрузчика
        /// </summary>
        public void LoadStart()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < Count; i++)
                {
                    System.Threading.Thread.Sleep(10);
                    OnTick();
                }
            });
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event EventHandler Tick;
        protected virtual void OnTick() => Tick?.Invoke(this, new EventArgs());
    }
}
