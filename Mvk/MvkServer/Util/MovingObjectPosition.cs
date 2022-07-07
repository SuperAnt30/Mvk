using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.World;
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
        /// Объект данных блока
        /// </summary>
        public BlockState Block { get; private set; }
        /// <summary>
        /// Позиция блока
        /// </summary>
        public BlockPos BlockPosition { get; private set; }
        /// <summary>
        /// Нормаль попадания по блоку
        /// </summary>
        public vec3i Norm { get; private set; }
        /// <summary>
        /// Координата куда попал луч в глобальных координатах по блоку
        /// </summary>
        public vec3 RayHit { get; private set; }
        /// <summary>
        /// Точка куда устанавливаем блок (параметр с RayCast)
        /// значение в пределах 0..1, образно фиксируем пиксел клика на стороне
        /// </summary>
        public vec3 Facing { get; private set; }
        /// <summary>
        /// Сторона блока куда смотрит луч
        /// </summary>
        public Pole Side { get; protected set; } = Pole.All;

        protected MovingObjectType type = MovingObjectType.None;

        /// <summary>
        /// Нет попадания
        /// </summary>
        public MovingObjectPosition() => Block = new BlockState().Empty();

        /// <summary>
        /// Попадаем в блок
        /// </summary>
        /// <param name="blockState">блок</param>
        /// <param name="pos">позиция блока</param>
        /// <param name="side">Сторона блока куда смотрит луч</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        /// <param name="norm">Нормаль попадания</param>
        /// <param name="rayHit">Координата куда попал луч</param>
        public MovingObjectPosition(BlockState blockState, BlockPos pos, Pole side, vec3 facing, vec3i norm, vec3 rayHit)
        {
            Block = blockState;
            BlockPosition = pos;
            Facing = facing;
            Side = side;
            RayHit = rayHit;
            Norm = norm;
            type = MovingObjectType.Block;
        }

        /// <summary>
        /// Попадаем в сущность
        /// </summary>
        /// <param name="entity">сущьность</param>
        public MovingObjectPosition(EntityBase entity)
        {
            Block = new BlockState().Empty();
            Entity = entity;
            type = MovingObjectType.Entity;
        }

        public bool IsBlock() => type == MovingObjectType.Block;

        public bool IsEntity() => type == MovingObjectType.Entity;

        /// <summary>
        /// Координата на какой надо ставить блок
        /// </summary>
        public vec3i GetPut(BlockBase blockNew)
        {
            BlockBase blockOld = Block.GetBlock();
            if (blockOld.IsReplaceable && blockOld.EBlock != blockNew.EBlock)
            { 
                return BlockPosition.ToVec3i();
            }
            return BlockPosition.ToVec3i() + Norm;
        }

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
            }
            return string.Format("{0} {3} {1} {2}", type, Side, Facing, str);
        }
    }
}
