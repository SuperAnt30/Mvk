using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стекла
    /// </summary>
    public class BlockGlass : BlockBase
    {
        /// <summary>
        /// Блок стекла
        /// </summary>
        public BlockGlass()
        {
            Boxes = new Box[] { new Box(5) };//, false, new vec3(1f, .53f, .99f)) };
            IsAlpha = true;
            Particle = 5;
            Hardness = 10;
            LightOpacity = 2;
            Material = EnumMaterial.Glass;
        }
    }
}
