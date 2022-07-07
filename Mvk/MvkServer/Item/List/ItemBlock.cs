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
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool ItemUse(ItemStack stack, EntityPlayer playerIn, WorldBase worldIn, BlockPos blockPos, Pole side, vec3 facing)
        {
            BlockState blockState = worldIn.GetBlockState(blockPos);
            BlockBase blockOld = blockState.GetBlock();
            // Проверка ставить блоки, на те которые можно, к примеру на траву
            if (!blockOld.IsReplaceable) return false;
            // Если устанавливаемый блок такой же как стоит
            if (blockOld == Block) return false;
            // Если стак пуст
            if (stack == null || stack.Amount == 0) return false;

           // BlockBase blockNew = Blocks.CreateBlock(Block.EBlock, blockPos);
           // if (blockNew == null) return false;
          // Block.Put
          //  if (Block.IsAir) return false; // Проверка что блок можно ставить

            bool isCheckCollision = !Block.IsCollidable;
            if (!isCheckCollision)
            {
                AxisAlignedBB axisBlock = Block.GetCollision(blockPos, blockState.Met());
                // Проверка коллизии игрока и блока
                isCheckCollision = axisBlock != null && !playerIn.BoundingBox.IntersectsWith(axisBlock)
                    && worldIn.GetEntitiesWithinAABB(ChunkBase.EnumEntityClassAABB.EntityLiving, axisBlock, playerIn.Id).Count == 0;
            }

            if (isCheckCollision)
            {
                //if (worldIn.IsRemote) return true;
                BlockState blockStateNew = new BlockState(Block.EBlock);
                BlockBase blockNew = blockStateNew.GetBlock();
                bool result = blockNew.Put(worldIn, blockPos, blockStateNew, side, facing);
                if (result) worldIn.PlaySound(playerIn, blockNew.SamplePut(worldIn), blockPos.ToVec3(), 1f, 1f);
                return result;
            }
            return false;
        }
    }
}
