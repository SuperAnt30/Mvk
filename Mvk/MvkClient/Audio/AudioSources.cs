using System.Collections.Generic;

namespace MvkClient.Audio
{
    /// <summary>
    /// Объект источкиков звука
    /// </summary>
    public class AudioSources
    {
        protected AudioSource[] sources;
        /// <summary>
        /// Общее количество источников
        /// </summary>
        public int CountAll { get; protected set; } = 0;
        /// <summary>
        /// Количество источников воспроизводившие звуки
        /// </summary>
        public int CountProcessing { get; protected set; } = 0;

        /// <summary>
        /// Инициализировать и определеить количество источников
        /// </summary>
        public void Initialize()
        {
            List<AudioSource> list = new List<AudioSource>();
            bool error = false;
            while (!error)
            {
                AudioSource audio = new AudioSource();
                if (audio.IsError)
                {
                    error = audio.IsError;
                }
                else
                {
                    list.Add(audio);
                }
            }
            sources = list.ToArray();
            CountAll = list.Count;
        }

        /// <summary>
        /// Получить свободный источник
        /// </summary>
        /// <returns></returns>
        public AudioSource GetAudio()
        {
            foreach (AudioSource audio in sources)
            {
                if (!audio.Processing)
                {
                    return audio;
                }
            }
            return null;
        }

        /// <summary>
        /// Тактовая проверка источников
        /// </summary>
        public void AudioTick()
        {
            int count = 0;
            foreach (AudioSource audio in sources)
            {
                if (audio.CheckProcessing())
                {
                    count++;
                }
            }
            CountProcessing = count;
        }
    }
}
