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

        public vec3i Position { get; protected set; }

        public BlockPos() { }
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
        /// Позиция блока сверху
        /// </summary>
        public BlockPos OffsetUp(int i) => new BlockPos(Position + (EnumFacing.DirectionVec(Pole.Up) * i));

        public vec3 ToVec3() => new vec3(Position.x, Position.y, Position.z);

        public override string ToString() => Position.ToString();
    }
}
