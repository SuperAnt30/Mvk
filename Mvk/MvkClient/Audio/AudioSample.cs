using NVorbis;
using System.Collections.Generic;
using System.IO;

namespace MvkClient.Audio
{
    /// <summary>
    /// Объект аудио сэмпла
    /// </summary>
    public class AudioSample
    {
        /// <summary>
        /// Количество каналов
        /// </summary>
        public int Channels { get; protected set; } = 1;
        /// <summary>
        /// Частота
        /// </summary>
        public int SamplesPerSecond { get; protected set; } = 44100;
        /// <summary>
        /// Буфер данных
        /// </summary>
        public byte[] Buffer { get; protected set; } = new byte[0];
        /// <summary>
        /// Размер буфера
        /// </summary>
        public int Size { get; protected set; } = 0;

        /// <summary>
        /// Формат для буффера
        /// </summary>
        public int AlFormat
        {
            get { return Channels == 1 ? Al.AL_FORMAT_MONO16 : Al.AL_FORMAT_STEREO16; }
        }

        /// <summary>
        /// Загрузить Wav сэмпл
        /// </summary>
        public void LoadWave(string path)
        {
            using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                char[] t = reader.ReadChars(4); // RIFF
                reader.ReadChars(18);
                Channels = reader.ReadInt16();
                SamplesPerSecond = reader.ReadInt32();
                reader.ReadChars(6);
                int bps = reader.ReadInt16();
                reader.ReadChars(4);
                Size = reader.ReadInt32();
                Buffer = reader.ReadBytes(Size);
            }
        }

        /// <summary>
        /// Загрузить Ogg сэмпл
        /// </summary>
        public void LoadOgg(string path)
        {
            VorbisReader vorbis = new VorbisReader(path);
            float[] buffer = new float[1024];
            List<byte> result = new List<byte>();
            int count;
            while ((count = vorbis.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    short temp = (short)(32767f * buffer[i]);
                    if (temp > 32767)
                    {
                        result.Add(0xFF);
                        result.Add(0x7F);
                    }
                    else if (temp < -32768)
                    {
                        result.Add(0x80);
                        result.Add(0x00);
                    }
                    result.Add((byte)temp);
                    result.Add((byte)(temp >> 8));
                }
            }
            Channels = vorbis.Channels;
            SamplesPerSecond = vorbis.SampleRate;
            Buffer = result.ToArray();
            Size = Buffer.Length;
        }

        /// <summary>
        /// Очистить буфер
        /// </summary>
        public void Clear()
        {
            Buffer = new byte[0];
            Size = 0;
        }
    }
}
