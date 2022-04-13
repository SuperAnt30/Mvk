using System;
using System.Collections.Generic;

namespace MvkClient.Util
{
    public class ByteBuffer
    {
        private List<byte> buffer = new List<byte>();

        public void PutByte(byte value) => buffer.Add(value);

        public void PutFloat(float value) => buffer.AddRange(BitConverter.GetBytes(value));

        public void ArrayByte(byte[] vs) => buffer.AddRange(vs);

        public void ArrayFloat(float[] vs)
        {
            // создайте массив байтов и скопируйте в него floats...
            var byteArray = new byte[vs.Length * 4];
            Buffer.BlockCopy(vs, 0, byteArray, 0, byteArray.Length);
            buffer.AddRange(byteArray);
        }

        public byte[] ToArray() => buffer.ToArray();

        public void Clear() => buffer.Clear();


        public static float GetFloat(byte[] vs, int startIndex)
        {
            return BitConverter.ToSingle(vs, startIndex);
        }
    }
}
