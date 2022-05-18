using MvkServer.Glm;

namespace MvkServer.World.Chunk.Light
{
    /// <summary>
    /// Структура для расчётов освещения
    /// </summary>
    public struct LightStruct
    {
        /// <summary>
        /// Глобальная позиция
        /// </summary>
        public vec3i Pos;
        /// <summary>
        /// Вектор от центра
        /// </summary>
        public vec3i Vec;
        /// <summary>
        /// Освещение
        /// </summary>
        public byte Light;
        /// <summary>
        /// Небесное освещение
        /// </summary>
        public bool Sky;

        private readonly bool isNotEmpty;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !isNotEmpty;


        public LightStruct(vec3i pos, vec3i vec, byte light)
        {
            Pos = pos;
            Vec = vec;
            Light = light;
            Sky = true;
            isNotEmpty = true;
        }

        public LightStruct(vec3i pos, byte light) : this(pos, new vec3i(0), light) { }

        public LightStruct(vec3i pos, byte light, bool sky) : this(pos, light) => Sky = sky;

        public override string ToString() => string.Format("{0} {2}{1}", Pos, Sky ? "s" : "", Light);
    }
}
