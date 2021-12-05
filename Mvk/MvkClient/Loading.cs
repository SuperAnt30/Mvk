using MvkAssets;
using MvkClient.Util;
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
        public int Count { get; protected set; } = 70;
        /// <summary>
        /// Основной объект клиента
        /// </summary>
        private Client client;

        public Loading(Client client)
        {
            this.client = client;
            
            // Определяем максимальное количество для счётчика
            Count = Enum.GetValues(typeof(AssetsSample)).Length + Enum.GetValues(typeof(AssetsTexture)).Length 
                - 2 // 2 текстуры загружаются до загрузчика (шрифт и логотип)
                + 1; // Финишный такт
        }

        /// <summary>
        /// Запуск загрузчика
        /// </summary>
        public void LoadStart()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                int speed = 400;
                // Загрузка семплов
                foreach (AssetsSample key in Enum.GetValues(typeof(AssetsSample)))
                {
                    client.Sample.InitializeSample(key);
                    OnTick(new ObjectEventArgs(ObjectKey.LoadTick));
                    System.Threading.Thread.Sleep(speed);
                }

                int i = 0;
                foreach (AssetsTexture key in Enum.GetValues(typeof(AssetsTexture)))
                {
                    i++;
                    if (i < 3) continue;
                    OnTick(new ObjectEventArgs(ObjectKey.LoadTickTexture, new BufferedImage(key, Assets.GetBitmap(key))));
                    System.Threading.Thread.Sleep(speed);
                }
                //System.Threading.Thread.Sleep(500); // Тест пауза чтоб увидеть загрузчик
                OnTick(new ObjectEventArgs(ObjectKey.LoadingStop));
            });
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event ObjectEventHandler Tick;
        protected virtual void OnTick(ObjectEventArgs e) => Tick?.Invoke(this, e);
    }
}
