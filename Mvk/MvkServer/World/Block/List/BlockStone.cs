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
            Hardness = 6000000;// 25;
            Material = EnumMaterial.Stone;
            InitBoxs(0);
        }
    }
}
