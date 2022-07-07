using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок дёрна
    /// </summary>
    public class BlockTurf : BlockBase
    {
        /// <summary>
        /// Блок дёрна
        /// </summary>
        public BlockTurf()
        {
            Particle = 3;
            Color = new vec3(.56f, .73f, .35f);
            //Slipperiness = 0.8f;
            //IsGrass = true;
            Hardness = 10;
            Material = EnumMaterial.Turf;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigGrass1, AssetsSample.DigGrass2, AssetsSample.DigGrass3, AssetsSample.DigGrass4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepGrass1, AssetsSample.StepGrass2, AssetsSample.StepGrass3, AssetsSample.StepGrass4 };
            InitBoxs();
        }

        /// <summary>
        /// Коробки
        /// </summary>
        public override Box[] GetBoxes(int met) => boxes[met];

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public override bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing)
        {
            int sideInt = (int)side - 2;
            if (sideInt < 0) sideInt = 0;

            return base.Put(worldIn, blockPos, new BlockState(state.Id(), sideInt, state.light), side, facing);
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 colorGreen = Color;
            vec3 colorBrown = new vec3(.62f, .44f, .37f);

            boxes = new Box[4][];
            for (int i = 0; i < 4; i++)
            {
                boxes[i] = new Box[] {
                    new Box()
                    {
                        Faces = new Face[] { new Face(Pole.Up, 3, true, colorGreen) }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Down, 2, colorBrown),
                            new Face(Pole.East, 2),
                            new Face(Pole.North, 2),
                            new Face(Pole.South, 2),
                            new Face(Pole.West, 2)
                        }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 4, true, colorGreen),
                            new Face(Pole.North, 4, true, colorGreen),
                            new Face(Pole.South, 4, true, colorGreen),
                            new Face(Pole.West, 4, true, colorGreen)
                        }
                    }
                };
            }
            boxes[1][0].RotateYawUV = 1;
            boxes[2][0].RotateYawUV = 2;
            boxes[3][0].RotateYawUV = 3;
        }
    }
}
