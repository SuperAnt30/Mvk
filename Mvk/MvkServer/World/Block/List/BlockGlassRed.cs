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
            Boxes = new Box[] { new Box(5, true, new vec3(1f, 0f, 0f)) };// { RotateYaw = glm.pi45 } };
            Color = new vec3(1f, 0f, 0f);
        }
    }
}
