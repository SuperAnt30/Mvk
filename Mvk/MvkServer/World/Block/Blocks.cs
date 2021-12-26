using MvkServer.Util;
using MvkServer.World.Block.Items;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень блоков
    /// </summary>
    public class Blocks
    {
        protected static BlockBase ToBlock(EnumBlock eBlock)
        {
            switch (eBlock)
            {
                case EnumBlock.Air: return new BlockAir();
                case EnumBlock.Stone: return new BlockStone();
                case EnumBlock.Cobblestone: return new BlockCobblestone();
                case EnumBlock.Dirt: return new BlockDirt();
                case EnumBlock.Turf: return new BlockTurf();
            }

            return null;
        }

        public static BlockBase GetBlock(EnumBlock eBlock, BlockPos pos)
        {
            BlockBase block = ToBlock(eBlock);
            if (block != null) block.SetPosition(pos);
            return block;
        }

        /// <summary>
        /// Получить блок воздуха
        /// </summary>
        public static BlockBase GetAir(BlockPos pos) => GetBlock(EnumBlock.Air, pos);
    }
}
