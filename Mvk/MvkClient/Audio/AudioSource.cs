using MvkServer.Glm;

namespace MvkClient.Audio
{
    /// <summary>
    /// Объект источника звука
    /// </summary>
    public class AudioSource
    {
        /// <summary>
        /// Тон звука
        /// </summary>
        public float Pitch { get; protected set; } = 1.0f;
        /// <summary>
        /// Усиление звука
        /// </summary>
        public float Volume { get; protected set; } = 1.0f;
        /// <summary>
        /// Позиция где будет звук
        /// </summary>
        public vec3 Position { get; protected set; } = new vec3(0, 0, 0);

        /// <summary>
        /// Id источника
        /// </summary>
        protected uint sourceId = 0;
        /// <summary>
        /// Id буфера
        /// </summary>
        protected uint bufferId = 0;

        /// <summary>
        /// Есть ли ошибка
        /// </summary>
        public bool IsError { get; protected set; } = false;
        /// <summary>
        /// Проигрывает ли сейчас звук
        /// </summary>
        public bool Processing { get; protected set; } = false;

        /// <summary>
        /// Создать объект звукового источника
        /// </summary>
        public AudioSource()
        {
            Al.alGenSources(1, out uint sid);
            int errorCode = Al.alGetError();
            // По ошибке определяем источник и буфер обмена
            if (errorCode == 0)
            {
                Al.alGenBuffers(1, out uint bid);
                errorCode = Al.alGetError();
                if (errorCode == 0)
                {
                    // Всё норм
                    bufferId = bid;
                    sourceId = sid;
                    return;
                }
            }
            IsError = true;
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void Play()
        {
            if (IsError) return;
            Processing = true;
            Al.alSourcef(sourceId, Al.AL_PITCH, Pitch);
            Al.alSourcef(sourceId, Al.AL_GAIN, Volume);
            Al.alSource3f(sourceId, Al.AL_POSITION, Position.x, Position.y, Position.z);
            Al.alSourcePlay(sourceId);
        }

        /// <summary>
        /// Проиграть звук
        /// </summary>
        public void Play(vec3 pos, float volume, float pitch)
        {
            Position = pos;
            Volume = volume;
            Pitch = pitch;
            Play();
        }

        /// <summary>
        /// Проверить процесс звучания
        /// </summary>
        public bool CheckProcessing()
        {
            if (Processing)
            {
                Al.alGetSourcei(sourceId, Al.AL_SOURCE_STATE, out int value);
                if (value != Al.AL_PLAYING)
                {
                    Al.alDeleteSources(1, ref sourceId);
                    Al.alDeleteBuffers(1, ref bufferId);
                    Al.alGenSources(1, out uint sid);
                    int errorS = Al.alGetError();
                    Al.alGenBuffers(1, out uint bid);
                    int errorB = Al.alGetError();
                    // Всё норм
                    bufferId = bid;
                    sourceId = sid;
                    Processing = false;
                }
            }
            return Processing;
        }

        /// <summary>
        /// Задать сэмпл
        /// </summary>
        /// <param name="audio">Объект сэмпла</param>
        public void Sample(AudioSample audio)
        {
            Al.alBufferData(bufferId, audio.AlFormat, audio.Buffer, audio.Size, audio.SamplesPerSecond);
            Al.alSourcei(sourceId, Al.AL_BUFFER, (int)bufferId);
        }
    }
}
