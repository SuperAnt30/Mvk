using System;

namespace MvkServer.Util
{
    public static partial class Mth
    {
        /// <summary>
        /// Округляем в меньшую сторону
        /// </summary>
        public static int Floor(float d)
        {
            int i = (int)d;
            return d < i ? i - 1 : i;
        }

        /// <summary>
        /// Округляем в большую сторону
        /// </summary>
        public static int Ceiling(float d)
        {
            int i = (int)d;
            return d > i ? i + 1 : i;
        }

        /// <summary>
        /// Округляем до ближайшего целого
        /// </summary>
        public static int Round(float d)
        {
            return (int)Math.Round(d);
        }

        /// <summary>
        /// Квадратный корень
        /// </summary>
        public static float Sqrt(float d)
        {
            return (float)Math.Sqrt(d);
        }

        /// <summary>
        /// Вернуть обсалютное значение
        /// </summary>
        public static float Abs(float a) => a >= 0f ? a : -a;
        /// <summary>
        /// Вернуть обсалютное значение
        /// </summary>
        public static int Abs(int a) => a >= 0 ? a : -a;

        /// <summary>
        /// Возращаем наибольшее
        /// </summary>
        public static byte Max(byte v1, byte v2) => v1 > v2 ? v1 : v2;
        /// <summary>
        /// Возращаем наибольшее
        /// </summary>
        public static int Max(int v1, int v2) => v1 > v2 ? v1 : v2;
        /// <summary>
        /// Возращаем наибольшее
        /// </summary>
        public static float Max(float v1, float v2) => v1 > v2 ? v1 : v2;

        /// <summary>
        /// Возращаем наименьшее
        /// </summary>
        public static int Min(int v1, int v2) => v1 > v2 ? v2 : v1;
        /// <summary>
        /// Возращаем наименьшее
        /// </summary>
        public static float Min(float v1, float v2) => v1 > v2 ? v2 : v1;

        /// <summary>
        /// Получить среднее значение массива лонгов
        /// </summary>
        public static float Average(long[] items)
        {
            long count = 0;
            for (int i = 0; i < items.Length; i++) { count += items[i]; }
            return (float)count / (float)items.Length;
        }

        /// <summary>
        /// Одинаковые ли массивы с погрешностью
        /// </summary>
        public static bool EqualsArrayFloat(float[] ar1, float[] ar2, float fault)
        {
            if (ar1 != null && ar2 != null && ar1.Length == ar2.Length)
            {
                for (int i = 0; i < ar1.Length; i++)
                {
                    if (Abs(ar1[i] - ar2[i]) > fault) return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Возвращает значение первого параметра, ограниченное нижним и верхним пределами, 
        /// заданными вторым и третьим параметрами. 
        /// </summary>
        public static int Clamp(int param, int min, int max) => param < min ? min : (param > max ? max : param);
        /// <summary>
        /// Возвращает значение первого параметра, ограниченное нижним и верхним пределами, 
        /// заданными вторым и третьим параметрами. 
        /// </summary>
        public static float Clamp(float param, float min, float max) => param < min ? min : (param > max ? max : param);

    }
}
