using MvkServer.Glm;
using MvkServer.Inventory;
using MvkServer.Item;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока
    /// </summary>
    public abstract class EntityPlayer : EntityLivingHead
    {
        /// <summary>
        /// Уникальный id
        /// </summary>
        public string UUID { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Обзор чанков прошлого такта
        /// </summary>
        public int OverviewChunkPrev { get; protected set; } = 0;// MvkGlobal.OVERVIEW_CHUNK_START;
        /// <summary>
        /// Массив по длинам используя квадратный корень для всей видимости
        /// </summary>
        public vec2i[] DistSqrt { get; protected set; }
        /// <summary>
        /// В каком чанке было обработка чанков
        /// </summary>
        public vec2i ChunkPosManaged { get; protected set; } = new vec2i();
        /// <summary>
        /// Инвенарь игрока
        /// </summary>
        public InventoryPlayer Inventory { get; protected set; }
        /// <summary>
        /// Творческий режим
        /// </summary>
        public bool IsCreativeMode { get; protected set; } = false;

        /// <summary>
        /// Тикущий уровень игрока
        /// </summary>
        public int ExperienceLevel { get; protected set; } = 0;
        /// <summary>
        /// Общее количество опыта игрока. Это также включает в себя количество опыта в их полосе опыта.
        /// </summary>
        public int ExperienceTotal { get; protected set; } = 0;
        /// <summary>
        /// Текущее количество опыта, которое игрок имеет на своей полосе опыта.
        /// </summary>
        public float Experience { get; protected set; } = 0;

        protected EntityPlayer(WorldBase world) : base(world)
        {
            Type = EnumEntities.Player;
            StepHeight = 1.2f;
            Inventory = new InventoryPlayer(this);
            previousEquipment = new ItemStack[InventoryPlayer.COUNT_ARMOR + 1];

            // TODO::2022-03-29 Временно предметы при старте у игрока
            if (!world.IsRemote)
            {
                Inventory.SetInventorySlotContents(1, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.Dirt), 64));
                Inventory.SetInventorySlotContents(2, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.Water), 64));
                Inventory.SetInventorySlotContents(3, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.Cobblestone), 16));
                Inventory.SetInventorySlotContents(4, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.GlassRed), 64));
                Inventory.SetInventorySlotContents(5, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.Glass), 64));
                Inventory.SetInventorySlotContents(6, new MvkServer.Item.ItemStack(Blocks.GetBlockCache(EnumBlock.Brol), 64));
            }

        }

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 16;

        //public override void Update()
        //{
        //    base.Update();
        //}
        //protected override void LivingUpdate()
        //{
        //    base.LivingUpdate();
        //}

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public virtual void SetOverviewChunk(int overviewChunk) => OverviewChunk = overviewChunk;

        /// <summary>
        /// Равны ли обзоры чанков между тактами
        /// </summary>
        public bool SameOverviewChunkPrev() => OverviewChunk == OverviewChunkPrev;

        /// <summary>
        /// Обновить перспективу камеры
        /// </summary>
        public virtual void UpProjection() { }

        /// <summary>
        /// Задать чанк обработки
        /// </summary>
        public void SetChunkPosManaged(vec2i pos) => ChunkPosManaged = pos;

        /// <summary>
        /// Возвращает элемент, который держит в руке
        /// </summary>
        public override ItemStack GetHeldItem() => Inventory.GetCurrentItem();

        /// <summary>
        /// Получить стак что в правой руке 0 или броня 1-4 (InventoryPlayer.COUNT_ARMOR)
        /// </summary>
        public override ItemStack GetEquipmentInSlot(int slot) 
            => slot == 0 ? Inventory.GetCurrentItem() : Inventory.GetArmorInventory(slot - 1);

        /// <summary>
        /// Получить слот брони 0-3 InventoryPlayer.COUNT_ARMOR
        /// </summary>
        public override ItemStack GetCurrentArmor(int slot) => Inventory.GetArmorInventory(slot);

        /// <summary>
        /// Задать стак в слот, что в правой руке 0, или 1-4 слот брони InventoryPlayer.COUNT_ARMOR
        /// </summary>
        public override void SetCurrentItemOrArmor(int slot, ItemStack itemStack)
        {
            if (slot == 0)
            {
                Inventory.SetCurrentItem(itemStack);
            }
            else
            {
                Inventory.SetArmorInventory(slot - 1, itemStack);
            }
        }

        /// <summary>
        /// Добавить очки опыта игроку
        /// </summary>
        public void AddExperience(int experience)
        {
            //this.addScore(experience);
            int i = int.MaxValue - ExperienceTotal;
            if (experience > i) experience = i;

            Experience += (float)experience / (float)XpBarCap();

            for (ExperienceTotal += experience; Experience >= 1f; Experience /= (float)XpBarCap())
            {
                Experience = (Experience - 1.0F) * (float)XpBarCap();
                AddExperienceLevel(1);
            }
        }

        /// <summary>
        /// Использование уровня игрока
        /// </summary>
        public void UseExperienceLevel(int experienceLevel)
        {
            ExperienceLevel -= experienceLevel;

            if (ExperienceLevel < 0)
            {
                ExperienceLevel = 0;
                Experience = 0f;
                ExperienceTotal = 0;
            }
        }

        /// <summary>
        /// Добавить уровень игроку
        /// </summary>
        public void AddExperienceLevel(int experienceLevel)
        {
            ExperienceLevel += experienceLevel;

            if (ExperienceLevel < 0)
            {
                ExperienceLevel = 0;
                Experience = 0f;
                ExperienceTotal = 0;
            }

            //if (experienceLevel > 0 && ExperienceLevel % 5 == 0 && (float)this.field_82249_h < (float)this.ticksExisted - 100.0F)
            //{
            //    // Звуковой эффект каждый 5 уровень с интервалом 100 тактов
            //    float var2 = ExperienceLevel > 30 ? 1.0F : (float)ExperienceLevel / 30.0F;
            //    this.worldObj.playSoundAtEntity(this, "random.levelup", var2 * 0.75F, 1.0F);
            //    this.field_82249_h = this.ticksExisted;
            //}
        }

        /// <summary>
        /// Этот метод возвращает максимальное количество опыта, которое может содержать полоса опыта.
        /// С каждым уровнем предел опыта на шкале опыта игрока увеличивается на 10.
        /// </summary>
        public int XpBarCap() 
            => ExperienceLevel >= 30 ? 112 + (ExperienceLevel - 30) * 9 : (ExperienceLevel >= 15 ? 37 + (ExperienceLevel - 15) * 5 : 7 + ExperienceLevel * 2);
    }
}
