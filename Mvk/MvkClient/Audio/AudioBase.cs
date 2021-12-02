using MvkServer.Glm;
using System;
using System.IO;

namespace MvkClient.Audio
{
    /// <summary>
    /// Базовый класс звуков
    /// </summary>
    public class AudioBase
    {
        /// <summary>
        /// Карта всех семплов
        /// </summary>
        protected AudioMap map = new AudioMap();
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

            // Загрузка всех сэмплов
            LoadSamples();
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
        public void PlaySound(string key, vec3 pos, float volume, float pitch)
        {
            if (map.Contains(key))
            {
                AudioSample sample = map.Get(key);
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
        public void PlaySound(string key)
        {
            PlaySound(key, new vec3(0), 1f, 1f);
        }

        /// <summary>
        /// Загрузка всех сэмплов
        /// </summary>
        protected void LoadSamples()
        {
            LoadDirectorie("sounds");
        }

        /// <summary>
        /// Загрузить директорию
        /// </summary>
        protected void LoadDirectorie(string path)
        {
            if (Directory.Exists(path))
            {
                string[] ar = Directory.GetFiles(path);
                foreach (string pathF in ar)
                {
                    LoadFile(pathF);
                }
                ar = Directory.GetDirectories(path);
                foreach (string pf in ar)
                {
                    LoadDirectorie(pf);
                }
            }
        }

        /// <summary>
        /// Загрузить файл, если правельный то внести в карту звуков
        /// </summary>
        protected void LoadFile(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".ogg")
            {
                string fileName = Path.GetFileNameWithoutExtension(path);
                string dir = Path.GetDirectoryName(path).Replace("\\", ".");
                string s = (dir + "." + fileName).Substring(7);

                AudioSample sample = new AudioSample();
                sample.LoadOgg(path);
                map.Set(s, sample);
            }
        }
    }
}
