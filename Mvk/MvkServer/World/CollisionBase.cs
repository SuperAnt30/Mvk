using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
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
        /// Получить блок в глобальной координате
        /// </summary>
        protected BlockState GetBlockState(int x, int y, int z)
        {
            if (y >= 0 && y <= 255)
            {
                ChunkBase chunk = World.GetChunk(new vec2i(x >> 4, z >> 4));
                if (chunk != null)
                {
                    BlockState blockState = chunk.GetBlockState(x & 15, y, z & 15);
                    // делаем без колизии если чанк загружен, чтоб можно было в пустых псевдо чанках двигаться
                    if (blockState.IsEmpty()) return new BlockState();
                    return chunk.GetBlockState(x & 15, y, z & 15);
                }
            }
            // Для колизи важно, если чанк не загружен, то блоки все с колизией, так-как начнём падать
            return new BlockState().Empty();
        }

        /// <summary>
        /// Возвращает список ограничивающих рамок, которые сталкиваются с aabb,
        /// /*за исключением переданного столкновения сущности.*/
        /// </summary>
        /// <param name="aabb">проверяемая рамка</param>
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
                        if (y >= 0 && y <= 255)
                        {
                            BlockState blockState = GetBlockState(x, y, z);
                            BlockBase block = blockState.IsEmpty() ? Blocks.GetNone() : blockState.GetBlock(); 
                            if (block.IsCollidable)
                            {
                                list.AddRange(block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met()));
                            }
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// Обработка колизии блока, особенно важен когда блок не цельный
        /// </summary>
        /// <param name="xyz">координата блока</param>
        /// <param name="aabb">проверяемая рамка</param>
        /// <returns>true - пересечение имеется</returns>
        protected bool BlockCollision(int x, int y, int z, AxisAlignedBB aabb)
        {
            BlockState blockState = GetBlockState(x, y, z);
            BlockBase block = blockState.IsEmpty() ? Blocks.GetNone() : blockState.GetBlock();
            if (block.IsCollidable)
            {
                // Цельный блок на коллизию
                if (block.FullBlock) return true;
                // Выбираем часть блока
                foreach(AxisAlignedBB aabbBlock in block.GetCollisionBoxesToList(new BlockPos(x, y, z), blockState.Met()))
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
            AxisAlignedBB aabb = entity.GetBoundingBox(pos).Expand(new vec3(-0.01f));
            vec3i min = aabb.MinInt();
            vec3i max = aabb.MaxInt();

            for (int y = min.y; y <= max.y; y++)
            {
                for (int x = min.x; x <= max.x; x++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        if (BlockCollision(x, y, z, aabb)) return true;
                    }
                }
            }
            return false;
        }
    }
}
