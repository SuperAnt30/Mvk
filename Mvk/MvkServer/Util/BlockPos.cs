using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Позиция блока
    /// </summary>
    public class BlockPos
    {
        public int X => Position.x;
        public int Y => Position.y;
        public int Z => Position.z;

        public bool IsEmpty { get; private set; } = false;
        public vec3i Position { get; private set; }

        public BlockPos() => IsEmpty = true;
        public BlockPos(int x, int y, int z) => Position = new vec3i(x, y, z);
        public BlockPos(float x, float y, float z) => Position = new vec3i(Mth.Floor(x), Mth.Floor(y), Mth.Floor(z));
        public BlockPos(vec3 v) => Position = new vec3i(Mth.Floor(v.x), Mth.Floor(v.y), Mth.Floor(v.z));
        public BlockPos(vec3i v) => Position = v;

        public BlockPos Add(float x, float y, float z) => Add(new BlockPos(x, y, z).Position);
        public BlockPos Add(int x, int y, int z) => Add(new vec3i(x, y, z));
        public BlockPos Add(vec3i v) => new BlockPos(Position + v);

        /// <summary>
        /// Позиция соседнего блока
        /// </summary>
        public BlockPos Offset(Pole pole) => new BlockPos(Position + EnumFacing.DirectionVec(pole));
        /// <summary>
        /// Позиция блока снизу
        /// </summary>
        public BlockPos OffsetDown() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.Down));
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        public BlockPos OffsetUp() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.Up));
        /// <summary>
        /// Позиция блока восток
        /// </summary>
        public BlockPos OffsetEast() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.East));
        /// <summary>
        /// Позиция блока запад
        /// </summary>
        public BlockPos OffsetWest() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.West));
        /// <summary>
        /// Позиция блока юг
        /// </summary>
        public BlockPos OffsetSouth() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.South));
        /// <summary>
        /// Позиция блока север
        /// </summary>
        public BlockPos OffsetNorth() => new BlockPos(Position + EnumFacing.DirectionVec(Pole.North));
        /// <summary>
        /// Позиция блока сверху
        /// </summary>
        public BlockPos OffsetUp(int i) => new BlockPos(Position + (EnumFacing.DirectionVec(Pole.Up) * i));

        public vec3 ToVec3() => new vec3(Position.x, Position.y, Position.z);
        public vec3 ToVec3Center() => new vec3(Position.x + .5f, Position.y + .5f, Position.z + .5f);

        /// <summary>
        /// Получить позицию блока в чанке, 0..15
        /// </summary>
        public vec3i GetPosition0() => new vec3i(Position.x & 15, Position.y, Position.z & 15);

        /// <summary>
        /// Получить позицию чанка XZ
        /// </summary>
        public vec2i GetPositionChunk() => new vec2i(Position.x >> 4, Position.z >> 4);

        /// <summary>
        /// Получить высоту псевдо чанка
        /// </summary>
        public int GetPositionChunkY() => Position.y >> 4;

        /// <summary>
        /// Получить растояние между двумя точками но не возводя в квадратный корень, на скорости
        /// </summary>
        public float DistanceNotSqrt(vec3 pos)
        {
            float x = Position.x - pos.x;
            float y = Position.y - pos.y;
            float z = Position.z - pos.z;
            return x * x + y * y + z * z;
        }

        /// <summary>
        /// Проверить локально позицию блока, 0..15
        /// </summary>
        public bool EqualsPosition0(int x, int y, int z) => (Position.x & 15) == x && Position.y == y && (Position.z & 15) == z;

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

        public override string ToString() => Position.ToString();
    }
}
