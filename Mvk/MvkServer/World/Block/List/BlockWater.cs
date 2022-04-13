using MvkServer.Glm;
using MvkServer.Util;

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
            vec3 color = new vec3(0.24f, 0.45f, 0.88f);
            //Boxes = new Box[] { new Box(63, true, new vec3(0.24f, 0.45f, 0.88f)) };
            Boxes = new Box[] {
                new Box()
                {
                    Faces = new Face[]
                    {
                        new Face(Pole.Up, 63, true, color),
                        new Face(Pole.Down, 63, true, color),
                        new Face(Pole.East, 62, true, color),
                        new Face(Pole.North, 62, true, color),
                        new Face(Pole.South, 62, true, color),
                        new Face(Pole.West, 62, true, color)
                    }
                }
            };

            IsAlphe = true;
            IsAction = false;
            //IsWater = true;
            IsCollidable = false;
            IsFullCube = false;
            CanPut = true;
            IsSpawn = false;
            Particle = 63;
            Hardness = 2;
            LightOpacity = 2;
            AnimationFrame = 32;
            AnimationPause = 2;
            IsParticle = false;
        }
    }
}
