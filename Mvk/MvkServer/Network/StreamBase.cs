using System;
using System.IO;
using System.Text;

namespace MvkServer.Network
{
    public class StreamBase: IDisposable
    {
        public Stream BaseStream { get; protected set; }

        public StreamBase(Stream stream) => BaseStream = stream;

        public void Dispose() => BaseStream.Dispose();

        protected int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);
        protected void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

        #region Read

        /// <summary>
        /// Прочесть логический тип (0..1) 1 байт
        /// </summary>
        public bool ReadBool() => ReadByte() != 0;

        /// <summary>
        /// Прочесть массив байт
        /// </summary>
        /// <param name="count">количество байт</param>
        public byte[] ReadBytes(int count)
        {
            byte[] b = new byte[count];
            for (int i = 0; i < count; i++)
            {
                b[i] = (byte)BaseStream.ReadByte();
            }
            return b;
        }
        /// <summary>
        /// Прочесть тип byte (0..255) 1 байт
        /// </summary>
        public byte ReadByte()
        {
            int value = BaseStream.ReadByte();
            if (value == -1) throw new EndOfStreamException();
            return (byte)value;
        }
        /// <summary>
        /// Прочесть тип ushort (0..65535) 2 байта
        /// </summary>
        public ushort ReadUShort() => (ushort)((ReadByte() << 8) | ReadByte());
        /// <summary>
        /// Прочесть тип uint (0..4 294 967 295) 4 байта
        /// </summary>
        public uint ReadUInt() => (uint)((ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        /// <summary>
        /// Прочесть тип uint (0..18 446 744 073 709 551 615) 8 байт
        /// </summary>
        public ulong ReadULong() => (ulong)((ReadByte() << 56) | (ReadByte() << 48) | (ReadByte() << 40) | (ReadByte() << 32)
            | (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte());
        /// <summary>
        /// Прочесть тип sbyte (-128..127) 1 байт
        /// </summary>
        public sbyte ReadSByte() => (sbyte)ReadByte();
        /// <summary>
        /// Прочесть тип short (-32768..32767) 2 байта
        /// </summary>
        public short ReadShort() => (short)ReadUShort();
        /// <summary>
        /// Прочесть тип int (-2 147 483 648..2 147 483 647) 4 байта
        /// </summary>
        public int ReadInt() => (int)ReadUInt();
        /// <summary>
        /// Прочесть тип int (–9 223 372 036 854 775 808..9 223 372 036 854 775 807) 8 байт
        /// </summary>
        public long ReadLong() => (long)ReadULong();

        /// <summary>
        /// Прочесть строку в UTF-16
        /// </summary>
        public string ReadString()
        {
            Encoding stringEncoding = Encoding.BigEndianUnicode;
            short length = ReadShort();
            if (length == 0) return string.Empty;
            // попробовать! было ранее другая методика ReadArray
            var data = ReadBytes(length * 2);// ReadArray(length * 2);
            return stringEncoding.GetString(data);
        }

        /// <summary>
        /// Прочесть тип float (точность 0,0001) 4 байта
        /// </summary>
        public float ReadFloat() => ReadInt() / 10000f;

        #endregion

        #region Write

        /// <summary>
        /// Записать логический тип (0..1) 1 байт
        /// </summary>
        public void WriteBool(bool value) => WriteByte(value ? (byte)1 : (byte)0);

        /// <summary>
        /// Записать массив байт
        /// </summary>
        public void WriteBytes(byte[] value) => Write(value, 0, value.Length);

        /// <summary>
        /// Записать тип byte (0..255) 1 байт
        /// </summary>
        public void WriteByte(byte value) => BaseStream.WriteByte(value);
        /// <summary>
        /// Записать тип ushort (0..65535) 2 байта
        /// </summary>
        public void WriteUShort(ushort value) => Write(new[]
        {
            (byte)((value & 0xFF00) >> 8),
            (byte)(value & 0xFF)
        }, 0, 2);
        /// <summary>
        /// Записать тип uint (0..4 294 967 295) 4 байта
        /// </summary>
        public void WriteUInt(uint value) => Write(new[]
        {
            (byte)((value & 0xFF000000) >> 24),
            (byte)((value & 0xFF0000) >> 16),
            (byte)((value & 0xFF00) >> 8),
            (byte)(value & 0xFF)
        }, 0, 4);
        /// <summary>
        /// Записать тип ulong (0..18 446 744 073 709 551 615) 8 байт
        /// </summary>
        public void WriteULong(ulong value) => Write(new[]
        {
            (byte)((value & 0xFF00000000000000) >> 56),
            (byte)((value & 0xFF000000000000) >> 48),
            (byte)((value & 0xFF0000000000) >> 40),
            (byte)((value & 0xFF00000000) >> 32),
            (byte)((value & 0xFF000000) >> 24),
            (byte)((value & 0xFF0000) >> 16),
            (byte)((value & 0xFF00) >> 8),
            (byte)(value & 0xFF)
        }, 0, 8);
        /// <summary>
        /// Записать тип sbyte (-128..127) 1 байт
        /// </summary>
        public void WriteSByte(sbyte value) => WriteByte((byte)value);
        /// <summary>
        /// Записать тип short (-32768..32767) 2 байта
        /// </summary>
        public void WriteShort(short value) => WriteUShort((ushort)value);
        /// <summary>
        /// Записать тип int (-2 147 483 648..2 147 483 647) 4 байта
        /// </summary>
        public void WriteInt(int value) => WriteUInt((uint)value);
        /// <summary>
        /// Записать тип long (–9 223 372 036 854 775 808..9 223 372 036 854 775 807) 8 байт
        /// </summary>
        public void WriteLong(long value) => WriteULong((ulong)value);

        /// <summary>
        /// Записать строку в UTF-16
        /// </summary>
        public void WriteString(string value)
        {
            Encoding stringEncoding = Encoding.BigEndianUnicode;
            WriteShort((short)value.Length);
            if (value.Length > 0) WriteBytes(stringEncoding.GetBytes(value));
        }

        /// <summary>
        /// Записать тип float (точность 0,0001) 4 байта
        /// </summary>
        public void WriteFloat(float value) => WriteInt((int)(value * 10000));

        #endregion

        /// <summary>
        /// Другая методика загрузки массива байт, альтернатива ReadBytes
        /// </summary>
        [Obsolete("Проверить, альтернатива ReadBytes")]
        protected byte[] ReadArray(int length)
        {
            var result = new byte[length];
            if (length == 0) return result;
            int n = length;
            while (true)
            {
                n -= Read(result, length - n, n);
                if (n == 0) break;
                System.Threading.Thread.Sleep(1);
            }
            return result;
        }
    }
}
