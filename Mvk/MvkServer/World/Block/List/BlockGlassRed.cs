using MvkServer.Glm;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стекла красного
    /// </summary>
    public class BlockGlassRed : BlockGlass
    {
        /// <summary>
        /// Блок стекла красного
        /// </summary>
        public BlockGlassRed()
        {
            Color = new vec3(1f, 0f, 0f);
            InitBoxs(5, true, new vec3(1f, 0f, 0f));
        }
    }
}
