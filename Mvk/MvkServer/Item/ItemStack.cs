using MvkServer.Item.List;
using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    /// Объект одной ячейки предметов
    /// </summary>
    public class ItemStack
    {
        /// <summary>
        /// Объект предмета
        /// </summary>
        public ItemBase Item { get; private set; }
        /// <summary>
        /// Урон
        /// </summary>
        public int ItemDamage { get; private set; }
        /// <summary>
        /// Количество вещей в стаке
        /// </summary>
        public int Amount { get; private set; }

        public ItemStack(BlockBase block) : this(block, 1, 0) { }
        public ItemStack(BlockBase block, int amount) : this(block, amount, 0) { }
        public ItemStack(BlockBase block, int amount, int itemDamage) 
            : this(new ItemBlock(block), amount, itemDamage) { }

        public ItemStack(ItemBase item) : this(item, 1, 0) { }
        public ItemStack(ItemBase item, int amount) : this(item, amount, 0) { }
        public ItemStack(ItemBase item, int amount, int itemDamage)
        {
            Item = item;
            Amount = amount;
            ItemDamage = itemDamage;
            if (ItemDamage < 0) ItemDamage = 0;
        }

    }
}
