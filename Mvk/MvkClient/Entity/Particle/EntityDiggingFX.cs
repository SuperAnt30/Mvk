using MvkAssets;
using MvkClient.Renderer;
using MvkServer.Glm;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkClient.Entity.Particle
{
    /// <summary>
    /// Эффект копать
    /// </summary>
    public class EntityDiggingFX : EntityFX
    {
        /// <summary>
        /// Координаты текстуры доп смещения микро частички
        /// </summary>
        private vec2i textureJitterUV = new vec2i(0);

        private EnumBlock enumBlock;

        public EntityDiggingFX(WorldBase world, vec3 pos, vec3 motion, EnumBlock eBlock) : base(world, pos, motion)
        {
            enumBlock = eBlock;
            particleGravity = 1f;
            particleScale /= 2f;
            color = new vec4(.8f, .8f, .8f, 1f);

            BlockBase block = Blocks.GetBlockCache(eBlock);

            // пробуем цвет травы подкрасить
            color *= new vec4(block.Color, 1f);

            int u = block.Particle % 64;
            int v = block.Particle / 64;

            textureUV = new vec2i(u, v); // В зависимости от блока надо будет вытянуть положение
            Texture = AssetsTexture.Atlas;
            // (0..12) в пикселах, и в смещении (0..0.01171875) (12 / (16 * 64))
            textureJitterUV = new vec2i(rand.Next(13), rand.Next(13));
        }

        /// <summary>
        /// Рендер прямоугольника частицы
        /// </summary>
        public override void RenderRectangle(float timeIndex)
        {
            float u1 = textureUV.x / 64f + textureJitterUV.x / 1024f;
            float v1 = textureUV.y / 64f + textureJitterUV.y / 1024f;
            float u2 = u1 + .00390625f; // 4-ая часть блока
            float v2 = v1 + .00390625f; // 4-ая часть блока
            float scale = particleScale * .1f;

            Render(scale, u1, v1, u2, v2);
        }
    }
}
