using MvkServer.Glm;
using System;

namespace MvkServer.Util
{
    public class ArrayDistance : IComparable
    {
        /// <summary>
        /// Расположение
        /// </summary>
        public vec2i Position { get; protected set; }
        /// <summary>
        /// Дистанция
        /// </summary>
        public float Distance { get; protected set; } = 0f;

        public ArrayDistance(vec2i pos, float distance)
        {
            Position = pos;
            Distance = distance;
        }

        /// <summary>
        /// Метод для сортировки
        /// </summary>
        public int CompareTo(object o)
        {
            if (o is ArrayDistance v)
                return Distance.CompareTo(v.Distance);
            else
                throw new Exception("Невозможно сравнить два объекта");
        }

        public override string ToString()
        {
            return string.Format("({0}) {1:0.00}", Position, Distance);
        }
    }
}
