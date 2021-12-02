using MvkServer.Glm;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MvkClient.Util
{
    /// <summary>
    /// Объект изображения
    /// </summary>
    public class BufferedImage
    {
        /// <summary>
        /// Ширина
        /// </summary>
        public int Width { get; protected set; }
        /// <summary>
        /// Высотп
        /// </summary>
        public int Height { get; protected set; }
        /// <summary>
        /// Массив байт
        /// </summary>
        public byte[] Buffer { get; protected set; }

        public BufferedImage(string fileName) : this(new Bitmap(fileName)) { }

        public BufferedImage(Bitmap bitmap)
        {
            Width = bitmap.Width;
            Height = bitmap.Height;
            Buffer = BitmapToByteArray(bitmap);
        }

        /// <summary>
        /// Получить цвет пикселя
        /// </summary>
        public vec4 GetPixel(int x, int y)
        {
            int index = y * Height * 4 + x * 4;
            byte r = Buffer[index];
            byte g = Buffer[index + 1];
            byte b = Buffer[index + 2];
            byte a = Buffer[index + 3];
            return new vec4(Bf(r), Bf(g), Bf(b), Bf(a));
        }

        protected float Bf(byte c) => (float)c / 255f;

        /// <summary>
        /// Конвертация из Bitmap в объект BufferedImage
        /// </summary>
        protected static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            BitmapData bmpdata = null;
            try
            {
                bmpdata = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
                int numbytes = bmpdata.Stride * bitmap.Height;
                byte[] bytedata = new byte[numbytes];
                IntPtr ptr = bmpdata.Scan0;

                Marshal.Copy(ptr, bytedata, 0, numbytes);

                return bytedata;
            }
            finally
            {
                if (bmpdata != null)
                    bitmap.UnlockBits(bmpdata);
            }
        }
    }
}
