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
            if (!block.IsAir)
            {
                //TODO::2022-04-01 Доп проверка ставить блоки, на те которые можно, к примеру на траву
                return false;
            }
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

            BlockBase blockNew = Blocks.GetBlock(Block.EBlock, blockPos);
            if (blockNew == null) return false;
            if (blockNew.IsAir) return false; // Проверка что блок можно ставить

            AxisAlignedBB axisBlock = blockNew.GetCollision();
            // Проверка коллизии игрока и блока
            if (axisBlock != null && !playerIn.BoundingBox.IntersectsWith(axisBlock) 
                && worldIn.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, playerIn.Id).Count == 0)
            {
                if (worldIn is WorldServer worldServer)
                {
                    worldServer.SetBlockState(blockPos, blockNew.EBlock);
                }
                return true;
                //if (worldIn.IsRemote)
                //{
                //     worldIn
                //    ClientMain.TrancivePacket(new PacketC08PlayerBlockPlacement(blockPos, facing));
                //}
                //itemInWorldManager.Put(blockPos, facing, blockNew.EBlock);
                //itemInWorldManager.PutPause(start);
                //putAbout = false;
                //Inventory.DecrStackSize(Inventory.CurrentItem, 1);
            }
            return false;

        }
    }
}
