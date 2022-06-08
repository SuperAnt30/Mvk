using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Позиция блока
    /// </summary>
    public struct BlockPos
    {
        public int X;
        public int Y;
        public int Z;

        public BlockPos(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public BlockPos(float x, float y, float z)
        {
            X = Mth.Floor(x);
            Y = Mth.Floor(y);
            Z = Mth.Floor(z);
        }
        public BlockPos(vec3 v)
        {
            X = Mth.Floor(v.x);
            Y = Mth.Floor(v.y);
            Z = Mth.Floor(v.z);
        }
        public BlockPos(vec3i v)
        {
            X = v.x;
            Y = v.y;
            Z = v.z;
        }

        private BlockPos Plus(float x, float y, float z) => new BlockPos(X + Mth.Floor(x), Y + Mth.Floor(y), Z + Mth.Floor(z));
        private BlockPos Plus(int x, int y, int z) => new BlockPos(X + x, Y + y, Z + z);
        private BlockPos Plus(vec3i v) => new BlockPos(X + v.x, Y + v.y, Z + v.z);

        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        public BlockPos Offset(Pole pole) => Plus(EnumFacing.DirectionVec(pole));
        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        public BlockPos Offset(vec3i vec) => Plus(vec);
        /// <summary>
        /// Позиция блока снизу
        /// </summary>
        public BlockPos OffsetDown() => Plus(EnumFacing.DirectionVec(Pole.Down));
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        public BlockPos OffsetUp() => Plus(EnumFacing.DirectionVec(Pole.Up));
        /// <summary>
        /// Позиция блока восток
        /// </summary>
        public BlockPos OffsetEast() => Plus(EnumFacing.DirectionVec(Pole.East));
        /// <summary>
        /// Позиция блока запад
        /// </summary>
        public BlockPos OffsetWest() => Plus(EnumFacing.DirectionVec(Pole.West));
        /// <summary>
        /// Позиция блока юг
        /// </summary>
        public BlockPos OffsetSouth() => Plus(EnumFacing.DirectionVec(Pole.South));
        /// <summary>
        /// Позиция блока север
        /// </summary>
        public BlockPos OffsetNorth() => Plus(EnumFacing.DirectionVec(Pole.North));
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        public BlockPos OffsetUp(int i) => Plus(EnumFacing.DirectionVec(Pole.Up) * i);

        public vec3i ToVec3i() => new vec3i(X, Y, Z);
        public vec3 ToVec3() => new vec3(X, Y, Z);
        public vec3 ToVec3Center() => new vec3(X + .5f, Y + .5f, Z + .5f);

        /// <summary>
        /// Получить позицию блока в чанке, 0..15 0..255 0..15
        /// </summary>
        public vec3i GetPosition0() => new vec3i(X & 15, Y, Z & 15);

        /// <summary>
        /// Получить позицию чанка XZ
        /// </summary>
        public vec2i GetPositionChunk() => new vec2i(X >> 4, Z >> 4);

        /// <summary>
        /// Получить высоту псевдо чанка
        /// </summary>
        public int GetPositionChunkY() => Y >> 4;

        /// <summary>
        /// Получить растояние между двумя точками но не возводя в квадратный корень, на скорости
        /// </summary>
        public float DistanceNotSqrt(vec3 pos)
        {
            float x = X - pos.x;
            float y = Y - pos.y;
            float z = Z - pos.z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Проверить локально позицию блока, 0..15
        /// </summary>
        public bool EqualsPosition0(int x, int y, int z) => (X & 15) == x && Y == y && (Z & 15) == z;

        /// <summary>
        /// Проверьте, имеет ли данный BlockPos действительные координаты
        /// </summary>
        public bool IsValid() => X >= -30000000 && Z >= -30000000 && X < 30000000 && Z < 30000000 && Y >= 0 && Y < 256;

        /// <summary>
        /// Проверьте, имеет ли данный BlockPos действительные координаты только по Y
        /// </summary>
        public bool IsValidY() => Y >= 0 && Y < 256;

        /// <summary>
        /// Получить массив всех позиция попадающих в облость
        /// </summary>
        public static BlockPos[] GetAllInBox(vec3i from, vec3i to)
        {
            vec3i f = new vec3i(Mth.Min(from.x, to.x), Mth.Min(from.y, to.y), Mth.Min(from.z, to.z));
            vec3i t = new vec3i(Mth.Max(from.x, to.x), Mth.Max(from.y, to.y), Mth.Max(from.z, to.z));

            BlockPos[] list = new BlockPos[(t.x - f.x + 1) * (t.y - f.y + 1) * (t.z - f.z + 1)];

            int i = 0;
            for (int x = f.x; x <= t.x; x++)
            {
                for (int y = f.y; y <= t.y; y++)
                {
                    for (int z = f.z; z <= t.z; z++)
                    {
                        list[i++] = new BlockPos(x, y, z);
                    }
                }
            }
            return list;
        }

        public override string ToString() => string.Format("{0}; {1}; {2}", X, Y, Z);

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(BlockPos))
            {
                var vec = (BlockPos)obj;
                if (X == vec.X && Y == vec.Y && Z == vec.Z) return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
    }
}
