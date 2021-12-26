using MvkServer.Util;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Сторона блока
    /// </summary>
    public struct Face
    {
        private Pole side;
        private int numberTexture;
        private bool isColor;

        public Face(int numberTexture) : this(Pole.All, numberTexture, false) { }
        public Face(Pole pole, int numberTexture) : this(pole, numberTexture, false) { }
        public Face(Pole pole, int numberTexture, bool isColor)
        {
            side = pole;
            this.numberTexture = numberTexture;
            this.isColor = isColor;
        }

        /// <summary>
        /// С какой стороны
        /// </summary>
        public Pole GetSide() => side;
        /// <summary>
        /// Номер текстуры в карте
        /// </summary>
        public int GetNumberTexture() => numberTexture;
        /// <summary>
        /// Применение цвета
        /// </summary>
        public bool GetIsColor() => isColor;
    }
}
