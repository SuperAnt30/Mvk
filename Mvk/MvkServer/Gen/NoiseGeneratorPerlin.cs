using System;

namespace MvkServer.Gen
{
    /// <summary>
    /// Объект шума Перлина
    /// </summary>
    public class NoiseGeneratorPerlin
    {
        /// <summary>
        /// Сборник функций генерации шума. Выходной сигнал комбинируется для получения различных октав шума. 
        /// </summary>
        private readonly NoiseGeneratorSimplex[] generatorCollection;
        /// <summary>
        /// Количество октав
        /// </summary>
        private readonly int octaves;

        public NoiseGeneratorPerlin(Random random, int octave)
        {
            octaves = octave;
            generatorCollection = new NoiseGeneratorSimplex[octave];

            for (int i = 0; i < octave; i++)
            {
                generatorCollection[i] = new NoiseGeneratorSimplex(random);
            }
        }

        /// <summary>
        /// Генерация шума объёма в массив
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="yOffset">координата Y</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="ySize">ширина по Y</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="yScale">масштаб по Y</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <returns>вернёт массив noiseArray</returns>
        public float[] GenerateNoise3d(float[] noiseArray, int xOffset, int yOffset, int zOffset,
            int xSize, int ySize, int zSize, float xScale, float yScale, float zScale)
        {
            if (noiseArray == null)
            {
                noiseArray = new float[xSize * ySize * zSize];
            }
            else
            {
                for (int i = 0; i < noiseArray.Length; i++)
                {
                    noiseArray[i] = 0f;
                }
            }
            float d = 1f;

            for (int i = 0; i < octaves; i++)
            {
                float x = (float)xOffset * d * xScale;
                float y = (float)yOffset * d * yScale;
                float z = (float)zOffset * d * zScale;
                if (ySize == 1)
                {
                    generatorCollection[i].PopulateNoiseArray2d(noiseArray, x, z, xSize, zSize, xScale * d, zScale * d, d);
                }
                else
                {
                    generatorCollection[i].PopulateNoiseArray3d(noiseArray, x, y, z, xSize, ySize, zSize, xScale * d, yScale * d, zScale * d, d);
                }
                d /= 2.0f;
            }
            return noiseArray;
        }

        /// <summary>
        /// Генерация шума плоскости в массив
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <returns></returns>
        public float[] GenerateNoise2d(float[] noiseArray, int xOffset, int zOffset, int xSize, int zSize, float xScale, float zScale) 
            => GenerateNoise3d(noiseArray, xOffset, 100, zOffset, xSize, 1, zSize, xScale, 1.0f, zScale);
    }
}
