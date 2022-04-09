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
            IsAir = true;
            IsAction = false;
            IsCollidable = collidable;
            IsParticle = false;
            IsFullCube = false;
            IsSpawn = false;
            CanPut = true;
            LightOpacity = 0;
        }
        /// <summary>
        /// Блок воздуха, пустота, не сталкивается
        /// </summary>
        public BlockAir() : this(false) { }
    }
}
