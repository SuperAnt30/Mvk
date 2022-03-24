using MvkServer.World;
using System;

namespace MvkServer.Gen
{
    /// <summary>
    /// Объект хранение шумов
    /// </summary>
    public class NoiseStorge
    {
        public NoiseStorge(WorldServer worldIn)
        {
            HeightBiome = new NoiseGeneratorPerlin(new Random(worldIn.Seed), 8);
            WetnessBiome = new NoiseGeneratorPerlin(new Random(worldIn.Seed + 8), 8);
            Cave = new NoiseGeneratorPerlin(new Random(worldIn.Seed + 2), 2);
            Down = new NoiseGeneratorPerlin(new Random(worldIn.Seed), 1);
            Area = new NoiseGeneratorPerlin(new Random(worldIn.Seed + 2), 1);
        }

        /// <summary>
        /// Шум высот биомов
        /// </summary>
        public NoiseGeneratorPerlin HeightBiome { get; protected set; }
        /// <summary>
        /// Шум влажности биомов
        /// </summary>
        public NoiseGeneratorPerlin WetnessBiome { get; protected set; }
        /// <summary>
        /// Шум пещер
        /// </summary>
        public NoiseGeneratorPerlin Cave { get; protected set; }
        /// <summary>
        /// Шум облостей
        /// </summary>
        public NoiseGeneratorPerlin Area { get; protected set; }
        /// <summary>
        /// Шум нижнего слоя
        /// </summary>
        public NoiseGeneratorPerlin Down { get; protected set; }
    }
}
