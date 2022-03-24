namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок камня
    /// </summary>
    public class BlockStone : BlockBase
    {
        /// <summary>
        /// Блок камня
        /// </summary>
        public BlockStone()
        {
            Boxes = new Box[] { new Box(0) };
            Particle = 0;
            Hardness = 25;
        }
    }
}
