using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;
using MvkServer.World.Chunk;

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
            UpId();
        }

        public override string GetName() => "block." + Block.EBlock.ToString();

        /// <summary>
        ///  Вызывается, когда блок щелкают правой кнопкой мыши с этим элементом
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="playerIn"></param>
        /// <param name="worldIn"></param>
        /// <param name="blockPos">Блок, по которому щелкают правой кнопкой мыши</param>
        /// <param name="side">Сторона, по которой щелкнули правой кнопкой мыши</param>
        /// <param name="hit"></param>
        public override bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, EnumFacing side, vec3 hit)
        {
            BlockBase block = worldIn.GetBlock(blockPos);
            // Проверка ставить блоки, на те которые можно, к примеру на траву
            if (!block.CanPut) return false;
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

            BlockBase blockNew = Blocks.CreateBlock(Block.EBlock, blockPos);
            if (blockNew == null) return false;
            if (blockNew.IsAir) return false; // Проверка что блок можно ставить

            bool isCheckCollision = !blockNew.IsCollidable;
            if (!isCheckCollision)
            {
                AxisAlignedBB axisBlock = blockNew.GetCollision();
                // Проверка коллизии игрока и блока
                isCheckCollision = axisBlock != null && !playerIn.BoundingBox.IntersectsWith(axisBlock)
                    && worldIn.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, playerIn.Id).Count == 0;
            }

            if (isCheckCollision)
            {
                if (worldIn is WorldServer worldServer)
                {
                    return worldServer.SetBlockState(blockPos, blockNew.EBlock);
                }
                return true;
            }
            return false;

        }
    }
}
