using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок земли
    /// </summary>
    public class BlockDirt : BlockBase
    {
        /// <summary>
        /// Блок земли
        /// </summary>
        public BlockDirt()
        {
            Boxes = new Box[] { new Box(2, false, new vec3(.62f, .44f, .37f)) };
            Particle = 2;
            Hardness = 5;
            Slipperiness = 0.8f;
            Material = EnumMaterial.Dirt;
        }
    }
}
