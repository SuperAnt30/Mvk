using MvkServer.Util;

namespace MvkClient.Audio
{
    /// <summary>
    /// Карта сэмплов
    /// </summary>
    public class AudioMap : Map
    {
        /// <summary>
        /// Добавить или изменить сэмпл
        /// </summary>
        public void Set(string key, AudioSample sample)
        {
            base.Set(key, sample);
        }

        /// <summary>
        /// Получить значение по ключу
        /// </summary>
        public new AudioSample Get(string key)
        {
            return base.Get(key) as AudioSample;
        }
    }
}
