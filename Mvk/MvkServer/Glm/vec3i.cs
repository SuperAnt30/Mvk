using MvkServer.Util;
using System;

namespace MvkServer.Glm
{
    /// <summary>
    /// Представляет трехмерный вектор
    /// </summary>
    public struct vec3i
    {
        public int x;
        public int y;
        public int z;

        public int this[int index]
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

        public vec3i(int s)
        {
            x = y = z = s;
        }

        public vec3i(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public vec3i(vec3 v)
        {
            x = Mth.Floor(v.x);
            y = Mth.Floor(v.y);
            z = Mth.Floor(v.z);
        }

        public vec3i(vec2i xy, int z)
        {
            x = xy.x;
            y = xy.y;
            this.z = z;
        }

        public static vec3i operator +(vec3i lhs, vec3i rhs)
        {
            return new vec3i(lhs.x + rhs.x, lhs.y + rhs.y, lhs.z + rhs.z);
        }

        public static vec3i operator +(vec3i lhs, int rhs)
        {
            return new vec3i(lhs.x + rhs, lhs.y + rhs, lhs.z + rhs);
        }

        public static vec3i operator -(vec3i lhs, vec3i rhs)
        {
            return new vec3i(lhs.x - rhs.x, lhs.y - rhs.y, lhs.z - rhs.z);
        }

        public static vec3i operator -(vec3i lhs, int rhs)
        {
            return new vec3i(lhs.x - rhs, lhs.y - rhs, lhs.z - rhs);
        }

        public static vec3i operator *(vec3i self, int s)
        {
            return new vec3i(self.x * s, self.y * s, self.z * s);
        }
        public static vec3i operator *(int s, vec3i self)
        {
            return new vec3i(self.x * s, self.y * s, self.z * s);
        }

        public static vec3i operator /(vec3i lhs, int rhs)
        {
            return new vec3i(lhs.x / rhs, lhs.y / rhs, lhs.z / rhs);
        }

        public static vec3i operator *(vec3i lhs, vec3i rhs)
        {
            return new vec3i(rhs.x * lhs.x, rhs.y * lhs.y, rhs.z * lhs.z);
        }

        public int[] to_array()
        {
            return new[] { x, y, z };
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
            if (obj.GetType() == typeof(vec3i))
            {
                var vec = (vec3i)obj;
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
        public static bool operator ==(vec3i v1, vec3i v2)
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
        public static bool operator !=(vec3i v1, vec3i v2)
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
            return string.Format("{0}; {1}; {2}", x, y, z);
        }

        #endregion
    }
}
