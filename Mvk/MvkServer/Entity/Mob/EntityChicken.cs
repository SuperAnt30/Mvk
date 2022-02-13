
namespace MvkServer.Entity.Mob
{
    /// <summary>
    /// Сущность курицы
    /// </summary>
    public class EntityChicken : EntityLivingHead
    {

        public EntityChicken()
        {
            Standing();
            SpeedSurvival();
            Type = EnumEntities.Chicken;
            StepHeight = 1.01f;
        }
    }
}
