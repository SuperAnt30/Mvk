using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок длинной травы
    /// </summary>
    public class BlockTallGrass : BlockBase
    {
        private readonly vec3[] offsetMet;

        /// <summary>
        /// Блок длинной травы
        /// </summary>
        public BlockTallGrass()
        {
            Particle = 11;
            FullBlock = false;
            Color = new vec3(.56f, .73f, .35f);
            RenderType = EnumRenderType.AllSideForcibly | EnumRenderType.NoSideDimming | EnumRenderType.BlocksNotSame;
            IsReplaceable = true;
            LightOpacity = 0;
            IsCollidable = false;
            UseNeighborBrightness = true;
            Material = EnumMaterial.Grass;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            offsetMet = new vec3[]
            {
                new vec3(0),
                new vec3(.125f, 0, .125f),
                new vec3(-.125f, 0, .125f),
                new vec3(.125f, 0, -.125f),
                new vec3(-.125f, 0, -.125f)
            };
            InitBoxs();
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        public override void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune) { }

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            BlockState blockStateDown = worldIn.GetBlockState(blockPos.OffsetDown());
            if (blockStateDown.GetBlock().Material == EnumMaterial.Dirt) 
                return base.Put(worldIn, blockPos, state, side, facing);
            return false;
        }

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public override AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            vec3 min = new vec3(pos.X + .125f, pos.Y, pos.Z + .125f);
            vec3 max = new vec3(pos.X + .875f, pos.Y + .875f, pos.Z + .875f);
            return new AxisAlignedBB[] { new AxisAlignedBB(min, max).Offset(offsetMet[met]) };
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met) => boxes[met];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            boxes = new Box[5][];
            for (int i = 0; i < 5; i++) {
                boxes[i] =
                    new Box[] {
                    new Box()
                    {
                        From = new vec3(0, 0, .5f),
                        To = new vec3(1f, 1f, .5f),
                        RotateYaw = glm.pi45,
                        Faces = new Face[]
                        {
                            new Face(Pole.North, 11, true, Color),
                            new Face(Pole.South, 11, true, Color),
                        }
                    },
                    new Box()
                    {
                        From = new vec3(.5f, 0, 0),
                        To = new vec3(.5f, 1f, 1f),
                        RotateYaw = glm.pi45,
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 11, true, Color),
                            new Face(Pole.West, 11, true, Color)
                        }
                    }
                };
                boxes[i][0].Translate = offsetMet[i];
                boxes[i][1].Translate = offsetMet[i];
            }
        }
    }
}
