using System;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект конвертирует массив в неуправляемую память
    /// </summary>
    public class BufferData
    {
        /// <summary>
        /// Размер
        /// </summary>
        public int Size { get; private set; }
        /// <summary>
        /// Указатель данных
        /// </summary>
        public IntPtr Data { get; private set; }
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty { get; private set; } = true;
        /// <summary>
        /// Количество полигонов
        /// </summary>
        //public int CountPoligon { get; private set; } = 0;



        /// <summary>
        /// Занести в память массив из float
        /// </summary>
        public void ConvertFloat(float[] data)
        {
            Size = data.Length * sizeof(float);
            Data = Marshal.AllocHGlobal(Size);
            Marshal.Copy(data, 0, Data, data.Length);
            IsEmpty = false;
        }

        /// <summary>
        /// Занести в память массив из float
        /// </summary>
        public void ConvertByte(byte[] data)
        {
            Size = data.Length * sizeof(byte);
            Data = Marshal.AllocHGlobal(Size);
            Marshal.Copy(data, 0, Data, data.Length);
            IsEmpty = false;
        }

        /// <summary>
        /// Освободить память
        /// </summary>
        public void Free()
        {
            if (!IsEmpty)
            {
                Marshal.FreeHGlobal(Data);
                IsEmpty = true;
            }
        }
    }
}
