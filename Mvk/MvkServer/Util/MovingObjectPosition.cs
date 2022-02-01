using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.World.Block;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект отвечающий какой объект попадает под луч
    /// </summary>
    public class MovingObjectPosition
    {
        /// <summary>
        /// Объект сущьности
        /// </summary>
        public EntityLiving Entity { get; protected set; }
        /// <summary>
        /// Объект блока
        /// </summary>
        public BlockBase Block { get; protected set; }
        /// <summary>
        /// Координата по которому ударили
        /// </summary>
        public vec3i Hit { get; protected set; }
        /// <summary>
        /// Координата на какой надо ставить
        /// </summary>
        public vec3i Put { get; protected set; }
        /// <summary>
        /// Нормаль попадания
        /// </summary>
        public vec3i Norm { get; protected set; }
        /// <summary>
        /// Координата куда попал луч
        /// </summary>
        public vec3 RayHit { get; protected set; }
        /// <summary>
        /// Сторона куда смотрит луч
        /// </summary>
        //public Pole Side { get; protected set; } = Pole.All;

        protected MovingObjectType type = MovingObjectType.None;

        /// <summary>
        /// Нет попадания
        /// </summary>
        public MovingObjectPosition() { }

        /// <summary>
        /// Попадаем в блок
        /// </summary>
        /// <param name="block">блок</param>
        /// <param name="hit">Координата по которому ударили</param>
        /// <param name="norm">Нормаль попадания</param>
        /// <param name="rayHit">Координата куда попал луч</param>
        public MovingObjectPosition(BlockBase block, vec3i hit, vec3i norm, vec3 rayHit)
        {
            Block = block;
            RayHit = rayHit;
            Hit = hit;
            Put = hit + norm;
            Norm = norm;
            type = MovingObjectType.Block;
        }

        /// <summary>
        /// Попадаем в сущность
        /// </summary>
        /// <param name="entity">сущьность</param>
        /// <param name="hit">Координата по которому ударили</param>
        public MovingObjectPosition(EntityLiving entity)
        {
            Entity = entity;
            type = MovingObjectType.Entity;
        }

        public bool IsBlock() => type == MovingObjectType.Block;

        public bool IsEntity() => type == MovingObjectType.Entity;

        /// <summary>
        /// Тип объекта
        /// </summary>
        protected enum MovingObjectType
        {
            None = 0,
            Block = 1,
            Entity = 2
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", type, Hit, RayHit);
        }
    }
}
