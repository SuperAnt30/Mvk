using MvkServer.Util;

namespace MvkServer.World.Block.Items
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
            Boxes = new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 3, true),
                        new Face(Pole.Down, 2),
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
                        new Face(Pole.East, 4, true),
                        new Face(Pole.North, 4, true),
                        new Face(Pole.South, 4, true),
                        new Face(Pole.West, 4, true)
                    }
                }
            };

            IsGrass = true;
            
        }
    }
}
