using MvkServer.Glm;
using System;

namespace MvkServer.Util
{
    public struct ArrayDistance : IComparable
    {
        private readonly vec3i pos;
        private readonly float distance;
        private readonly bool body;

        public ArrayDistance(vec3i pos, float distance)
        {
            this.pos = pos;
            this.distance = distance;
            body = true;
        }

        /// <summary>
        /// Позиция блока в этом чанке, скорее всего 0..15
        /// </summary>
        public vec3i Position() => pos;
        /// <summary>
        /// Дистанция
        /// </summary>
        public float Distance() => distance;
        /// <summary>
        /// Пустой ли объект
        /// </summary>
        public bool IsEmpty() => !body;
        /// <summary>
        /// Получить вектор 2д (x, y)
        /// </summary>
        public vec2i GetPos2d() => new vec2i(pos.x, pos.y);

        /// <summary>
        /// Метод для сортировки
        /// </summary>
        public int CompareTo(object obj)
        {
            if (obj is ArrayDistance v) return distance.CompareTo(v.Distance());
            else throw new Exception("Невозможно сравнить два объекта");
        }

        public override string ToString()
        {
            return string.Format("({0}) {1:0.00}", pos, distance);
        }
    }
}
