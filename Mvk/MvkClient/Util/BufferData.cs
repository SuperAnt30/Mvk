using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект конвертирует массив в неуправляемую память
    /// </summary>
    public struct BufferData
    {
        /// <summary>
        /// Размер
        /// </summary>
        public int size;
        /// <summary>
        /// Указатель данных
        /// </summary>
        public IntPtr data;
        /// <summary>
        /// имеется ли тело
        /// </summary>
        public bool body;
        /// <summary>
        /// Количество полигонов
        /// </summary>
        //public int CountPoligon { get; private set; } = 0;



        /// <summary>
        /// Занести в память массив из float
        /// </summary>
        //public void ConvertFloat(float[] data)
        //{
        //    Size = data.Length * sizeof(float);
        //    Data = Marshal.AllocHGlobal(Size);
        //    Marshal.Copy(data, 0, Data, data.Length);
        //    IsEmpty = false;
        //}

        /// <summary>
        /// Занести в память массив из float
        /// </summary>
        public void ConvertByte(List<byte> data)
        {
            if (data.Count == 0)
            {
                Free();
            }
            else
            {
                //GCHandle handle = GCHandle.Alloc(data);
                //this.data = (IntPtr)handle;
                //handle.Free();
                size = data.Count * sizeof(byte);
                this.data = Marshal.AllocHGlobal(size);
                Marshal.Copy(data.ToArray(), 0, this.data, data.Count);
                body = true;
            }
        }

        /// <summary>
        /// Освободить память
        /// </summary>
        public void Free()
        {
            if (body)
            {
                Marshal.FreeHGlobal(data);
                body = false;
            }
        }
    }
}
