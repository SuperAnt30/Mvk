using MvkAssets;
using MvkServer.Glm;
using System;
using System.Collections;

namespace MvkClient.Audio
{
    /// <summary>
    /// Базовый класс звуков
    /// </summary>
    public class AudioBase
    {
        /// <summary>
        /// Массив всех семплов
        /// </summary>
        protected Hashtable items = new Hashtable();
        /// <summary>
        /// Объект источников звука
        /// </summary>
        protected AudioSources sources = new AudioSources();
        /// <summary>
        /// Строка для дэбага сколько источников и занятых
        /// </summary>
        public string StrDebug { get; protected set; }

        public void Initialize()
        {
            // Инициализация звука
            IntPtr pDevice = Al.alcOpenDevice(null);
            IntPtr pContext = Al.alcCreateContext(pDevice, null);
            Al.alcMakeContextCurrent(pContext);

            // Инициализация источников звука
            sources.Initialize();
        }

        /// <summary>
        /// Загрузка сэмпла
        /// </summary>
        public void InitializeSample(AssetsSample key)
        {
            byte[] vs = Assets.GetSample(key);
            AudioSample sample = new AudioSample();
            sample.LoadOgg(vs);
            Set(key, sample);
        }

        /// <summary>
        /// Такт
        /// </summary>
        public void Tick()
        {
            sources.AudioTick();
            StrDebug = string.Format("{0}/{1}", sources.CountProcessing, sources.CountAll);
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(AssetsSample key, vec3 pos, float volume, float pitch)
        {
            if (items.Contains(key))
            {
                AudioSample sample = Get(key);
                if (sample != null && sample.Size > 0)
                {
                    AudioSource source = sources.GetAudio();
                    if (source != null)
                    {
                        source.Sample(sample);
                        source.Play(pos, volume, pitch);
                    }
                }
            }
        }
        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void PlaySound(AssetsSample key)
        {
            PlaySound(key, new vec3(0), 1f, 1f);
        }

        /// <summary>
        /// Добавить или изменить сэмпл
        /// </summary>
        protected void Set(AssetsSample key, AudioSample sample)
        {
            if (items.ContainsKey(key))
            {
                items[key] = sample;
            }
            else
            {
                items.Add(key, sample);
            }
        }

        /// <summary>
        /// Получить сэмпл по ключу
        /// </summary>
        protected AudioSample Get(AssetsSample key) => items.ContainsKey(key) ? items[key] as AudioSample : null;
    }
}
