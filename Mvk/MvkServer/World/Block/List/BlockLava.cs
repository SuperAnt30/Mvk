using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей лавы
    /// </summary>
    public class BlockLava : BlockBase
    {
        /// <summary>
        /// Блок стоячей лавы
        /// </summary>
        public BlockLava()
        {
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            Translucent = true; 
            IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            Shadow = false;
            BackSide = true;
            AllSideForcibly = true;
            //UseNeighborBrightness = true; // Нельзя, будет глючная тень при AO
            IsReplaceable = true;
            Hardness = 2;
           // LightOpacity = 4;
            LightValue = 15;
            IsParticle = false;
            Material = EnumMaterial.Lava;
            samplesStep = new AssetsSample[0];
            //samplesBreak = new AssetsSample[] { AssetsSample.LiquidSplash1, AssetsSample.LiquidSplash2 };
            //samplesStep = new AssetsSample[] { AssetsSample.LiquidSwim1, AssetsSample.LiquidSwim2, AssetsSample.LiquidSwim3, AssetsSample.LiquidSwim4 };
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
           // vec3 color = new vec3(0.24f, 0.45f, 0.88f);

            boxes = new Box[][] { new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 61).SetAnimation(32, 4),
                        new Face(Pole.Down, 61).SetAnimation(32, 4),
                        new Face(Pole.East, 60).SetAnimation(64, 1),
                        new Face(Pole.North, 60).SetAnimation(64, 1),
                        new Face(Pole.South, 60).SetAnimation(64, 1),
                        new Face(Pole.West, 60).SetAnimation(64, 1)
                    }
                }
            }};
        }
    }
}
