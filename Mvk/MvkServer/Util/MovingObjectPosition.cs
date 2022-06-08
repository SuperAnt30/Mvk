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
        public EntityBase Entity { get; private set; }
        /// <summary>
        /// Объект блока
        /// </summary>
        public BlockBase Block { get; private set; }
        /// <summary>
        /// Позиция блока
        /// </summary>
        public BlockPos BlockPosition { get; private set; }
        /// <summary>
        /// Координата по которому ударили
        /// </summary>
        public vec3i Hit { get; private set; }
        /// <summary>
        /// Координата на какой надо ставить
        /// </summary>
        public vec3i Put { get; private set; }
        /// <summary>
        /// Нормаль попадания
        /// </summary>
        public vec3i Norm { get; private set; }
        /// <summary>
        /// Координата куда попал луч
        /// </summary>
        public vec3 RayHit { get; private set; }
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
        public MovingObjectPosition(BlockBase block, BlockPos pos, vec3i hit, vec3i norm, vec3 rayHit)
        {
            Block = block;
            BlockPosition = pos;
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
        public MovingObjectPosition(EntityBase entity)
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
            string str = "";
            if (type == MovingObjectType.Entity)
            {
                float h = Entity is EntityLiving ? ((EntityLiving)Entity).Health : 0; 
                str = Entity.GetName() + " " + h + " " + Entity.Position;
            //} else if (type == MovingObjectType.Block)
            //{
            //    str = Block.W
            }
            return string.Format("{0} {3} {1} {2}", type, Hit, RayHit, str);
        }
    }
}
