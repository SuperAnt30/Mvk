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

        protected EntityPlayer(WorldBase world) : base(world)
        {
            Type = EnumEntities.Player;
            StepHeight = 1.2f;
            Inventory = new InventoryPlayer(this);
            previousEquipment = new ItemStack[InventoryPlayer.COUNT_ARMOR + 1];

            // TODO::2022-03-29 Временно
            if (!world.IsRemote)
            {
                // TODO::доделать, этап при старте видеть игрока в инвентарём что в руке, и сам игрок загружать весь инвентарь
                Inventory.SetInventorySlotContents(2, new MvkServer.Item.ItemStack(Blocks.GetBlock(EnumBlock.Dirt)));
                Inventory.SetInventorySlotContents(3, new MvkServer.Item.ItemStack(Blocks.GetBlock(EnumBlock.Cobblestone), 16));
            }

        }

        /// <summary>
        /// Максимальное значение здоровья сущности
        /// </summary>
        protected override float GetHelathMax() => 20;

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

    }
}
