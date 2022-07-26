using MvkServer.Glm;
using MvkServer.World;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект тестовый
    /// </summary>
    public class EntityTestFX : EntityFX
    {
        public EntityTestFX(WorldBase world, vec3 pos, vec3 motion) : base(world, pos, motion)
        {
            particleGravity = .02f;
            textureUV = new vec2i(0, 9);
            color = new vec3((float)rand.NextDouble(), (float)rand.NextDouble(), (float)rand.NextDouble());
            particleMaxAge *= 2;
        }

        public override void Update()
        {
            base.Update();

            if (rand.Next(4) == 0)
            {
                int u = textureUV.x;
                if (u < 7) u++;
                textureUV = new vec2i(u, textureUV.y);
            }
        }
    }
}
