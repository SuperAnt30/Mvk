using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей воды
    /// </summary>
    public class BlockWater : BlockBase
    {
        /// <summary>
        /// Блок стоячей воды
        /// </summary>
        public BlockWater()
        {
            Boxes = new Box[] { new Box(63, true, new vec3(0.24f, 0.45f, 0.88f)) };
            IsAlphe = true;
            IsAction = false;
            //IsWater = true;
            IsCollidable = false;
            IsFullCube = false;
            CanPut = true;
            IsSpawn = false;
            Particle = 63;
            Hardness = 2;
            LightOpacity = 2;
        }
    }
}
