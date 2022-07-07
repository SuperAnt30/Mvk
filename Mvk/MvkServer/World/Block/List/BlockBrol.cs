using MvkServer.Sound;
using MvkServer.Util;

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
    }
}
