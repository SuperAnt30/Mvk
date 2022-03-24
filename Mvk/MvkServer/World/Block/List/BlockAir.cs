namespace MvkServer.World.Block.List
{
    /// <summary>
    /// Блок воздуха, пустота
    /// </summary>
    public class BlockAir : BlockBase
    {
        /// <summary>
        /// Блок воздуха, пустотаб может сталкиваться
        /// </summary>
        /// <param name="collidable">Выбрать может ли блок сталкиваться</param>
        public BlockAir(bool collidable)
        {
            IsCollidable = collidable;
            IsParticle = false;
        }
        /// <summary>
        /// Блок воздуха, пустота, не сталкивается
        /// </summary>
        public BlockAir() : this(false) { }
    }
}
