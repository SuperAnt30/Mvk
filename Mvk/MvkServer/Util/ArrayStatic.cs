
using MvkServer.Glm;

namespace MvkServer.Util
{
    /// <summary>
    /// Статические переменные из массивов
    /// </summary>
    public class ArrayStatic
    {
        /// <summary>
        /// Область в один блок без центра, 8 блоков
        /// </summary>
        public static vec2i[] areaOne8 = new vec2i[] {
            new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
            new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
        };

        /// <summary>
        /// Область в один блок c центром, 9 блоков
        /// </summary>
        public static vec2i[] areaOne9 = new vec2i[] { new vec2i(0, 0),
            new vec2i(0, 1), new vec2i(1, 1), new vec2i(1, 0), new vec2i(1, -1),
            new vec2i(0, -1), new vec2i(-1, -1), new vec2i(-1, 0), new vec2i(-1, 1)
        };
    }
}
