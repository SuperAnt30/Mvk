using MvkClient.Util;
using System.Collections;

namespace MvkClient.Renderer.Font
{
    /// <summary>
    /// Объект хранит все глифы
    /// </summary>
    public class FontAdvance
    {
        /// <summary>
        /// Массив символов
        /// </summary>
        protected static Hashtable[] hashtable = new Hashtable[3];

        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        public static int[] HoriAdvance { get; protected set; } = new int[] { 8, 12, 16 };
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        public static int[] VertAdvance { get; protected set; } = new int[] { 8, 12, 16 };

        /// <summary>
        /// Инициализировать шрифты
        /// </summary>
        public static void Initialize(BufferedImage textureFont8, BufferedImage textureFont12, BufferedImage textureFont16)
        {
            InitializeFontX(textureFont8, 0);
            InitializeFontX(textureFont12, 1);
            InitializeFontX(textureFont16, 2);
        }

        protected static void InitializeFontX(BufferedImage textureFont, int size)
        {
            HoriAdvance[size] = textureFont.Width >> 4;
            VertAdvance[size] = textureFont.Height >> 4;

            hashtable[size] = new Hashtable();
            char[] vc = Symbol.ToArrayKey();
            for (int i = 0; i < vc.Length; i++)
            {
                Symbol symbol = new Symbol(vc[i], size);
                symbol.Initialize(textureFont);
                if (!hashtable[size].ContainsKey(vc[i])) hashtable[size].Add(symbol.Symb, symbol);
            }
        }

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public static Symbol Get(char key, int size) => hashtable[size].ContainsKey(key) ? hashtable[size][key] as Symbol : null;
    }
}
