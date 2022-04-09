using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block.List;
using System;
using System.Collections.Generic;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень блоков
    /// </summary>
    public class Blocks
    {
        /// <summary>
        /// Массив всех кэш блоков
        /// </summary>
        private static Dictionary<EnumBlock, BlockBase> blocks = new Dictionary<EnumBlock, BlockBase>();

        private static BlockBase ToBlock(EnumBlock eBlock)
        {
            switch (eBlock)
            {
                case EnumBlock.None: return new BlockAir(true);
                case EnumBlock.Air: return new BlockAir();
                case EnumBlock.Stone: return new BlockStone();
                case EnumBlock.Cobblestone: return new BlockCobblestone();
                case EnumBlock.Dirt: return new BlockDirt();
                case EnumBlock.Turf: return new BlockTurf();
                case EnumBlock.Water: return new BlockWater();
                case EnumBlock.Glass: return new BlockGlass();
                case EnumBlock.GlassRed: return new BlockGlassRed();
            }

            return null;
        }

        /// <summary>
        /// Инициализировать все блоки для кэша
        /// </summary>
        public static void Initialized()
        {
            foreach (EnumBlock eBlock in Enum.GetValues(typeof(EnumBlock)))
            {
                BlockBase block = ToBlock(eBlock);
                if (block != null)
                {
                    block.SetEnumBlock(eBlock);
                    blocks.Add(eBlock, block);
                }
            }
        }

        /// <summary>
        /// Получить объект блока с кеша, для получения информационных данных
        /// </summary>
        public static BlockBase GetBlockCache(EnumBlock eBlock) => blocks[eBlock];

        /// <summary>
        /// Создать объект блока с позицией
        /// </summary>
        public static BlockBase CreateBlock(EnumBlock eBlock, BlockPos pos)
        {
            BlockBase block = ToBlock(eBlock);
            if (block != null)
            {
                block.SetEnumBlock(eBlock);
                block.SetPosition(pos);
            }
            return block;
        }

        /// <summary>
        /// Создать объект блока воздуха с позицией
        /// </summary>
        public static BlockBase CreateAir(BlockPos pos) => CreateBlock(EnumBlock.Air, pos);
        /// <summary>
        /// Создать объект блока воздуха с позицией
        /// </summary>
        public static BlockBase CreateAir(vec3i pos) => CreateAir(new BlockPos(pos));
    }
}
