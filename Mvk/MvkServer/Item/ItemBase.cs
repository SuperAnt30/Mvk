using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item.List;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Item
{
    /// <summary>
    ///  Базовый объект предметов
    /// </summary>
    public abstract class ItemBase
    {
        /// <summary>
        /// Максимальное количество однотипный вещей в одной ячейке
        /// </summary>
        public int MaxStackSize { get; protected set; } = 64;

        /// <summary>
        /// id предмета
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Обновить id
        /// </summary>
        protected void UpId() => Id = GetIdFromItem(this);

        public virtual void Update()
        {
            //ItemStack stack, World worldIn, Entity entityIn, int itemSlot, boolean isSelected
        }

        public virtual string GetName() => "base";

        /// <summary>
        /// Копия предмет
        /// </summary>
        public ItemBase Copy() => GetItemById(Id);

        /// <summary>
        /// По id сгенерировать объект предмета
        /// </summary>
        public static ItemBase GetItemById(int id)
        {
            // TODO:доделать
            if (id > 0 && id < 4096) // Block
            {
                return new ItemBlock(Blocks.GetBlock((EnumBlock)id));
            }
            // остальное предметы, пока их нет
            return null;
        }

        /// <summary>
        /// По предмету сгенерировать id
        /// </summary>
        public static int GetIdFromItem(ItemBase itemIn)
        {
            if (itemIn == null) return 0;

            if (itemIn is ItemBlock itemBlock)
            {
                return (int)itemBlock.Block.EBlock;
            }
            // остальное предметы, пока их нет
            return 0;
        }


        /// <summary>
        ///  Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="pos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="hit"></param>
        public virtual bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, EnumFacing side, vec3 hit) => false;
    }
}
