using MvkAssets;

namespace MvkClient.Renderer
{
    /// <summary>
    /// Структура для информации о текстуре
    /// </summary>
    public struct TextureStruct
    {
        private readonly uint key;
        private readonly int width;
        private readonly int height;
        private readonly AssetsTexture assets;
        private readonly bool notEmpty;

        public TextureStruct(uint key, int width, int height, AssetsTexture assets)
        {
            this.key = key;
            this.width = width;
            this.height = height;
            this.assets = assets;
            notEmpty = true;
        }

        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !notEmpty;
        /// <summary>
        /// Ключ текстуры для OpenGL
        /// </summary>
        public uint GetKey() => key;
        /// <summary>
        /// Ширина
        /// </summary>
        public int GetWidth() => width;
        /// <summary>
        /// Высота
        /// </summary>
        public int GetHeight() => height;
        /// <summary>
        /// Получить тип текстуры
        /// </summary>
        public AssetsTexture GetAssets() => assets;

        public override string ToString() => string.Format("{0} {3} ({1}; {2}) ", key, width, height, assets);
    }
}
