using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект висюлек без движения
    /// </summary>
    public class EntitySuspendFX : EntityFX
    {
        public EntitySuspendFX(WorldBase world, vec3 pos, vec3 motion) : base(world, pos)
        {
            Motion = new vec3(0f);
            textureUV = new vec2i(0, 0);
            color = new vec3(.4f, .4f, .7f);
            SetSize(.01f, .01f);
            particleScale *= (float)rand.NextDouble() *.6f +.2f;
            particleMaxAge = (int)(16f / ((float)rand.NextDouble() * .8f + .2f));
        }

        public override void Update()
        {
            LastTickPos = PositionPrev = Position;

            // Проверка столкновения
            MoveEntity(Motion);

            if (World.GetBlockState(new BlockPos(Position)).GetBlock().Material != EnumMaterial.Water
                || particleAge++ >= particleMaxAge) SetDead();
        }
    }
}
