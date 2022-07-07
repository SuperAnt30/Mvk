using MvkServer.Glm;
using MvkServer.Sound;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок земли
    /// </summary>
    public class BlockDirt : BlockBase
    {
        /// <summary>
        /// Блок земли
        /// </summary>
        public BlockDirt()
        {
            Particle = 2;
            Hardness = 5;
            Slipperiness = 0.8f;
            Material = EnumMaterial.Dirt;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepSand1, AssetsSample.StepSand2, AssetsSample.StepSand3, AssetsSample.StepSand4 };
            InitBoxs(2, false, new vec3(.62f, .44f, .37f));
        }

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public override int QuantityDropped(Random random) => 8;
    }
}
