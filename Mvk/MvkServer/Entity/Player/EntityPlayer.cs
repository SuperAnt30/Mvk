using MvkServer.Glm;
using MvkServer.Util;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока
    /// </summary>
    public class EntityPlayer : EntityLiving
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Имя
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости
        /// </summary>
        public vec2i[] DistSqrt { get; protected set; }

        /// <summary>
        /// Задать id и имя игрока
        /// </summary>
        public void SetUUID(string name, string uuid)
        {
            Name = name;
            UUID = uuid;
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk, int plusDistSqrt)
        {
            OverviewChunk = overviewChunk;
            DistSqrt = ArrayStatic.GetSqrt(overviewChunk + plusDistSqrt);
        }
    }
}
