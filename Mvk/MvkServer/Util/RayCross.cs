using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Луч пересечения
    /// </summary>
    public class RayCross
    {
        protected vec3 pos1, pos2;

        /// <summary>
        /// Создать объект луча задав сразу ночальную и конечную точку луча
        /// </summary>
        /// <param name="pos1">начальная точка</param>
        /// <param name="pos2">конечная точка</param>
        public RayCross(vec3 pos1, vec3 pos2)
        {
            this.pos1 = pos1;
            this.pos2 = pos2;
        }
        /// <summary>
        /// Создать объект луча задав положение луча и вектор направление
        /// </summary>
        /// <param name="a">начальная точка</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public RayCross(vec3 a, vec3 dir, float maxDist)
        {
            pos1 = a;
            pos2 = a + dir * maxDist;
        }

        /// <summary>
        /// Пересекает ли отрезок прямоугольник в объёме 
        /// </summary>
        /// <param name="from">меньшый угол прямоугольника</param>
        /// <param name="to">больший угол прямоугольника</param>
        public bool CrossLineToRectangle(vec3 from, vec3 to)
        {
            bool bxy = CrossLineToRectangle(
                new vec2(from), new vec2(to), new vec2(pos1), new vec2(pos2)
            );
            if (!bxy) return false;
            bool bxz = CrossLineToRectangle(
                new vec2(from.x, from.z), new vec2(to.x, to.z), new vec2(pos1.x, pos1.z), new vec2(pos2.x, pos2.z)
            );
            if (!bxz) return false;
            bool byz = CrossLineToRectangle(
                new vec2(from.y, from.z), new vec2(to.y, to.z), new vec2(pos1.y, pos1.z), new vec2(pos2.y, pos2.z)
            );
            return byz;
        }

        /// <summary>
        /// Пересекает ли отрезок прямоугольник в объёме 
        /// </summary>
        /// <param name="aabb">ограничительная рамка</param>
        public bool CrossLineToRectangle(AxisAlignedBB aabb) => CrossLineToRectangle(aabb.Min, aabb.Max);

        /// <summary>
        /// Пересекает ли луч хоть одину из рамок 
        /// </summary>
        /// <param name="aabbs">ограничительные рамки</param>
        public bool IsCrossAABBs(AxisAlignedBB[] aabbs)
        {
            foreach (AxisAlignedBB aabb in aabbs)
            {
                if (CrossLineToRectangle(aabb)) return true;
            }
            return false;
        }

        /// <summary>
        /// Пересекает ли отрезок прямоугольник в плоскости
        /// </summary>
        /// <param name="from">меньшый угол прямоугольника</param>
        /// <param name="to">больший угол прямоугольника</param>
        /// <param name="a">сторона отрезка</param>
        /// <param name="b">сторона отрезка</param>
        protected bool CrossLineToRectangle(vec2 from, vec2 to, vec2 a, vec2 b)
        {
            bool bc = CrossLineToTriangle(from, to, new vec2(from.x, to.y), a, b);
            return bc ? true : CrossLineToTriangle(from, new vec2(to.x, from.y), to, a, b);
        }

        /// <summary>
        /// Пересекает ли отрезок треугольник в плоскости
        /// </summary>
        /// <param name="a">вершина треугольника</param>
        /// <param name="b">вершина треугольника</param>
        /// <param name="c">вершина треугольника</param>
        /// <param name="x">сторона отрезка</param>
        /// <param name="y">сторона отрезка</param>
        /// <returns>true - пересекают</returns>
        protected bool CrossLineToTriangle(vec2 a, vec2 b, vec2 c, vec2 x, vec2 y)
        {
            // r1 == 3 -> треугольник по одну сторону от отрезка
            bool r1 = (3 != Mth.Abs(Relatively(x, y, a) + Relatively(x, y, b) + Relatively(x, y, c)));
            // r2 == 2 -> точки x,y по одну сторону от стороны ab
            bool r2 = (2 != Mth.Abs(Relatively(a, b, x) + Relatively(a, b, y)));
            // r3 == 2 -> точки x,y по одну сторону от стороны bc
            bool r3 = (2 != Mth.Abs(Relatively(b, c, x) + Relatively(b, c, y)));
            // r4 == 2 -> точки x,y по одну сторону от стороны ca
            bool r4 = (2 != Mth.Abs(Relatively(c, a, x) + Relatively(c, a, y)));
            // r2 == r3 == r4 == 2 -> точки x,y по одну сторону от треугольника abс

            return (r1 && (r2 || r3 || r4));
        }

        /// <summary>
        /// Вычисляет положение точки D относительно AB
        /// </summary>
        /// <returns>важен знак</returns>
        protected int Relatively(vec2 a, vec2 b, vec2 d)
        {
            float r = (d.x - a.x) * (b.y - a.y) - (d.y - a.y) * (b.x - a.x);
            if (Mth.Abs(r) < 0.000001) return 0;
            else if (r < 0) return -1;
            else return 1;
        }
    }
}
