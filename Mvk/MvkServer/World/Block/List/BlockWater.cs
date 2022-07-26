using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;
using System;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей воды
    /// </summary>
    public class BlockWater : BlockBase
    {
        /// <summary>
        /// Блок стоячей воды
        /// </summary>
        public BlockWater()
        {
            Translucent = true;
            IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            Shadow = false;
            BackSide = true;
            AllSideForcibly = true;
            IsReplaceable = true;
            UseNeighborBrightness = true;
            Hardness = 2;
            LightOpacity = 1;
            IsParticle = false;
            Material = EnumMaterial.Water;
            samplesBreak = new AssetsSample[] { AssetsSample.LiquidSplash1, AssetsSample.LiquidSplash2 };
            samplesStep = new AssetsSample[] { AssetsSample.LiquidSwim1, AssetsSample.LiquidSwim2, AssetsSample.LiquidSwim3, AssetsSample.LiquidSwim4 };
            InitBoxs();
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 63, true, color).SetAnimation(32, 2),
                        new Face(Pole.Down, 63, true, color).SetAnimation(32, 2),
                        new Face(Pole.East, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.North, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.South, 62, true, color).SetAnimation(64, 1),
                        new Face(Pole.West, 62, true, color).SetAnimation(64, 1)
                    }
                }
            }};
        }

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public override void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Random random)
        {
            //if (random.Next(64) == 0)
            //{
            //    world.PlaySound(AssetsSample.MobChickenPlop, blockPos.ToVec3() + .5f, (float)random.NextDouble() * .25f + .75f, (float)random.NextDouble() * 1.0f + .5f);
            //}
            //else 
            if (random.Next(10) == 0)
            {
                world.SpawnParticle(Entity.EnumParticle.Suspend,
                    new vec3(blockPos.X + (float)random.NextDouble(), blockPos.Y + (float)random.NextDouble(), blockPos.Z + (float)random.NextDouble()),
                    new vec3(0));
            }
        }
    }
}
