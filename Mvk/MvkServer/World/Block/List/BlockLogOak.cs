using MvkServer.Glm;
using MvkServer.Sound;
using MvkServer.Util;

namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок бревна дуба
    /// </summary>
    public class BlockLogOak : BlockBase
    {
        /// <summary>
        /// Блок дёрна дуба
        /// </summary>
        public BlockLogOak()
        {
            Particle = 9;
            Hardness = 60;
            Material = EnumMaterial.Dirt;
            samplesPut = samplesBreak = new AssetsSample[] { AssetsSample.DigWood1, AssetsSample.DigWood2, AssetsSample.DigWood3, AssetsSample.DigWood4 };
            samplesStep = new AssetsSample[] { AssetsSample.StepWood1, AssetsSample.StepWood2, AssetsSample.StepWood3, AssetsSample.StepWood4 };
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
            int met = 0;
            if (side == Pole.East || side == Pole.West) met = 1;
            else if (side == Pole.South || side == Pole.North) met = 2;

            return base.Put(worldIn, blockPos, new BlockState(state.Id(), met, state.lightBlock, state.lightSky), side, facing);
        }

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs()
        {
            vec3 colorBrown = new vec3(.62f, .44f, .37f);
            vec3 colorYelow = new vec3(.79f, .64f, .43f);

            boxes = new Box[][] {
                new Box[] {
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, 9, false, colorYelow),
                            new Face(Pole.Down, 9, false, colorYelow),
                            new Face(Pole.East, 10, false, colorBrown),
                            new Face(Pole.North, 10, false, colorBrown),
                            new Face(Pole.South, 10, false, colorBrown),
                            new Face(Pole.West, 10, false, colorBrown)
                        }
                    }
                },
                new Box[] {
                    new Box()
                    {
                        RotateYawUV = 1,
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, 10, false, colorBrown),
                            new Face(Pole.Down, 10, false, colorBrown),
                            new Face(Pole.East, 9, false, colorYelow),
                            new Face(Pole.North, 10, false, colorBrown),
                            new Face(Pole.South, 10, false, colorBrown),
                            new Face(Pole.West, 9, false, colorYelow)
                        }
                    }
                },
                new Box[] {
                    new Box()
                    {
                        RotateYawUV = 1,
                        Faces = new Face[]
                        {
                            new Face(Pole.East, 10, false, colorBrown),
                            new Face(Pole.North, 9, false, colorYelow),
                            new Face(Pole.South, 9, false, colorYelow),
                            new Face(Pole.West, 10, false, colorBrown)
                        }
                    },
                    new Box()
                    {
                        Faces = new Face[]
                        {
                            new Face(Pole.Up, 10, false, colorBrown),
                            new Face(Pole.Down, 10, false, colorBrown),
                        }
                    }
                }
            };
        }
    }
}
