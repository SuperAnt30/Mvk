using MvkServer.Glm;
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
            vec3 colorGreen = new vec3(.56f, .73f, .35f);
            vec3 colorBrown = new vec3(.62f, .44f, .37f);
            Boxes = new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 3, true, colorGreen),
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
            
            IsGrass = true;
        }

        /// <summary>
        /// Значение для разрушения в тактах
        /// </summary>
        public override int GetDamageValue() => 10;
    }
}
