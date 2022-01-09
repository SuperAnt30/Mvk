using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.World.Block;

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
        /// Обработка колизии блока, особенно важен когда блок не цельный
        /// </summary>
        /// <param name="blockPos">координата блока</param>
        /// <param name="hbs">объект хитбокса сущьности</param>
        /// <returns>true - пересечение имеется</returns>
        protected bool BlockCollision(vec3i blockPos, HitBoxSizeUD hbs)
        {
            BlockBase block = World.GetBlock(blockPos);
            if (block.IsCollision)
            {
                // Цельный блок на коллизию
                if (block.HitBox.IsHitBoxAll) return true;
                // Выбираем часть блока
                vec3 bpos = new vec3(blockPos);
                vec3 vf = block.HitBox.From + bpos;
                vec3 vt = block.HitBox.To + bpos;
                if (vf.x > hbs.GetVu().x || vt.x < hbs.GetVd().x
                    || vf.y > hbs.GetVu().y || vt.y < hbs.GetVd().y
                    || vf.z > hbs.GetVu().z || vt.z < hbs.GetVd().z)
                {
                    // пересечения нет
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Проверяем коллизию тела c блоками
        /// </summary>
        /// <param name="entity">Сущность проверки</param>
        /// <param name="bias">смещение позиции</param>
        public EnumCollisionBody IsCollisionBody(EntityBase entity, vec3 bias)
        {
            vec3 pos = entity.Position + bias;

            HitBoxSizeUD hbs = new HitBoxSizeUD(pos, entity.Hitbox);
            vec3i vd = hbs.GetVdi();
            vec3i vu = hbs.GetVui();

            for (int y = vd.y; y <= vu.y; y++)
            {
                for (int x = vd.x; x <= vu.x; x++)
                {
                    for (int z = vd.z; z <= vu.z; z++)
                    {
                        if (BlockCollision(new vec3i(x, y, z), hbs))
                        {
                            return bias.y < 0 ? EnumCollisionBody.CollisionDown : EnumCollisionBody.Collision;
                        }
                    }
                }
            }
            return EnumCollisionBody.None;
        }

        /// <summary>
        /// Проверяем коллизию под ногами
        /// </summary>
        /// <param name="entity">Сущность проверки</param>
        /// <param name="pos">позиция</param>
        public bool IsCollisionDown(EntityBase entity, vec3 pos)
        {
            HitBoxSizeUD hbs = new HitBoxSizeUD(pos, entity.Hitbox);
            vec3 vd = hbs.GetVd();
            vec3 vu = hbs.GetVu();
            vd.y -= .01f;
            vec3i d = new vec3i(vd);
            vec3i d2 = new vec3i(vu);

            for (int x = d.x; x <= d2.x; x++)
            {
                for (int z = d.z; z <= d2.z; z++)
                {
                    if (BlockCollision(new vec3i(x, d.y, z), hbs)) return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Проверяем колизию блоков над головой
        /// </summary>
        protected bool IsCollisionUp(EntityBase entity, vec3 pos)
        {
            HitBoxSizeUD hbs = new HitBoxSizeUD(pos, entity.Hitbox);
            vec3 vd = hbs.GetVd();
            vec3 vu = hbs.GetVu();
            vu.y += entity.Hitbox.GetUpEyes();
            vd.y = vu.y;
            vec3i d = new vec3i(vd);
            vec3i d2 = new vec3i(vu);

            for (int x = d.x; x <= d2.x; x++)
            {
                for (int z = d.z; z <= d2.z; z++)
                {
                    if (BlockCollision(new vec3i(x, d.y, z), hbs)) return true;
                }
            }
            return false;
        }
    }
}
