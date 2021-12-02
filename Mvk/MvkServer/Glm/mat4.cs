using System;
using System.Linq;

namespace MvkServer.Glm
{
    /// <summary>
    /// Represents a 4x4 matrix.
    /// </summary>
    public struct mat4
    {
        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="mat4"/> struct.
        /// This matrix is the identity matrix scaled by <paramref name="scale"/>.
        /// </summary>
        /// <param name="scale">The scale.</param>
        public mat4(float scale)
        {
            cols = new[]
                  {
                new vec4(scale, 0.0f, 0.0f, 0.0f),
                new vec4(0.0f, scale, 0.0f, 0.0f),
                new vec4(0.0f, 0.0f, scale, 0.0f),
                new vec4(0.0f, 0.0f, 0.0f, scale),
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="mat4"/> struct.
        /// The matrix is initialised with the <paramref name="cols"/>.
        /// </summary>
        /// <param name="cols">The colums of the matrix.</param>
        public mat4(vec4[] cols)
        {
            this.cols = new[]
                  {
                cols[0],
                cols[1],
                cols[2],
                cols[3]
            };
        }

        public mat4(vec4 a, vec4 b, vec4 c, vec4 d)
        {
            this.cols = new[]
            {
                a, b, c, d
            };
        }

        /// <summary>
        /// Creates an identity matrix.
        /// </summary>
        /// <returns>A new identity matrix.</returns>
        public static mat4 identity()
        {
            return new mat4
            {
                cols = new[]
                {
                    new vec4(1,0,0,0),
                    new vec4(0,1,0,0),
                    new vec4(0,0,1,0),
                    new vec4(0,0,0,1)
                }
            };
        }

        #endregion

        #region Index Access

        /// <summary>
        /// Gets or sets the <see cref="vec4"/> column at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="vec4"/> column.
        /// </value>
        /// <param name="column">The column index.</param>
        /// <returns>The column at index <paramref name="column"/>.</returns>
        public vec4 this[int column]
        {
            get { return cols[column]; }
            set { cols[column] = value; }
        }

        /// <summary>
        /// Gets or sets the element at <paramref name="column"/> and <paramref name="row"/>.
        /// </summary>
        /// <value>
        /// The element at <paramref name="column"/> and <paramref name="row"/>.
        /// </value>
        /// <param name="column">The column index.</param>
        /// <param name="row">The row index.</param>
        /// <returns>
        /// The element at <paramref name="column"/> and <paramref name="row"/>.
        /// </returns>
        public float this[int column, int row]
        {
            get { return cols[column][row]; }
            set { cols[column][row] = value; }
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Returns the matrix as a flat array of elements, column major.
        /// </summary>
        /// <returns></returns>
        public float[] to_array()
        {
            return cols.SelectMany(v => v.to_array()).ToArray();
        }

        #endregion

        #region Multiplication

        /// <summary>
        /// Multiplies the <paramref name="lhs"/> matrix by the <paramref name="rhs"/> vector.
        /// </summary>
        /// <param name="lhs">The LHS matrix.</param>
        /// <param name="rhs">The RHS vector.</param>
        /// <returns>The product of <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
        public static vec4 operator *(mat4 lhs, vec4 rhs)
        {
            return new vec4(
                lhs[0, 0] * rhs[0] + lhs[1, 0] * rhs[1] + lhs[2, 0] * rhs[2] + lhs[3, 0] * rhs[3],
                lhs[0, 1] * rhs[0] + lhs[1, 1] * rhs[1] + lhs[2, 1] * rhs[2] + lhs[3, 1] * rhs[3],
                lhs[0, 2] * rhs[0] + lhs[1, 2] * rhs[1] + lhs[2, 2] * rhs[2] + lhs[3, 2] * rhs[3],
                lhs[0, 3] * rhs[0] + lhs[1, 3] * rhs[1] + lhs[2, 3] * rhs[2] + lhs[3, 3] * rhs[3]
            );
        }

        /// <summary>
        /// Multiplies the <paramref name="lhs"/> matrix by the <paramref name="rhs"/> matrix.
        /// </summary>
        /// <param name="lhs">The LHS matrix.</param>
        /// <param name="rhs">The RHS matrix.</param>
        /// <returns>The product of <paramref name="lhs"/> and <paramref name="rhs"/>.</returns>
        public static mat4 operator *(mat4 lhs, mat4 rhs)
        {
            return new mat4(new[]
                  {
                rhs[0][0] * lhs[0] + rhs[0][1] * lhs[1] + rhs[0][2] * lhs[2] + rhs[0][3] * lhs[3],
                rhs[1][0] * lhs[0] + rhs[1][1] * lhs[1] + rhs[1][2] * lhs[2] + rhs[1][3] * lhs[3],
                rhs[2][0] * lhs[0] + rhs[2][1] * lhs[1] + rhs[2][2] * lhs[2] + rhs[2][3] * lhs[3],
                rhs[3][0] * lhs[0] + rhs[3][1] * lhs[1] + rhs[3][2] * lhs[2] + rhs[3][3] * lhs[3]
            });
        }

        public static mat4 operator *(mat4 lhs, float s)
        {
            return new mat4(new[]
            {
                lhs[0]*s,
                lhs[1]*s,
                lhs[2]*s,
                lhs[3]*s
            });
        }

        #endregion

        #region ToString support

        public override string ToString()
        {
            return String.Format(
                "[{0}, {1}, {2}, {3}; {4}, {5}, {6}, {7}; {8}, {9}, {10}, {11}; {12}, {13}, {14}, {15}]",
                this[0, 0], this[1, 0], this[2, 0], this[3, 0],
                this[0, 1], this[1, 1], this[2, 1], this[3, 1],
                this[0, 2], this[1, 2], this[2, 2], this[3, 2],
                this[0, 3], this[1, 3], this[2, 3], this[3, 3]
            );
        }
        #endregion

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
            if (obj.GetType() == typeof(mat4))
            {
                var mat = (mat4)obj;
                if (mat[0] == this[0] && mat[1] == this[1] && mat[2] == this[2] && mat[3] == this[3])
                    return true;
            }

            return false;
        }
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="m1">The first Matrix.</param>
        /// <param name="m2">The second Matrix.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(mat4 m1, mat4 m2)
        {
            return m1.Equals(m2);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="m1">The first Matrix.</param>
        /// <param name="m2">The second Matrix.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(mat4 m1, mat4 m2)
        {
            return !m1.Equals(m2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return this[0].GetHashCode() ^ this[1].GetHashCode() ^ this[2].GetHashCode() ^ this[3].GetHashCode();
        }
        #endregion

        /// <summary>
        /// The columms of the matrix.
        /// </summary>
        private vec4[] cols;
    }
}
