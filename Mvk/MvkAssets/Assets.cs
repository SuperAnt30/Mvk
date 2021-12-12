using System.Drawing;

namespace MvkAssets
{
    /// <summary>
    /// Объект ресурсов
    /// </summary>
    public class Assets
    {
        /// <summary>
        /// Получить картинку по ключу
        /// </summary>
        public static Bitmap GetBitmap(AssetsTexture key)
        {
            object obj = ResourceTexture.ResourceManager.GetObject(key.ToString(), ResourceTexture.Culture);
            if (obj != null && obj.GetType() == typeof(Bitmap)) return obj as Bitmap;
            return new Bitmap(16, 16);
        }

        /// <summary>
        /// Получить семпл 
        /// </summary>
        public static byte[] GetSample(AssetsSample key)
        {
            object obj = ResourceSound.ResourceManager.GetObject(key.ToString(), ResourceSound.Culture);
            if (obj.GetType() == typeof(byte[])) return obj as byte[];
            return new byte[0];
        }

        /// <summary>
        /// Получить текстуру по размеру шрифта
        /// </summary>
        public static AssetsTexture ConvertFontToTexture(FontSize size)
        {
            switch(size)
            {
                case FontSize.Font8: return AssetsTexture.Font8;
                case FontSize.Font16: return AssetsTexture.Font16;
                default: return AssetsTexture.Font12;
            }
        }

        /// <summary>
        /// Получить текстовый файл языков
        /// </summary>
        public static string GetLanguage(AssetsLanguage key)
        {
            object obj = ResourceLanguage.ResourceManager.GetObject(key.ToString(), ResourceLanguage.Culture);
            if (obj != null && obj.GetType() == typeof(string)) return obj.ToString();
            return "";
        }

        /// <summary>
        /// Растояние между буквами в пикселях
        /// </summary>
        public static int StepFont(FontSize size)
        {
            switch (size)
            {
                case FontSize.Font8: return 1;
                case FontSize.Font16: return 2;
                default: return 1;
            }
        }
    }
}
