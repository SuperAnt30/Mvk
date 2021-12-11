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

        /// <summary>
        /// Размер шрифта
        /// </summary>
        public int Size { get; protected set; }
        /// <summary>
        /// Символ
        /// </summary>
        public char Symb { get; protected set; }
        /// <summary>
        /// Ширина символа
        /// </summary>
        public int Width { get; protected set; } = 4;

        /// <summary>
        /// Индекс листа символа
        /// </summary>
        protected uint dList;

        public Symbol(char c, int size)
        {
            Symb = c;
            Size = size;
        }

        public void Initialize(BufferedImage bi)
        {
            int index = Key.IndexOf(Symb) + 32;
            if (index == -1) return;

            float u1 = (index & 15) * 0.0625f;
            float u2 = u1 + 0.0625f;
            float v1 = (index >> 4) * 0.0625f;
            float v2 = v1 + 0.0625f;

            GetWidth(bi, index);
            dList = GLRender.ListBegin();
            GLRender.Rectangle(0, 0, FontAdvance.HoriAdvance[Size], FontAdvance.VertAdvance[Size], u1, v1, u2, v2);
            GLRender.ListEnd();
        }

        /// <summary>
        /// Прорисовка символа
        /// </summary>
        public void Draw() => GLRender.ListCall(dList);

        /// <summary>
        /// Получить ширину символа
        /// </summary>
        protected void GetWidth(BufferedImage bi, int index)
        {
            int advance = FontAdvance.HoriAdvance[Size];

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
                        Width = x - x0 + 1;
                        return;
                    }
                }
            }
        }
    }
}
