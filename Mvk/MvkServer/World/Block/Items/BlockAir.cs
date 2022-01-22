namespace MvkServer.World.Block.Items
{
    /// <summary>
    /// Блок воздуха, пустота
    /// </summary>
    public class BlockAir : BlockBase
    {
        public BlockAir()
        {
            Boxes = new Box[] { new Box(33) };

            IsCollidable = false;
        }
    }
}
