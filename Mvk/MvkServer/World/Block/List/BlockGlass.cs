using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стекла
    /// </summary>
    public class BlockGlass : BlockBase
    {
        /// <summary>
        /// Блок стекла
        /// </summary>
        public BlockGlass()
        {
            Translucent = true;
            Particle = 5;
            Hardness = 10;
            LightOpacity = 2;
            Material = EnumMaterial.Glass;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
            InitBoxs(5);
        }

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        public override ItemBase GetItemDropped(BlockState state, Random rand, int fortune) 
            => new ItemBlock(Blocks.GetBlockCache(EnumBlock.GlassRed));
    }
}
