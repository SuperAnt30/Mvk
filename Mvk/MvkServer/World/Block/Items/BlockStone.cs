namespace MvkServer.World.Block.Items
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
        }

        /// <summary>
        /// Значение для разрушения в тактах
        /// </summary>
        public override int GetDamageValue() => 25;
    }
}
