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
            Particle = 0;
            Hardness = 25;
            Material = EnumMaterial.Stone;
            InitBoxs(0);
        }
    }
}
