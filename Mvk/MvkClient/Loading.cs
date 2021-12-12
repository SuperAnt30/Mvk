using MvkAssets;
using MvkClient.Setitings;
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
            Count = 1 // Загрузка опций
                + Enum.GetValues(typeof(AssetsSample)).Length + Enum.GetValues(typeof(AssetsTexture)).Length 
                - 4 // 3 текстуры загружаются до загрузчика (шрифты и логотип)
                + 1; // Финишный такт
        }

        /// <summary>
        /// Запуск загрузчика
        /// </summary>
        public void LoadStart()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                // Опции
                Setting.Load();
                Language.SetLanguage((AssetsLanguage)Setting.Language);
                OnTick(new ObjectEventArgs(ObjectKey.LoadStep));

                // Загрузка семплов
                foreach (AssetsSample key in Enum.GetValues(typeof(AssetsSample)))
                {
                    client.Sample.InitializeSample(key);
                    OnTick(new ObjectEventArgs(ObjectKey.LoadStep));
                }

                int i = 0;
                foreach (AssetsTexture key in Enum.GetValues(typeof(AssetsTexture)))
                {
                    i++;
                    if (i < 4) continue;
                    OnTick(new ObjectEventArgs(ObjectKey.LoadStepTexture, new BufferedImage(key, Assets.GetBitmap(key))));
                }
                //System.Threading.Thread.Sleep(2000); // Тест пауза чтоб увидеть загрузчик
                OnTick(new ObjectEventArgs(ObjectKey.LoadingStopMain));
            });
        }

        /// <summary>
        /// Событие такта
        /// </summary>
        public event ObjectEventHandler Tick;
        protected virtual void OnTick(ObjectEventArgs e) => Tick?.Invoke(this, e);
    }
}
