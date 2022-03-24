using System;

namespace MvkServer.Gen
{
    /// <summary>
    /// Объект обработки шума Перлина одной актавы
    /// </summary>
    public class NoiseGeneratorSimplex
    {
        private int[] permutations;
        public float xCoord;
        public float yCoord;
        public float zCoord;
        private static readonly float[] arStat1 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] arStat2 = new float[] { 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f };
        private static readonly float[] arStat3 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };
        private static readonly float[] arStat4 = new float[] { 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 1.0f, -1.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 0.0f };
        private static readonly float[] arStat5 = new float[] { 0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 1.0f, -1.0f, -1.0f, 1.0f, 1.0f, -1.0f, -1.0f, 0.0f, 1.0f, 0.0f, -1.0f };

        public NoiseGeneratorSimplex() => Generator(new Random());
        public NoiseGeneratorSimplex(Random random) => Generator(random);

        protected void Generator(Random random)
        {
            permutations = new int[512];
            xCoord = (float)random.NextDouble() * 256f;
            yCoord = (float)random.NextDouble() * 256f;
            zCoord = (float)random.NextDouble() * 256f;
            int i;

            for (i = 0; i < 256; permutations[i] = i++) ;

            for (i = 0; i < 256; i++)
            {
                int r = random.Next(256 - i) + i;
                int old = permutations[i];
                permutations[i] = permutations[r];
                permutations[r] = old;
                permutations[i + 256] = permutations[i];
            }
        }

        public float Lerp(float f1, float f2, float f3) => f2 + f1 * (f3 - f2);

        /// <summary>
        /// Умнажаем первые значения с массива и суммируем их
        /// </summary>
        public float Multiply(int index, float x, float y)
        {
            int i = index & 15;
            return arStat4[i] * x + arStat5[i] * y;
        }

        public float Grad(int index, float x, float y, float z)
        {
            int i = index & 15;
            return arStat1[i] * x + arStat2[i] * y + arStat3[i] * z;
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
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray3d(float[] noiseArray, float xOffset, float yOffset, float zOffset,
            int xSize, int ySize, int zSize, float xScale, float yScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            float ler1 = 0.0f;
            float ler2 = 0.0f;
            float ler3 = 0.0f;
            float ler4 = 0.0f;

            for (int x = 0; x < xSize; x++)
            {
                float xd = xOffset + (float)x * xScale + xCoord;
                int xi = (int)xd;
                if (xd < (float)xi) xi--;
                int xa = xi & 255;
                xd -= (float)xi;
                float xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (int z = 0; z < zSize; z++)
                {
                    float zd = zOffset + (float)z * zScale + zCoord;
                    int zi = (int)zd;
                    if (zd < (float)zi) zi--;
                    int za = zi & 255;
                    zd -= (float)zi;
                    float zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);

                    for (int y = 0; y < ySize; ++y)
                    {
                        float yd = yOffset + (float)y * yScale + yCoord;
                        int yi = (int)yd;
                        if (yd < (float)yi) yi--;
                        int ya = yi & 255;
                        yd -= (float)yi;
                        float yr = yd * yd * yd * (yd * (yd * 6.0f - 15.0f) + 10.0f);

                        if (y == 0 || ya >= 0)
                        {
                            int p1 = permutations[xa] + ya;
                            int p2 = permutations[p1] + za;
                            int p3 = permutations[p1 + 1] + za;
                            int p4 = permutations[xa + 1] + ya;
                            int p5 = permutations[p4] + za;
                            int p6 = permutations[p4 + 1] + za;
                            ler1 = Lerp(xr, Grad(permutations[p2], xd, yd, zd), Grad(permutations[p5], xd - 1, yd, zd));
                            ler2 = Lerp(xr, Grad(permutations[p3], xd, yd - 1, zd), Grad(permutations[p6], xd - 1, yd - 1, zd));
                            ler3 = Lerp(xr, Grad(permutations[p2 + 1], xd, yd, zd - 1), Grad(permutations[p5 + 1], xd - 1, yd, zd - 1));
                            ler4 = Lerp(xr, Grad(permutations[p3 + 1], xd, yd - 1, zd - 1), Grad(permutations[p6 + 1], xd - 1, yd - 1, zd - 1));
                        }

                        float ler5 = Lerp(yr, ler1, ler2);
                        float ler6 = Lerp(yr, ler3, ler4);
                        float ler7 = Lerp(zr, ler5, ler6);
                        noiseArray[count] += ler7 * noise;
                        count++;
                    }
                }
            }
        }

        /// <summary>
        /// Генерация шума плоскости в массиве
        /// </summary>
        /// <param name="noiseArray">массив</param>
        /// <param name="xOffset">координата Х</param>
        /// <param name="zOffset">координата Z</param>
        /// <param name="xSize">ширина по Х</param>
        /// <param name="zSize">ширина по Z</param>
        /// <param name="xScale">масштаб по X</param>
        /// <param name="zScale">масштаб по Z</param>
        /// <param name="noiseScale">масштаб шума</param>
        public void PopulateNoiseArray2d(float[] noiseArray, float xOffset, float zOffset,
            int xSize, int zSize, float xScale, float zScale, float noiseScale)
        {
            float noise = 1.0f / noiseScale;
            int count = 0;

            for (int x = 0; x < xSize; x++)
            {
                float xd = xOffset + (float)x * xScale + xCoord;
                int xi = (int)xd;
                if (xd < (float)xi) xi--;
                int xa = xi & 255;
                xd -= (float)xi;
                float xr = xd * xd * xd * (xd * (xd * 6.0f - 15.0f) + 10.0f);

                for (int z = 0; z < zSize; z++)
                {
                    float zd = zOffset + (float)z * zScale + zCoord;
                    int zi = (int)zd;
                    if (zd < (float)zi) zi--;
                    int za = zi & 255;
                    zd -= (float)zi;
                    float zr = zd * zd * zd * (zd * (zd * 6.0f - 15.0f) + 10.0f);
                    int p1 = permutations[xa] + 0;
                    int p2 = permutations[p1] + za;
                    int p3 = permutations[xa + 1] + 0;
                    int p4 = permutations[p3] + za;
                    float ler1 = Lerp(xr, Multiply(permutations[p2], xd, zd), Grad(permutations[p4], xd - 1.0f, 0.0f, zd));
                    float ler2 = Lerp(xr, Grad(permutations[p2 + 1], xd, 0.0f, zd - 1.0f), Grad(permutations[p4 + 1], xd - 1.0f, 0.0f, zd - 1.0f));
                    float ler3 = Lerp(zr, ler1, ler2);
                    noiseArray[count] += ler3 * noise;
                    count++;
                }
            }
        }
    }
}
