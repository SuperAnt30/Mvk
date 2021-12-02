using MvkClient.Util;
using MvkServer.Glm;

namespace MvkClient.Renderer.Font
{
    /// <summary>
    /// Объект символа
    /// </summary>
    public class Symbol
    {
        protected static string Key = " !\"#$%&\'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯабвгдежзиклмнопрстуфхцчшщъыьэюяЁёЙй";
        /// <summary>
        /// Получить массив символов
        /// </summary>
        public static char[] ToArrayKey() => Key.ToCharArray();


        public Symbol(char c) => Symb = c;

        public void Initialize(BufferedImage bi)
        {
            int index = Key.IndexOf(Symb) + 32;
            if (index == -1) return;

            U1 = (index & 15) * 0.0625f;
            U2 = U1 + 0.0625f;
            V1 = (index >> 4) * 0.0625f;
            V2 = V1 + 0.0625f;

            GetWidth(bi, index);
        }

        /// <summary>
        /// Получить ширину символа
        /// </summary>
        protected void GetWidth(BufferedImage bi, int index)
        {
            int advance = FontAdvance.HoriAdvance;

            int x0 = (index & 15) * advance;
            int y0 = (index >> 4) * advance;
            int x1 = x0 + advance - 1;
            int y1 = y0 + advance;

            for (int x = x1; x >= x0; x--)
            {
                for (int y = y0; y < y1; y++)
                {
                    vec4 col = bi.GetPixel(x, y);
                    if (col.w > 0)
                    {
                        Width = x - x0;
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Символ
        /// </summary>
        public char Symb { get; protected set; }
        /// <summary>
        /// Ширина символа
        /// </summary>
        public int Width { get; protected set; } = 4;

        /// <summary>
        /// x
        /// </summary>
        public float U1 { get; protected set; } = 0f;
        /// <summary>
        /// x
        /// </summary>
        public float U2 { get; protected set; } = 0f;
        /// <summary>
        /// y
        /// </summary>
        public float V1 { get; protected set; } = 0f;
        /// <summary>
        /// y
        /// </summary>
        public float V2 { get; protected set; } = 0f;
    }
}
