using MvkServer.World.Block.List;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Перечень блоков
    /// </summary>
    public class Blocks
    {
        /// <summary>
        /// Массив прозрачности и излучаемости освещения, для ускорения алгоритмов освещения
        /// LightOpacity << 4 | LightValue
        /// LightOpacity = blocksLightOpacity[0] >> 4
        /// LightValue = blocksLightOpacity[0] & 0xF
        /// </summary>
        public static byte[] blocksLightOpacity;
        /// <summary>
        /// Массив всех кэш блоков
        /// </summary>
        public static BlockBase[] blocksInt;

        private static BlockBase ToBlock(EnumBlock eBlock)
        {
            switch (eBlock)
            {
              //  case EnumBlock.None: return new BlockAir(true);
                case EnumBlock.Air: return new BlockAir();
                case EnumBlock.Stone: return new BlockStone();
                case EnumBlock.Cobblestone: return new BlockCobblestone();
                case EnumBlock.Dirt: return new BlockDirt();
                case EnumBlock.Turf: return new BlockTurf();
                case EnumBlock.Water: return new BlockWater();
                case EnumBlock.Lava: return new BlockLava();
                case EnumBlock.Oil: return new BlockOil();
                case EnumBlock.Fire: return new BlockFire();
                case EnumBlock.Glass: return new BlockGlass();
               // case EnumBlock.GlassRed: return new BlockGlassRed();
                case EnumBlock.Brol: return new BlockBrol();
                case EnumBlock.LogOak: return new BlockLogOak();
                case EnumBlock.TallGrass: return new BlockTallGrass();
            }

            return new BlockAir(true);
        }

        /// <summary>
        /// Инициализировать все блоки для кэша
        /// </summary>
        public static void Initialized()
        {
            int count = BlocksCount.COUNT + 1;
            blocksInt = new BlockBase[count];
            blocksLightOpacity = new byte[count];

            for (int i = 0; i < count; i++)
            {
                EnumBlock enumBlock = (EnumBlock)i;
                BlockBase block = ToBlock(enumBlock);
                block.SetEnumBlock(enumBlock);
                blocksInt[i] = block;
                blocksLightOpacity[i] = (byte)(block.LightOpacity << 4 | block.LightValue);
            }
        }

        /// <summary>
        /// Получить объект блока с кеша, для получения информационных данных
        /// </summary>
        public static BlockBase GetBlockCache(EnumBlock eBlock) => blocksInt[(int)eBlock];
    }
}
