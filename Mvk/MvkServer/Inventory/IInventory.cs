using MvkServer.Item;

namespace MvkServer.Inventory
{
    public interface IInventory
    {
        /// <summary>
        /// Возвращает количество слотов в инвентаре
        /// </summary>
        int GetSizeInventory();

        /// <summary>
        /// Возвращает стек в слоте slotIn
        /// </summary>
        ItemStack GetStackInSlot(int slotIn);

        /// <summary>
        /// Устанавливает данный стек предметов в указанный слот в инвентаре (может быть раздел крафта или брони).
        /// </summary>
        void SetInventorySlotContents(int index, ItemStack stack);

        /// <summary>
        /// Добавляет стек предметов в инвентарь, возвращает false, если это невозможно.
        /// </summary>
        bool AddItemStackToInventory(ItemStack stack);

        /// <summary>
        /// Удаляет из слота инвентаря (первый аргумент) до указанного количества (второй аргумент) предметов и возвращает их в новый стек.
        /// </summary>
        ItemStack DecrStackSize(int index, int count);
    }
}
