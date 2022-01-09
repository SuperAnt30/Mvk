namespace MvkServer.World
{
    public enum EnumCollisionBody
    {
        /// <summary>
        /// Нет коллизии
        /// </summary>
        None = 0,
        /// <summary>
        /// Коллизия
        /// </summary>
        Collision = 1,
        /// <summary>
        /// Коллизия с низу, когда падали вниз, прекращаем падать
        /// </summary>
        CollisionDown = 2
    }
}
