using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок брол, автор Вероника
    /// </summary>
    public class BlockBrol : BlockBase
    {
        /// <summary>
        /// Блок брол, автор Вероника
        /// </summary>
        public BlockBrol()
        {
            LightValue = 15;
            Particle = 8;
            АmbientOcclusion = false;
            Hardness = 5;
            Material = EnumMaterial.Brol;
            samplesBreak = new AssetsSample[] { AssetsSample.DigGlass1, AssetsSample.DigGlass2, AssetsSample.DigGlass3 };
            InitBoxs();
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        { 
            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 6),
                        new Face(Pole.Down, 7),
                        new Face(Pole.East, 8),
                        new Face(Pole.North, 8),
                        new Face(Pole.South, 8),
                        new Face(Pole.West, 8)
                    }
                }
            }};
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Random random)
        {
            //if (random.Next(10) == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    world.SpawnParticle(Entity.EnumParticle.Digging,
                        new vec3(blockPos.X + (float)random.NextDouble(), blockPos.Y + 1f, blockPos.Z + (float)random.NextDouble()),
                        new vec3(0),
                        (int)EBlock);
                }
              //  world.PlaySound(AssetsSample.DigGlass1, blockPos.ToVec3() + .5f, (float)random.NextDouble() * .25f + .75f, 1f);
            }
        }
    }
}
