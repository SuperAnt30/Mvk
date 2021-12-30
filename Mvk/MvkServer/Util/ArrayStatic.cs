using MvkServer.Glm;
using System.Collections.Generic;

namespace MvkServer.Util
{
    /// <summary>
    /// Статические переменные из массивов
    /// Многие формируются при открытии проекта
    /// </summary>
    public class ArrayStatic
    {
        /// <summary>
        /// Область в один блок без центра, 8 блоков
        /// </summary>
        public static vec2i[] AreaOne8 { get; } = new vec2i[] {
            new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
            new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
        };

        /// <summary>
        /// Область в один блок c центром, 9 блоков
        /// </summary>
        public static vec2i[] AreaOne9 { get; } = new vec2i[] { new vec2i(0, 0),
            new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
            new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
        };

        /// <summary>
        /// Параметр [0]=0 .. [16]=0.0625f
        /// </summary>
        public static float[] Uv { get; protected set; } = new float[17];
        /// <summary>
        /// Параметр [1]=0.0625f .. [16]=1.0f
        /// </summary>
        public static float[] Xy { get; protected set; } = new float[17];

        /// <summary>
        /// Инициализация, запускаем при старте
        /// </summary>
        public static void Initialized()
        {
            for (int i = 0; i <= 16; i++)
            {
                Uv[i] = (float)i * 0.00390625f;
                Xy[i] = (float)i * 0.0625f;
            }
        }

        /// <summary>
        /// Сгенерировать массив по длинам используя квадратный корень
        /// </summary>
        /// <param name="overview">Обзор, в одну сторону от ноля</param>
        public static vec2i[] GetSqrt(int overview)
        {
            List<ArrayDistance> r = new List<ArrayDistance>();
            for (int x = -overview; x <= overview; x++)
            {
                for (int y = -overview; y <= overview; y++)
                {
                    r.Add(new ArrayDistance(new vec2i(x, y), Mth.Sqrt(x * x + y * y)));
                }
            }
            r.Sort();
            vec2i[] list = new vec2i[r.Count];
            for (int i = 0; i < r.Count; i++)
            {
                list[i] = r[i].Position;
            }
            return list;
        }
    }
}
