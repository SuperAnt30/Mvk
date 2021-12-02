using System;

namespace MvkServer.Glm
{
    /// <summary>
    /// Представляет трехмерный вектор
    /// </summary>
    public struct vec3
    {
        public float x;
        public float y;
        public float z;

        public float this[int index]
        {
            get
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else if (index == 2) return z;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) x = value;
                else if (index == 1) y = value;
                else if (index == 2) z = value;
                else throw new Exception("Out of range.");
            }
        }

        public vec3(float s)
        {
            x = y = z = s;
        }

        public vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public vec3(vec3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public vec3(vec3i v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public vec3(vec4 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public vec3(vec2 xy, float z)
        {
            x = xy.x;
            y = xy.y;
            this.z = z;
        }

        /// <summary>
        /// Получить точку из матрици перемещения, вращения
        /// </summary>
        /// <param name="m4"></param>
        public vec3(mat4 m4)
        {
            x = m4[3][0];
            y = m4[3][1];
            z = m4[3][2];
        }

        /// <summary>
        /// Угол по вертикали в радианах
        /// </summary>
        public vec3 rotatePitch(float angle)
        {
            float c = glm.cos(angle);
            float s = glm.sin(angle);
            float y2 = y * c + z * s;
            float z2 = z * c - y * s;
            return new vec3(x, y2, z2);
        }

        /// <summary>
        /// Угол по горизонтали в радианах
        /// </summary>
        public vec3 rotateYaw(float angle)
        {
            float c = glm.cos(angle);
            float s = glm.sin(angle);
            float x2 = x * c + z * s;
            float z2 = z * c - x * s;
            return new vec3(x2, y, z2);
        }

        public static vec3 operator +(vec3 lhs, vec3 rhs)
        {
            return new vec3(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static vec3 operator +(vec3 lhs, float rhs)
        {
            return new vec3(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs);
        }

        public static vec3 operator -(vec3 lhs, vec3 rhs)
        {
            return new vec3(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static vec3 operator -(vec3 lhs, float rhs)
        {
            return new vec3(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
        }

        public static vec3 operator *(vec3 self, float s)
        {
            return new vec3(self.x * s, self.y * s, self.z * s);
        }
        public static vec3 operator *(float s, vec3 self)
        {
            return new vec3(self.x * s, self.y * s, self.z * s);
        }

        public static vec3 operator /(vec3 lhs, float rhs)
        {
            return new vec3(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        }

        public static vec3 operator *(vec3 lhs, vec3 rhs)
        {
            return new vec3(rhs.x * lhs.x, rhs.y * lhs.y, rhs.z * lhs.z);
        }

        public float[] to_array()
        {
            return new[] { x, y, z };
        }

        public vec3 normalize()
        {
            return glm.normalize(this);
        }

        #region Comparision

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// The Difference is detected by the different values
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(vec3))
            {
                var vec = (vec3)obj;
                if (this.x == vec.x && this.y == vec.y && this.z == vec.z)
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(vec3 v1, vec3 v2)
        {
            return v1.Equals(v2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="v1">The first Vector.</param>
        /// <param name="v2">The second Vector.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(vec3 v1, vec3 v2)
        {
            return !v1.Equals(v2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this.x.GetHashCode() ^ this.y.GetHashCode() ^ this.z.GetHashCode();
        }

        #endregion

        #region ToString support

        public override string ToString()
        {
            return string.Format("{0:0.00}; {1:0.00}; {2:0.00}", x, y, z);
        }

        #endregion
    }
}
