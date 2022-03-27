using MvkServer.World.Block;

namespace MvkServer.Item.List
{
    /// <summary>
    /// Предмет блок
    /// </summary>
    public class ItemBlock : ItemBase
    {
        public BlockBase Block { get; private set; }

        public ItemBlock(BlockBase block)
        {
            Block = block;
        }
    }
}
