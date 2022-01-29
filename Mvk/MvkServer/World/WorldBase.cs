using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World.Block;
using MvkServer.World.Chunk;
using System.Collections;

namespace MvkServer.World
{
    /// <summary>
    /// Базовый объект мира
    /// </summary>
    public abstract class WorldBase
    {
        /// <summary>
        /// Посредник чанков
        /// </summary>
        public ChunkProvider ChunkPr { get; protected set; }
        /// <summary>
        /// Объект проверки коллизии
        /// </summary>
        public CollisionBase Collision { get; protected set; }
        


        protected WorldBase() => Collision = new CollisionBase(this);

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public virtual void Tick()
        {

        }

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public virtual string ToStringDebug() => "";

        /// <summary>
        /// Получить чанк по координатам чанка
        /// </summary>
        public ChunkBase GetChunk(vec2i pos) => ChunkPr.GetChunk(pos);

        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="bpos">глобальная позиция блока</param>
        public BlockBase GetBlock(BlockPos bpos) => GetBlock(bpos.Position);
        /// <summary>
        /// Получить блок
        /// </summary>
        /// <param name="pos">глобальная позиция блока</param>
        public BlockBase GetBlock(vec3i pos)
        {
            if (pos.y >= 0 && pos.y <= 255)
            {
                ChunkBase chunk = GetChunk(new vec2i(pos.x >> 4, pos.z >> 4));
                if (chunk != null)
                {
                    return chunk.GetBlock0(new vec3i(pos.x & 15, pos.y, pos.z & 15));
                }
            }
            return Blocks.GetAir(pos);
        }

        /// <summary>
        /// Пересечения лучей с визуализируемой поверхностью
        /// </summary>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public MovingObjectPosition RayCast(vec3 a, vec3 dir, float maxDist)
        {
            float px = a.x;
            float py = a.y;
            float pz = a.z;

            float dx = dir.x;
            float dy = dir.y;
            float dz = dir.z;

            float t = 0.0f;
            int ix = Mth.Floor(px);
            int iy = Mth.Floor(py);
            int iz = Mth.Floor(pz);

            int stepx = (dx > 0.0f) ? 1 : -1;
            int stepy = (dy > 0.0f) ? 1 : -1;
            int stepz = (dz > 0.0f) ? 1 : -1;

            float infinity = float.MaxValue;

            float txDelta = (dx == 0.0f) ? infinity : Mth.Abs(1.0f / dx);
            float tyDelta = (dy == 0.0f) ? infinity : Mth.Abs(1.0f / dy);
            float tzDelta = (dz == 0.0f) ? infinity : Mth.Abs(1.0f / dz);

            float xdist = (stepx > 0) ? (ix + 1 - px) : (px - ix);
            float ydist = (stepy > 0) ? (iy + 1 - py) : (py - iy);
            float zdist = (stepz > 0) ? (iz + 1 - pz) : (pz - iz);

            float txMax = (txDelta < infinity) ? txDelta * xdist : infinity;
            float tyMax = (tyDelta < infinity) ? tyDelta * ydist : infinity;
            float tzMax = (tzDelta < infinity) ? tzDelta * zdist : infinity;

            int steppedIndex = -1;

            while (t <= maxDist)
            {
                BlockBase block = GetBlock(new vec3i(ix, iy, iz));
                if (block.CollisionRayTrace(a, dir, maxDist))
                {
                    vec3 end;
                    vec3i norm;
                    vec3i iend;

                    end.x = px + t * dx;
                    end.y = py + t * dy;
                    end.z = pz + t * dz;

                    iend.x = ix;
                    iend.y = iy;
                    iend.z = iz;

                    norm.x = norm.y = norm.z = 0;
                    if (steppedIndex == 0) norm.x = -stepx;
                    if (steppedIndex == 1) norm.y = -stepy;
                    if (steppedIndex == 2) norm.z = -stepz;

                    //if (EntityDis != null)
                    //{
                    //    if (t < EntityDis.Distance)
                    //    {
                            return new MovingObjectPosition(block, iend, norm, end);
                    //    }
                    //    return new MovingObjectPosition(EntityDis.Entity);
                    //}
                }
                if (txMax < tyMax)
                {
                    if (txMax < tzMax)
                    {
                        ix += stepx;
                        t = txMax;
                        txMax += txDelta;
                        steppedIndex = 0;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
                else
                {
                    if (tyMax < tzMax)
                    {
                        iy += stepy;
                        t = tyMax;
                        tyMax += tyDelta;
                        steppedIndex = 1;
                    }
                    else
                    {
                        iz += stepz;
                        t = tzMax;
                        tzMax += tzDelta;
                        steppedIndex = 2;
                    }
                }
            }
            return new MovingObjectPosition();
        }
    }
}
