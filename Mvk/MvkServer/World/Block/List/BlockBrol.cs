using MvkServer.Glm;
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
            Boxes = new Box[] {
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
            };

            LightValue = 15;
            Particle = 8;
            Hardness = 5;
            Material = EnumMaterial.Brol;
        }
    }
}
