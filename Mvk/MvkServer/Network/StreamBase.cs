using System.IO;
using System.Text;

namespace MvkServer.Network
{
    public class StreamBase : Stream
    {
        public Stream BaseStream { get; protected set; }

        public StreamBase(Stream stream) => BaseStream = stream;

        public override long Position
        {
            get { return BaseStream.Position; }
            set { BaseStream.Position = value; }
        }

        public override bool CanRead => BaseStream.CanRead;
        public override bool CanSeek => BaseStream.CanSeek;
        public override bool CanWrite => BaseStream.CanWrite;
        public override long Length => BaseStream.Length;
        public override void Flush() => BaseStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);
        public override void SetLength(long value) => BaseStream.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => BaseStream.Read(buffer, offset, count);
        public override void Write(byte[] buffer, int offset, int count) => BaseStream.Write(buffer, offset, count);

        public byte[] ReadBytes(int count)
        {
            byte[] b = new byte[count];
            for (int i = 0; i < count; i++)
            {
                b[i] = (byte)BaseStream.ReadByte();
            }
            return b;
        }
        public byte ReadInt8()
        {
            int value = BaseStream.ReadByte();
            if (value == -1)
                throw new EndOfStreamException();
            return (byte)value;
        }

        public void WriteInt8(byte value)
        {
            WriteByte(value);
        }

        public int ReadInt32()
        {
            return (int)ReadUInt32();
        }

        public void WriteInt32(int value)
        {
            WriteUInt32((uint)value);
        }

        public byte ReadUInt8()
        {
            int value = BaseStream.ReadByte();
            if (value == -1)
                throw new EndOfStreamException();
            return (byte)value;
        }

        public ushort ReadUInt16()
        {
            return (ushort)(
                (ReadUInt8() << 8) |
                ReadUInt8());
        }

        public void WriteUInt16(ushort value)
        {
            Write(new[]
                {
                    (byte)((value & 0xFF00) >> 8),
                    (byte)(value & 0xFF)
                }, 0, 2);
        }

        public short ReadInt16()
        {
            return (short)ReadUInt16();
        }

        public void WriteInt16(short value)
        {
            WriteUInt16((ushort)value);
        }

        public uint ReadUInt32()
        {
            return (uint)(
                (ReadUInt8() << 24) |
                (ReadUInt8() << 16) |
                (ReadUInt8() << 8) |
                ReadUInt8());
        }

        public void WriteUInt32(uint value)
        {
            Write(new[]
                {
                    (byte)((value & 0xFF000000) >> 24),
                    (byte)((value & 0xFF0000) >> 16),
                    (byte)((value & 0xFF00) >> 8),
                    (byte)(value & 0xFF)
                }, 0, 4);
        }

        public byte[] ReadUInt8Array(int length)
        {
            var result = new byte[length];
            if (length == 0) return result;
            int n = length;
            while (true)
            {
                n -= Read(result, length - n, n);
                if (n == 0)
                    break;
                System.Threading.Thread.Sleep(1);
            }
            return result;
        }

        public void WriteUInt8Array(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        public string ReadString()
        {
            Encoding stringEncoding = Encoding.BigEndianUnicode;
            short length = ReadInt16();
            if (length == 0) return string.Empty;
            var data = ReadUInt8Array(length * 2);
            return stringEncoding.GetString(data);
        }

        public void WriteString(string value)
        {
            Encoding stringEncoding = Encoding.BigEndianUnicode;
            WriteInt16((short)value.Length);
            if (value.Length > 0)
                WriteUInt8Array(stringEncoding.GetBytes(value));
        }
    }
}
