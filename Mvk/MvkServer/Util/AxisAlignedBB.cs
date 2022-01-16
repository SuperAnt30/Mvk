using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Выровненные по осям ограничивающие рамки
    /// Axis-aligned bounding boxes
    /// </summary>
    public class AxisAlignedBB
    {
        public vec3 Min { get; protected set; }
        public vec3 Max { get; protected set; }

        public AxisAlignedBB(vec3 from, vec3 to)
        {
            Min = new vec3(Mth.Min(from.x, to.x), Mth.Min(from.y, to.y), Mth.Min(from.z, to.z));
            Max = new vec3(Mth.Max(from.x, to.x), Mth.Max(from.y, to.y), Mth.Max(from.z, to.z));
        }

        public vec3i MinInt() => new vec3i(Min);
        public vec3i MaxInt() => new vec3i(Max);

        public AxisAlignedBB Clone() => new AxisAlignedBB(Min, Max);
        /// <summary>
        /// Смещает текущую ограничивающую рамку на указанные координаты
        /// </summary>
        public AxisAlignedBB Offset(vec3 bias) => new AxisAlignedBB(Min + bias, Max + bias);

        /// <summary>
        /// Добавить координату в область
        /// </summary>
        public AxisAlignedBB AddCoord(vec3 pos)
        {
            vec3 min = new vec3(Min);
            vec3 max = new vec3(Max);

            if (pos.x < 0f) min.x += pos.x;
            else if (pos.x > 0f) max.x += pos.x;

            if (pos.y < 0f) min.y += pos.y;
            else if (pos.y > 0f) max.y += pos.y;

            if (pos.z < 0f) min.z += pos.z;
            else if (pos.z > 0f) max.z += pos.z;

            return new AxisAlignedBB(min, max);
        }

        /// <summary>
        /// Возвращает ограничивающую рамку, расширенную указанным вектором
        /// </summary>
        public AxisAlignedBB Expand(vec3 vec) => new AxisAlignedBB(Min - vec, Max + vec);

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и Z, 
        /// вычислите смещение между ними в измерении X. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateXOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.y > Min.y && other.Min.y < Max.y && other.Max.z > Min.z && other.Min.z < Max.z)
            {
                if (offset > 0f && other.Max.x <= Min.x)
                {
                    float bias = Min.x - other.Max.x;
                    if (bias < offset) offset = bias;
                }
                else if (offset < 0f && other.Min.x >= Max.x)
                {
                    float bias = Max.x - other.Min.x;
                    if (bias > offset) offset = bias;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях X и Z, 
        /// вычислите смещение между ними в измерении Y. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateYOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.x > Min.x && other.Min.x < Max.x && other.Max.z > Min.z && other.Min.z < Max.z)
            {
                if (offset > 0f && other.Max.y <= Min.y)
                {
                    float bias = Min.y - other.Max.y;
                    if (bias < offset) offset = bias;
                }
                else if (offset < 0f && other.Min.y >= Max.y)
                {
                    float bias = Max.y - other.Min.y;
                    if (bias > offset) offset = bias;
                }
            }
            return offset;
        }

        /// <summary>
        /// Если экземпляр и ограничивающие рамки аргумента перекрываются в измерениях Y и X, 
        /// вычислите смещение между ними в измерении Z. вернуть offset, если ограничивающие 
        /// рамки не перекрываются или если offset ближе к 0, чем вычисленное смещение. 
        /// В противном случае вернуть рассчитанное смещение.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="offset">смещение</param>
        /// <returns></returns>
        public float CalculateZOffset(AxisAlignedBB other, float offset)
        {
            if (other.Max.x > Min.x && other.Min.x < Max.x && other.Max.y > Min.y && other.Min.y < Max.y)
            {
                if (offset > 0f && other.Max.z <= Min.z)
                {
                    float bias = Min.z - other.Max.z;
                    if (bias < offset) offset = bias;
                }
                else if (offset < 0f && other.Min.z >= Max.z)
                {
                    float bias = Max.z - other.Min.z;
                    if (bias > offset) offset = bias;
                }
            }
            return offset;
        }

        /// <summary>
        /// Возвращает, пересекается ли данная ограничивающая рамка с этой
        /// </summary>
        public bool IntersectsWith(AxisAlignedBB other) => other.Max.x > Min.x && other.Min.x < Max.x 
                ? (other.Max.y > Min.y && other.Min.y < Max.y ? other.Max.z > Min.z && other.Min.z < Max.z : false) 
                : false;

        /// <summary>
        /// Возвращает, если предоставленный vec3 полностью находится внутри ограничивающей рамки.
        /// </summary>
        public bool IsVecInside(vec3 vec) => vec.x > Min.x && vec.x < Max.x
                ? (vec.y > Min.y && vec.y < Max.y ? vec.z > Min.z && vec.z < Max.z : false)
                : false;

        /// <summary>
        /// Возвращает среднюю длину краев ограничивающей рамки.
        /// </summary>
        public float GetAverageEdgeLength()
        {
            float x = Max.x - Min.x;
            float y = Max.y - Min.y;
            float z = Max.z - Min.z;
            return (x + y + z) / 3f;
        }

        public override string ToString()
        {
            return "box[" + Min.ToString() + " -> " + Max.ToString() + "]";
        }
    }
}
