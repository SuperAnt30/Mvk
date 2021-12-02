using System;

namespace MvkServer.Glm
{
    /// <summary>
    /// Представляет двумерный вектор
    /// </summary>
    public struct vec2i
    {
        public int x;
        public int y;

        public int this[int index]
        {
            get
            {
                if (index == 0) return x;
                else if (index == 1) return y;
                else throw new Exception("Out of range.");
            }
            set
            {
                if (index == 0) x = value;
                else if (index == 1) y = value;
                else throw new Exception("Out of range.");
            }
        }

        public vec2i(int s)
        {
            x = y = s;
        }

        public vec2i(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public vec2i(vec2i v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        public static vec2i operator +(vec2i lhs, vec2i rhs)
        {
            return new vec2i(lhs.x + rhs.x, lhs.y + rhs.y);
        }

        public static vec2i operator +(vec2i lhs, int rhs)
        {
            return new vec2i(lhs.x + rhs, lhs.y + rhs);
        }

        public static vec2i operator -(vec2i lhs, vec2i rhs)
        {
            return new vec2i(lhs.x - rhs.x, lhs.y - rhs.y);
        }

        public static vec2i operator -(vec2i lhs, int rhs)
        {
            return new vec2i(lhs.x - rhs, lhs.y - rhs);
        }

        public static vec2i operator *(vec2i self, int s)
        {
            return new vec2i(self.x * s, self.y * s);
        }

        public static vec2i operator *(int s, vec2i self)
        {
            return new vec2i(self.x * s, self.y * s);
        }

        public static vec2i operator *(vec2i lhs, vec2i rhs)
        {
            return new vec2i(rhs.x * lhs.x, rhs.y * lhs.y);
        }

        public static vec2i operator /(vec2i lhs, int rhs)
        {
            return new vec2i(lhs.x / rhs, lhs.y / rhs);
        }

        public int[] to_array()
        {
            return new[] { x, y };
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
            if (obj.GetType() == typeof(vec2i))
            {
                var vec = (vec2i)obj;
                if (this.x == vec.x && this.y == vec.y)
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
        public static bool operator ==(vec2i v1, vec2i v2)
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
        public static bool operator !=(vec2i v1, vec2i v2)
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
            return this.x.GetHashCode() ^ this.y.GetHashCode();
        }

        #endregion

        #region ToString support

        public override string ToString()
        {
            return string.Format("{0}; {1}", x, y);
        }

        #endregion
    }
}
