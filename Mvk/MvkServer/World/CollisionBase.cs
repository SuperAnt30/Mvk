using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using System.Collections.Generic;

namespace MvkServer.World
{
    /// <summary>
    /// Базовый класс проверки колизии
    /// </summary>
    public class CollisionBase
    {
        /// <summary>
        /// Сылка на объект мира
        /// </summary>
        public WorldBase World { get; protected set; }

        public CollisionBase(WorldBase world) => World = world;

        /// <summary>
        /// Возвращает список ограничивающих рамок, которые сталкиваются с aabb,
        /// /*за исключением переданного столкновения сущности.*/
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="aabb"></param>
        /// <returns></returns>
        public List<AxisAlignedBB> GetCollidingBoundingBoxes(AxisAlignedBB aabb)
        {
            List<AxisAlignedBB> list = new List<AxisAlignedBB>();
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        BlockBase block = World.GetBlock(new vec3i(x, y, z));
                        if (block.IsCollision)
                        {
                            list.AddRange(block.GetCollisionBoxesToList());
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Обработка колизии блока, особенно важен когда блок не цельный
        /// </summary>
        /// <param name="blockPos">координата блока</param>
        /// <param name="aabb">проверяемая рамка</param>
        /// <returns>true - пересечение имеется</returns>
        protected bool BlockCollision(vec3i blockPos, AxisAlignedBB aabb)
        {
            BlockBase block = World.GetBlock(blockPos);
            if (block.IsCollision)
            {
                // Цельный блок на коллизию
                if (block.IsBoundingBoxAll) return true;
                // Выбираем часть блока
                foreach(AxisAlignedBB aabbBlock in block.GetCollisionBoxesToList())
                {
                    if (aabbBlock.IntersectsWith(aabb)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяем коллизию тела c блоками
        /// </summary>
        /// <param name="entity">Сущность проверки</param>
        /// <param name="pos">позиция</param>
        public bool IsCollisionBody(EntityBase entity, vec3 pos)
        {
            AxisAlignedBB aabb = entity.GetBoundingBox(pos);
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        if (BlockCollision(new vec3i(x, y, z), aabb)) return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяем коллизию только нижнего ряда
        /// </summary>
        /// <param name="entity">Сущность проверки</param>
        /// <param name="pos">позиция</param>
        public bool IsCollisionDown(EntityBase entity, vec3 pos)
        {
            AxisAlignedBB aabb = entity.GetBoundingBox(pos);
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();
            int y = min.y;

            for (int x = min.x; x <= max.x; x++)
            {
                for (int z = min.z; z <= max.z; z++)
                {
                    if (BlockCollision(new vec3i(x, y, z), aabb)) return true;
                }
            }
            return false;
        }
    }
}
