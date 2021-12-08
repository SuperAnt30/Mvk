using MvkClient.Util;

namespace MvkClient
{
    /// <summary>
    /// Объект подготовки загрузки звуковых файлов в буфер, текстур и прочего
    /// </summary>
    public class TestLoadingWorld
    {
        /// <summary>
        /// Количество процессинга
        /// </summary>
        public int Count { get; protected set; } = 128;
        /// <summary>
        /// Основной объект клиента
        /// </summary>
        private Client client;

        public TestLoadingWorld(Client client) => this.client = client;

        /// <summary>
        /// Запуск загрузчика
        /// </summary>
        public void LoadStart()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                int speed = 5;
                for (int i = 0; i < Count; i++)
                {
                    OnTick(new ObjectEventArgs(ObjectKey.LoadStep));
                    System.Threading.Thread.Sleep(speed);
                }
                System.Threading.Thread.Sleep(50); // Тест пауза чтоб увидеть загрузчик
                OnTick(new ObjectEventArgs(ObjectKey.LoadingStopWorld));
            });
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event ObjectEventHandler Tick;
        protected virtual void OnTick(ObjectEventArgs e) => Tick?.Invoke(this, e);
    }
}
