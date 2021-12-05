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
    }
}
