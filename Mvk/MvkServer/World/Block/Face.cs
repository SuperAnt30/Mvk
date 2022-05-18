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

        private byte animationFrame;
        private byte animationPause;

        public Face(int numberTexture) : this(Pole.All, numberTexture) { }
        public Face(Pole pole, int numberTexture) : this(pole, numberTexture, false, new vec3(1)) { }
        public Face(Pole pole, int numberTexture, vec3 color) : this(pole, numberTexture, false, color) { }
        public Face(Pole pole, int numberTexture, bool isColor, vec3 color)
        {
            side = pole;
            this.numberTexture = numberTexture;
            this.isColor = isColor;
            this.color = color;
            animationFrame = 0;
            animationPause = 0;
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

        /// <summary>
        /// Для анимации блока, указывается количество кадров в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 - нет анимации
        /// </summary>
        public byte AnimationFrame() => animationFrame;
        /// <summary>
        /// Для анимации блока, указывается пауза между кадрами в игровом времени (50 мс),
        /// можно кратно 2 в степени (2, 4, 8, 16, 32, 64...)
        /// 0 или 1 - нет задержки, каждый такт игры смена кадра
        /// </summary>
        public byte AnimationPause() => animationPause;

        /// <summary>
        /// Задать анимированную текстуру
        /// </summary>
        /// <param name="frame">количество кадров в игровом времени</param>
        /// <param name="pause">пауза между кадрами в игровом времени</param>
        public Face SetAnimation(byte frame, byte pause)
        {
            animationFrame = frame;
            animationPause = pause;
            return this;
        }
    }
}
