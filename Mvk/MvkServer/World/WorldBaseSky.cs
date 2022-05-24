using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.World
{
    /// <summary>
    /// Продолжение мира, но тут все расчёты по небу
    /// </summary>
    public abstract partial class WorldBase
    {

        private const int SPEED_DAY = 24000;

        /// <summary>
        /// Получить яркость неба
        /// </summary>
        public float GetSkyLight(float timeIndex)
        {
            float angle = CalculateCelestialAngle(timeIndex);
            float f = glm.cos(angle * glm.pi360) * 2f + .5f;
            f = Mth.Clamp(f, 0, 1f);
            return f;
        }

        /// <summary>
        /// Яркость звёзд
        /// </summary>
        public float GetStarBrightness(float timeIndex)
        {
            float angle = CalculateCelestialAngle(timeIndex);
            float f = 1.0F - (glm.cos(angle * glm.pi360) * 2f + .25f);
            f = Mth.Clamp(f, 0, 1f);
            return f * f * .75f;
        }

        /// <summary>
        /// Цвет туманаn
        /// </summary>
        public vec3 GetFogColor(float timeIndex)
        {
            float f = GetSkyLight(timeIndex);
            return new vec3(.71f * f + .06f, .8f * f + .06f, .91f * f + .09f);
        }

        /// <summary>
        /// Вычисляет цвет для неба
        /// </summary>
        public vec3 GetSkyColor(float timeIndex)
        {
            float f = GetSkyLight(timeIndex);
            return new vec3(.5f * f, .7f * f, .99f * f);
        }

        /// <summary>
        /// Фаза луны
        /// </summary>
        public int GetMoonPhase()
        {
            uint totalWorldTime = GetTotalWorldTime();
            return (int)(totalWorldTime / SPEED_DAY % 8L + 8L) % 8;
        }

        /// <summary>
        /// Вычисляет угол солнца и луны в небе относительно заданного времени (0.0 - 1.0)
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        /// <returns></returns>
        public float CalculateCelestialAngle(float timeIndex)
        {
            uint totalWorldTime = GetTotalWorldTime();
            int time = (int)(totalWorldTime % SPEED_DAY);
            float timeFloat = (time + timeIndex) / (float)SPEED_DAY - .25f;

            if (timeFloat < 0f) timeFloat++;
            if (timeFloat > 1f) timeFloat--;

            float time2 = timeFloat;
            timeFloat = 1f - ((glm.cos(timeFloat * glm.pi) + 1f) / 2f);
            timeFloat = time2 + (timeFloat - time2) / 3.0F;
            return timeFloat;
        }

        /// <summary>
        /// Возвращает массив цветов восхода/заката
        /// </summary>
        /// <param name="timeIndex">коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1</param>
        public float[] CalcSunriseSunsetColors(float angle, float timeIndex)
        {
            float x = .4f;
            float y = glm.cos(angle * glm.pi360);
            float z = 0;

            if (y >= z - x && y <= z + x)
            {
                float f1 = (y - z) / x * .5f + .5f;
                float f2 = 1f - (1f - glm.sin(f1 * glm.pi)) * .99f;
                f2 *= f2;
                return new float[] {
                    f1 * .3f + .7f,
                    f1 * f1 * .7f + .2f,
                    f1 * f1 * 0 + .2f,
                    f2
                };
            }
            return new float[0];
        }
    }
}
