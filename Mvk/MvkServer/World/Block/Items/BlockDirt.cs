using MvkServer.Glm;

namespace MvkServer.World.Block.Items
{
    /// <summary>
    /// Блок земли
    /// </summary>
    public class BlockDirt : BlockBase
    {
        /// <summary>
        /// Блок земли
        /// </summary>
        public BlockDirt()
        {
            Boxes = new Box[] { new Box(2, false, new vec3(.62f, .44f, .37f)) };
        }

        /// <summary>
        /// Значение для разрушения в тактах
        /// </summary>
        public override int GetDamageValue() => 5;
    }
}
