namespace MvkServer.Item
{
    /// <summary>
    ///  Базовый объект вещи
    /// </summary>
    public abstract class ItemBase
    {
        /// <summary>
        /// Максимальное количество однотипный вещей в одной ячейке
        /// </summary>
        public int MaxStackSize { get; protected set; } = 64;


        public virtual void Update()
        {
            //ItemStack stack, World worldIn, Entity entityIn, int itemSlot, boolean isSelected
        }
             
    }
}
