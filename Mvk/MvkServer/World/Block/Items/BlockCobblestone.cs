using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World.Block.Items
{
    /// <summary>
    /// Блок булыжника
    /// </summary>
    public class BlockCobblestone : BlockBase
    {
        /// <summary>
        /// Блок булыжника
        /// </summary>
        public BlockCobblestone()
        {
            //Boxes = new Box[] { new Box(1) };

            Boxes = new Box[] {
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
                    Faces = new Face[] { new Face(Pole.Down, 1) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[8], 0, MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[1]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[15]),
                    Faces = new Face[] { new Face(Pole.Up, 1) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[1]),
                    UVTo = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[15]),
                    Faces = new Face[] { new Face(Pole.Up, 1) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[8]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 1),
                        new Face(Pole.South, 1),
                        new Face(Pole.East, 1)
                    }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[16]),
                    Faces = new Face[] { new Face(Pole.West, 1) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[1], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
                    Faces = new Face[] { new Face(Pole.East, 1) }
                },
                new Box()
                {
                    From = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]),
                    To = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]),
                    UVFrom = new vec2(MvkStatic.Uv[8], MvkStatic.Uv[0]),
                    UVTo = new vec2(MvkStatic.Uv[15], MvkStatic.Uv[8]),
                    Faces = new Face[]
                    {
                        new Face(Pole.North, 1),
                        new Face(Pole.South, 1)
                    }
                }
            };

            Particle = 1;
            IsBoundingBoxAll = false;
            AllDrawing = true;
        }

        /// <summary>
        /// Передать список ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList()
        {
            vec3 pos = Position.ToVec3();
            vec3 min, max;

            //AxisAlignedBB[] aabbs = new AxisAlignedBB[1];
            //min = new vec3(0);
            //max = new vec3(1, MvkStatic.Xy[8], 1);
            //aabbs[0] = new AxisAlignedBB(pos + min, pos + max);


            AxisAlignedBB[] aabbs = new AxisAlignedBB[2];
            min = new vec3(MvkStatic.Xy[1], 0, MvkStatic.Xy[1]);
            max = new vec3(MvkStatic.Xy[15], MvkStatic.Xy[8], MvkStatic.Xy[15]);
            aabbs[0] = new AxisAlignedBB(pos + min, pos + max);
            min = new vec3(MvkStatic.Xy[1], MvkStatic.Xy[8], MvkStatic.Xy[1]);
            max = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[15]);
            aabbs[1] = new AxisAlignedBB(pos + min, pos + max);

            //AxisAlignedBB[] aabbs = new AxisAlignedBB[2];
            //min = new vec3(0);
            //max = new vec3(MvkStatic.Xy[16], MvkStatic.Xy[8], MvkStatic.Xy[16]);
            //aabbs[0] = GetBoundingBox();
            //min = new vec3(0, MvkStatic.Xy[9], 0);
            //max = new vec3(MvkStatic.Xy[8], MvkStatic.Xy[16], MvkStatic.Xy[16]);
            //aabbs[1] = GetBoundingBox();
            return aabbs;
        }
    }
}
