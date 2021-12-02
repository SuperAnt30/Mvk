using MvkServer.Util;

namespace MvkServer.Glm
{
    public static partial class glm
    {
        public static vec3 cross(vec3 lhs, vec3 rhs)
        {
            return new vec3(
                lhs.y * rhs.z - rhs.y * lhs.z,
                lhs.z * rhs.x - rhs.z * lhs.x,
                lhs.x * rhs.y - rhs.x * lhs.y);
        }

        public static float dot(vec2 x, vec2 y)
        {
            vec2 tmp = new vec2(x * y);
            return tmp.x + tmp.y;
        }

        public static float dot(vec3 x, vec3 y)
        {
            vec3 tmp = new vec3(x * y);
            return tmp.x + tmp.y + tmp.z;
        }

        public static float dot(vec4 x, vec4 y)
        {
            vec4 tmp = new vec4(x * y);
            return (tmp.x + tmp.y) + (tmp.z + tmp.w);
        }

        public static vec2 normalize(vec2 v)
        {
            float sqr = v.x * v.x + v.y * v.y;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        public static vec3 normalize(vec3 v)
        {
            float sqr = v.x * v.x + v.y * v.y + v.z * v.z;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        public static vec4 normalize(vec4 v)
        {
            float sqr = v.x * v.x + v.y * v.y + v.z * v.z + v.w * v.w;
            return v * (1.0f / Mth.Sqrt(sqr));
        }

        /// <summary>
        /// Вращение точки вокруг оси координат вокруг вектора
        /// </summary>
        /// <param name="pos">позиция точки</param>
        /// <param name="angle">угол</param>
        /// <param name="vec">вектор</param>
        public static vec3 rotate(vec3 pos, float angle, vec3 vec)
        {
            mat4 rotat = rotate(new mat4(1.0f), angle, vec);
            mat4 res = translate(rotat, pos);
            return new vec3(res);
        }
    }
}
