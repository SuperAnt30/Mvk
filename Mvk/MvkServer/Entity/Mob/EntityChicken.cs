
using MvkServer.Glm;
using MvkServer.Sound;
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
            samplesStep = new AssetsSample[] { AssetsSample.MobChickenStep1, AssetsSample.MobChickenStep2 };
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

        int iii = 0;
        int iii2 = 0;
        bool f = true;
        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update()
        {
            base.Update();
            PositionPrev = Position;
            if (!OnGround)
            {
                // При падении лапки двигаются в разы медленее
                LimbSwingAmount *= 0.25f;
            }

            if (World is WorldServer)
            {
                iii--;
                if (iii <= 0)
                {
                    SetRotationHead(RotationYawHead + glm.radians(rand.Next(180) - 90), RotationPitch);
                    iii = rand.Next(200) + 50;
                }
                iii2--;
                if (iii2 <= 0)
                {
                    Input = f ? Util.EnumInput.Forward : Util.EnumInput.Down;
                    f = !f;
                    iii2 = rand.Next(200) + 100;
                    if (f) iii2 += 200;
                }
                //InputAdd(Util.EnumInput.Down);
                //ChunkBase chunk = World.GetChunk(GetChunkPos());
                //if (chunk != null && chunk.CountEntity() > 1)
                //{
                //    Input = Util.EnumInput.Forward;
                //}
                //else
                //{
                //    Input = Util.EnumInput.None;
                //}
            }
        }

        protected override void LivingUpdate()
        {
            base.LivingUpdate();
            if (!OnGround && Motion.y < 0)
            {
                // При падение, курица махая крыльями падает медленее
                Motion = new vec3(Motion.x, Motion.y * .6f, Motion.z);
            }
        }
    }
}
