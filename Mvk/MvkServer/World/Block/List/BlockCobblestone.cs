namespace MvkServer.World.Block.List
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
            Particle = 1;
            Hardness = 25;
            Material = EnumMaterial.Stone;
            InitBoxs(1);
        }
    }
}
