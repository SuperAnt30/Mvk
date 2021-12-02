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
        /// Массив шрифтов
        /// </summary>
        protected static Hashtable hashtable = new Hashtable();

        /// <summary>
        /// Горизонтальное смещение начала следующего глифа
        /// </summary>
        public static int HoriAdvance { get; protected set; } = 8;
        /// <summary>
        /// Вертикальное смещение начала следующего глифа 
        /// </summary>
        public static int VertAdvance { get; protected set; } = 8;

        /// <summary>
        /// Инициализировать шрифты
        /// </summary>
        /// <param name="textureFont">Текстура шрифтов</param>
        public static void Initialize(BufferedImage textureFont)
        {
            HoriAdvance = textureFont.Width >> 4;
            VertAdvance = textureFont.Height >> 4;

            hashtable = new Hashtable();
            char[] vc = Symbol.ToArrayKey();
            for (int i = 0; i < vc.Length; i++)
            {
                Symbol symbol = new Symbol(vc[i]);
                symbol.Initialize(textureFont);
                if (!hashtable.ContainsKey(vc[i])) hashtable.Add(symbol.Symb, symbol);
            }
        }

        /// <summary>
        /// Получить объект символа
        /// </summary>
        public static Symbol Get(char key) => hashtable.ContainsKey(key) ? hashtable[key] as Symbol : null;
    }
}
