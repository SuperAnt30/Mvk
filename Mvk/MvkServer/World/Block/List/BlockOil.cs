using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок стоячей нефти
    /// </summary>
    public class BlockOil : BlockBase
    {
        /// <summary>
        /// Блок стоячей нефти
        /// </summary>
        public BlockOil()
        {
            // Затычка, для сортировки, и прорисовки из нутри когда к примеру блок стекла
            Translucent = true; 
            IsAction = false;
            IsCollidable = false;
            АmbientOcclusion = false;
            Shadow = false;
            BackSide = true;
            AllSideForcibly = true;
            IsReplaceable = true;
            Hardness = 2;
            IsParticle = false;
            Material = EnumMaterial.Oil;
            samplesStep = new AssetsSample[0];
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
                        new Face(Pole.Up, 59).SetAnimation(32, 8),
                        new Face(Pole.Down, 59).SetAnimation(32, 8),
                        new Face(Pole.East, 58).SetAnimation(64, 4),
                        new Face(Pole.North, 58).SetAnimation(64, 4),
                        new Face(Pole.South, 58).SetAnimation(64, 4),
                        new Face(Pole.West, 58).SetAnimation(64, 4)
                    }
                }
            }};
        }
    }
}
