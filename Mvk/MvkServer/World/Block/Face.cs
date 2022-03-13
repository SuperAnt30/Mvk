using MvkServer.Glm;
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
        private vec3 color;

        public Face(int numberTexture) : this(Pole.All, numberTexture) { }
        public Face(Pole pole, int numberTexture) : this(pole, numberTexture, false, new vec3(1)) { }
        public Face(Pole pole, int numberTexture, vec3 color) : this(pole, numberTexture, false, color) { }
        public Face(Pole pole, int numberTexture, bool isColor, vec3 color)
        {
            side = pole;
            this.numberTexture = numberTexture;
            this.isColor = isColor;
            this.color = color;
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
        /// <summary>
        /// Получить цвет
        /// </summary>
        public vec3 GetColor() => color;
    }
}
