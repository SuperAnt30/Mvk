
using MvkServer.Glm;
using MvkServer.World;
using MvkServer.World.Chunk;

namespace MvkServer.Entity.Mob
{
    /// <summary>
    /// Сущность курицы
    /// </summary>
    public class EntityChicken : EntityLivingHead
    {
        public EntityChicken(WorldBase world) : base(world)
        {
            Type = EnumEntities.Chicken;
            StepHeight = 1.01f;
        }

        /// <summary>
        /// Положение стоя
        /// </summary>
        protected override void Standing() => SetSize(.3f, .99f);

        /// <summary>
        /// Положение сидя
        /// </summary>
        protected override void Sitting() => SetSize(.3f, .7f);

        /// <summary>
        /// Скорость для режима выживания
        /// </summary>
        protected override void SpeedSurvival() => Speed = new EntitySpeed(.05f);

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 5;

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();

            if (World is WorldServer)
            {
                ChunkBase chunk = World.GetChunk(GetChunkPos());
                if (chunk != null && chunk.CountEntity() > 1)
                {
                    Input = Util.EnumInput.Forward;
                }
                else
                {
                    Input = Util.EnumInput.None;
                }
            }
        }
    }
}
