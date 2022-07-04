using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MvkAssets
{
    public class Language : IEquatable<Language>
    {
        /// <summary>
        /// Название языка
        /// </summary>
        public string Name => Translate("lang.name");
        /// <summary>
        /// Название языка на этом языке
        /// </summary>
        public string TranslatedName => ID == 0 ? Name : Translate("lang.original.name");
        /// <summary>
        /// Код языка en-US, ru-RU и другие
        /// </summary>
        public string Code { get; }
        /// <summary>
        /// Номер языка
        /// <br/>
        /// 0 -> англиский
        /// </summary>
        public ushort ID { get; }
        private Dictionary<string, string> values = new Dictionary<string, string>(128);
        private Language(string key)
        {
            Code = key;
            ID = langCount++;
            languages.Add(this);
            LoadTranslate(this, Assets.GetLangFile(ID));
        }
        private Language(string key,string translates)
		{
            Code = key;
            ID = langCount++;
            languages.Add(this);
            LoadTranslate(this, translates);
        }
        /// <summary>
        /// Возвращает следующий язык
        /// </summary>
        public Language Next()
		{
            if (ID >= languages.Count - 1)
			{
                return languages[0];
			}
            return languages[ID + 1];
		}
        /// <summary>
        /// Возвращает перевод по ключу
        /// </summary>
        /// <param name="key">Ключ перевода</param>
        /// <param name="ifNotFound">Возвращаемое значание если значение по ключу не найдено, возвращает <paramref name="key"/> если <see langword="null"/></param>
        /// <returns>
        /// Вернёт ключ, если значение не найдено
        /// </returns>
        public string Translate(string key, string ifNotFound = null)
        {
            if (values.ContainsKey(key))
                return values[key];
            else return ifNotFound ?? key;
        }
        /// <summary>
        /// Существует ли перевод
        /// </summary>
        /// <param name="key">Ключ перевода</param>
        public bool ContainsTranslation(string key)
		{
            return values.ContainsKey(key);
		}
        public override bool Equals(object obj)
		{
            if (obj is Language lang) return Equals(lang);

            if (obj is string str)
			{
                if (Code == str)
				{
                    return true;
				}
			}

            return false;
		}
		public bool Equals(Language other)
		{
            return ID == other.ID;
		}

		public override int GetHashCode() => ID;

		private static ushort langCount = 0;
		private static ushort selectedLang = 0;
        public static readonly Language English, Russian;
        private static readonly List<Language> languages = new List<Language>();
        public static bool operator ==(Language left, Language right) => left?.Equals(right) ?? false;
		public static bool operator !=(Language left, Language right) => !(left == right);
        public static Language Get(ushort id)
            => id >= languages.Count ? languages[0] : languages[id];
        public static Language Current => languages[selectedLang];
        public static void Select(ushort id)
		{
            if (id >= languages.Count)
			{
                selectedLang = 0;
			}else
			{
                selectedLang = id;
			}
		}
        private static void LoadTranslate(Language lang, string translateFile)
		{
            string[] stringSeparators = new string[] { "\r\n" };
            string[] strs = translateFile.Split(stringSeparators, StringSplitOptions.None);
            lang.values.Clear();
            foreach (string strLine in strs)
            {
                // комментарий
                if (strLine.Length == 0 || strLine.Substring(0, 1) == "#") continue;
                // Разделитель ключа и текста
                int index = strLine.IndexOf(":");
                if (index > 0)
                {
                    string key = strLine.Substring(0, index);
                    if (!lang.values.ContainsKey(key))
                    {
                        lang.values.Add(strLine.Substring(0, index), strLine.Substring(index + 1));
                    }
                }
            }
        }
        public static Language RegistryLanguage(string code, string translateFile)
		{
            Language language = languages.Where(lang => lang.Code == code).FirstOrDefault();
            if (language is null)
			{
                return new Language(code, translateFile);
			}
            return language;
		}
        [Obsolete("Используйте Language.Current.Translate вместо T")]
        public static string T(string key)
            => Current.Translate(key);
        static Language()
		{
            English = new Language("en-US");
            Russian = new Language("ru-RU");
		}
	}
}
