namespace MvkServer.World.Block.Items
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
            Boxes = new Box[] { new Box(2) };
        }
    }
}
