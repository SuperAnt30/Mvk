using MvkServer.Glm;
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
                case EnumBlock.None: return new BlockAir(true);
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
            if (block != null)
            {
                block.SetEnumBlock(eBlock);
                block.SetPosition(pos);
            }
            return block;
        }
        public static BlockBase GetBlock(EnumBlock eBlock) => GetBlock(eBlock, new BlockPos());

        /// <summary>
        /// Получить блок воздуха
        /// </summary>
        public static BlockBase GetAir(BlockPos pos) => GetBlock(EnumBlock.Air, pos);
        /// <summary>
        /// Получить блок воздуха
        /// </summary>
        public static BlockBase GetAir(vec3i pos) => GetAir(new BlockPos(pos));
    }
}
