using MvkServer.Glm;
using System;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект перечня сторон
    /// </summary>
    public class EnumFacing
    {
        /// <summary>
        /// Нормализованный вектор, указывающий в направлении этой облицовки
        /// </summary>
        public static vec3i DirectionVec(Pole pole)
        {
            switch (pole)
            {
                case Pole.Up: return new vec3i(0, 1, 0);
                case Pole.Down: return new vec3i(0, -1, 0);
                case Pole.East: return new vec3i(1, 0, 0);
                case Pole.West: return new vec3i(-1, 0, 0);
                case Pole.North: return new vec3i(0, 0, -1);
                case Pole.South: return new vec3i(0, 0, 1);
                default: throw new ArgumentNullException("Не существует такой стороны");
            }

        }

        /// <summary>
        /// Получите облицовку, соответствующую заданному углу (0-360). Угол 0 - SOUTH, угол 90 - WEST.
        /// </summary>
        /// <param name="angle">угол в градусах</param>
        public static Pole FromAngle(float angle)
        {
            if (angle >= -45f && angle <= 45f) return Pole.North;
            else if (angle > 45f && angle < 135f) return Pole.West;
            else if (angle < -45f && angle > -135f) return Pole.East;
            return Pole.South;
        }

        /// <summary>
        /// Проверить левее ли от тикущего полюса
        /// </summary>
        /// <param name="angle">угол в градусах</param>
        public static bool IsFromAngleLeft(float angle, Pole pole)
        {
            if (pole == Pole.North) return angle > 0;
            if (pole == Pole.West) return angle > 90f;
            if (pole == Pole.South) return angle < 0;
            if (pole == Pole.East) return angle > -90f;
            return true;
        }


        /// <summary>
        /// Получите сторону по его горизонтальному индексу (0-3). Заказ S-W-N-E.
        /// </summary>
        /// <param name="index">индекс (0-3)</param>
        public static Pole GetHorizontal(int index)
        {
            switch (index)
            {
                case 2: return Pole.North;
                case 0: return Pole.South;
                case 1: return Pole.West;
                case 3: return Pole.East;
                default: throw new ArgumentNullException("Не существует такой стороны");
            }
        }
        /// <summary>
        /// Нормализованный горизонтальный вектор, указывающий в направлении этой облицовки
        /// </summary>
        public static vec3i DirectionHorizontalVec(Pole pole)
        {
            switch (pole)
            {
                case Pole.East: return new vec3i(1, 0, 0);
                case Pole.West: return new vec3i(-1, 0, 0);
                case Pole.North: return new vec3i(0, 0, -1);
                case Pole.South: return new vec3i(0, 0, 1);
                default: throw new ArgumentNullException("Не существует такой стороны");
            }
        }

        /// <summary>
        /// Нормализованный горизонтальный вектор, (0-3). Заказ S-W-N-E.
        /// </summary>
        /// <param name="index">индекс (0-3)</param>
        public static vec3i DirectionHorizontalVec(int index) => DirectionHorizontalVec(GetHorizontal(index));
    }
}
