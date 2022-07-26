using MvkServer.Glm;
using MvkServer.World;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект дыма
    /// </summary>
    public class EntitySmokeFX : EntityFX
    {
        public EntitySmokeFX(WorldBase world, vec3 pos, vec3 motion, int size) : base(world, pos)
        {
            Motion = Motion * .1f + motion;
            textureUV = new vec2i(7, 0);
            color = new vec3((float)rand.NextDouble() * .3f);
            float sf = size / 16f;
            particleScale *= .5f * sf;
            particleMaxAge = (int)(8f / ((float)rand.NextDouble() * .8f + .2f) * sf);
        }

        public override void Update()
        {
            LastTickPos = PositionPrev = Position;

            if (particleAge++ >= particleMaxAge) SetDead();
            textureUV = new vec2i(7 - particleAge * 8 / particleMaxAge, textureUV.y);

            vec3 motion = Motion;
            motion.y += .004f;

            // Проверка столкновения
            MoveEntity(motion);
            motion = Motion;

            if (Position.y == PositionPrev.y)
            {
                motion.x *= 1.1f;
                motion.z *= 1.1f;
            }

            motion *= .96f;

            if (OnGround)
            {
                motion.x *= .69999f;
                motion.z *= .69999f;
            }

            Motion = motion;
        }
    }
}
