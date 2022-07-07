using System;
using System.Collections;

namespace MvkAssets
{
    /// <summary>
    /// Объект для перевода языка
    /// </summary>
    public class Language
    {
        /// <summary>
        /// Массив всех слов
        /// </summary>
        protected static Hashtable hashtable = new Hashtable();

        /// <summary>
        /// Выбрать язык
        /// </summary>
        /// <param name="keyLang"></param>
        public static void SetLanguage(AssetsLanguage keyLang)
        {
            string strAll = Assets.GetLanguage(keyLang);
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = strAll.Split(stringSeparators, StringSplitOptions.None);
            hashtable.Clear();
            foreach (string strLine in strs)
            {
                // комментарий
                if (strLine.Length == 0 || strLine.Substring(0, 1) == "#") continue;
                // Разделитель ключа и текста
                int index = strLine.IndexOf(":");
                if (index > 0)
                {
                    string key = strLine.Substring(0, index);
                    if (!hashtable.ContainsKey(key))
                    {
                        hashtable.Add(strLine.Substring(0, index), strLine.Substring(index + 1));
                    }
                }
            }
        }

        /// <summary>
        /// Слежующий номер
        /// </summary>
        public static int Next(int value)
        {
            int count = Enum.GetValues(typeof(AssetsLanguage)).Length - 1;
            value++;
            return value > count ? 0 : value;
        }

        /// <summary>
        /// Получить название по id
        /// </summary>
        public static string GetName(int id) => ((AssetsLanguage)id).ToString();

        /// <summary>
        /// Перевести фразу
        /// </summary>
        public static string T(string key) => hashtable.ContainsKey(key) ? hashtable[key].ToString() : key;
    }
}
