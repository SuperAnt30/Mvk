using MvkServer.Entity.Item;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Sound;
using MvkServer.Util;
using System;

namespace MvkServer.World.Block
{
    /// <summary>
    /// Базовый объект Блока
    /// </summary>
    public abstract class BlockBase
    {
        /// <summary>
        /// Ограничительная рамка занимает весь блок, для оптимизации, без проверки AABB блока
        /// </summary>
        public bool FullBlock { get; protected set; } = true;
        /// <summary>
        /// Сколько света вычитается для прохождения этого блока Air = 0
        /// В VoxelEngine он в public static byte GetBlockLightOpacity(EnumBlock eblock)
        /// получть инфу не создавая блок
        /// </summary>
        public byte LightOpacity { get; protected set; } = 15;
        /// <summary>
        /// Количество излучаемого света (плафон)
        /// </summary>
        public int LightValue { get; protected set; } = 0;
        /// <summary>
        /// Полупрозрачный, альфа блок, вода, стекло...
        /// </summary>
        public bool Translucent { get; protected set; } = false;

        /// <summary>
        /// Флаг, если блок должен использовать самое яркое значение соседнего света как свое собственное
        /// Пример: трава, вода, стекло, но только не лава и нефть
        /// VE LightingYourself
        /// </summary>
        public bool UseNeighborBrightness { get; protected set; } = false;
        /// <summary>
        /// Все стороны принудительно, пример: трава, стекло, вода, лава
        /// </summary>
        public bool AllSideForcibly { get; protected set; } = false;
        /// <summary>
        /// Сторону рисуем с двух сторон, пример: вода, лава
        /// </summary>
        public bool BackSide { get; protected set; } = false;
        /// <summary>
        /// Обрабатывается блок эффектом АmbientOcclusion
        /// </summary>
        public bool АmbientOcclusion { get; protected set; } = true;
        /// <summary>
        /// Нет бокового затемнения, пример: трава, цветы
        /// </summary>
        public bool NoSideDimming { get; protected set; } = false;
        /// <summary>
        /// Может ли быть тень сущности на блоке, только для целых блоков
        /// </summary>
        public bool Shadow { get; protected set; } = true;
        /// <summary>
        /// Не однотипные блоки, пример: трава, цветы, кактус
        /// </summary>
        public bool BlocksNotSame { get; protected set; } = false;
        /// <summary>
        /// Сколько ударов требуется, чтобы сломать блок в тактах (20 тактов = 1 секунда)
        /// </summary>
        public int Hardness { get; protected set; } = 0;
        /// <summary>
        /// Устойчивость блоков к взрывам. (для будущего)
        /// </summary>
        public float Resistance { get; protected set; } = 0;
        /// <summary>
        /// Включить в статистику. (для будущего)
        /// </summary>
        public bool EnableStats { get; protected set; } = true;
        /// <summary>
        /// Отмечает, относится ли этот блок к типу, требующему случайной пометки. 
        /// Функция ExtendedBlockStorage подсчитывает ссылки, чтобы в целях эффективности 
        /// отобрать фрагмент из случайного списка обновлений фрагментов. (для будущего)
        /// </summary>
        public bool NeedsRandomTick { get; protected set; } = false;

        /// <summary>
        /// Скользкость
        /// 0.6 стандартная
        /// 0.8 медленее, типа по песку
        /// 0.98 по льду, скользко
        /// </summary>
        public float Slipperiness { get; protected set; } = .6f;

        /// <summary>
        /// Вся ли прорисовка, аналог кактус, забор...
        /// </summary>
        //public EnumRenderType RenderType { get; protected set; } = EnumRenderType.АmbientOcclusion | EnumRenderType.Shadow;

        /// <summary>
        /// Получить тип блока
        /// </summary>
        public EnumBlock EBlock { get; protected set; }
        /// <summary>
        /// Явлыется ли блок небом
        /// </summary>
        public bool IsAir { get; protected set; } = false;
        /// <summary>
        /// Можно ли выбирать блок
        /// </summary>
        public bool IsAction { get; protected set; } = true;
        /// <summary>
        /// Может ли блок сталкиваться
        /// </summary>
        public bool IsCollidable { get; protected set; } = true;
        /// <summary>
        /// Цвет блока по умолчани, биом потом заменит
        /// Для частички
        /// </summary>
        public vec3 Color { get; protected set; } = new vec3(1);
        /// <summary>
        /// Индекс картинки частички
        /// </summary>
        public int Particle { get; protected set; } = 0;
        /// <summary>
        /// Имеется ли у блока частичка
        /// </summary>
        public bool IsParticle { get; protected set; } = true;

        /// <summary>
        /// Может на этот блок поставить другой, к примеру трава
        /// </summary>
        public bool IsReplaceable { get; protected set; } = false;
        
        protected Box[][] boxes;

        /// <summary>
        /// Семплы сломоного блока
        /// </summary>
        protected AssetsSample[] samplesBreak = new AssetsSample[] {
            AssetsSample.DigStone1, AssetsSample.DigStone2,
            AssetsSample.DigStone3, AssetsSample.DigStone4 };
        /// <summary>
        /// Семплы установленного блока
        /// </summary>
        protected AssetsSample[] samplesPut = new AssetsSample[] {
            AssetsSample.DigStone1, AssetsSample.DigStone2,
            AssetsSample.DigStone3, AssetsSample.DigStone4 };
        /// <summary>
        /// Семплы хотьбы по блоку
        /// </summary>
        protected AssetsSample[] samplesStep = new AssetsSample[] { 
            AssetsSample.StepStone1, AssetsSample.StepStone2,
            AssetsSample.StepStone3, AssetsSample.StepStone4 };

        /// <summary>
        /// Материал блока
        /// </summary>
        public EnumMaterial Material { get; protected set; } = EnumMaterial.Air;

        /// <summary>
        /// Коробки
        /// </summary>
        public virtual Box[] GetBoxes(int met) => boxes[0];

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs(int numberTexture) 
            => boxes = new Box[][] { new Box[] { new Box(numberTexture) } };

        /// <summary>
        /// Инициализация коробок
        /// </summary>
        protected void InitBoxs(int numberTexture, bool isColor, vec3 color)
            => boxes = new Box[][] { new Box[] { new Box(numberTexture, isColor, color) } };

        /// <summary>
        /// Задать тип блока
        /// </summary>
        public void SetEnumBlock(EnumBlock enumBlock) => EBlock = enumBlock;

        /// <summary>
        /// Передать список  ограничительных рамок блока
        /// </summary>
        public virtual AxisAlignedBB[] GetCollisionBoxesToList(BlockPos pos, int met)
        {
            vec3 min = pos.ToVec3();
            return new AxisAlignedBB[] { new AxisAlignedBB(min, min + 1f) };
        }

        /// <summary>
        /// Получить одну большую рамку блока, если их несколько они объеденяться
        /// </summary>
        public AxisAlignedBB GetCollision(BlockPos pos, int met)
        {
            if (!IsCollidable) return null;
            AxisAlignedBB[] axes = GetCollisionBoxesToList(pos, met);
            if (axes.Length > 0)
            {
                AxisAlignedBB aabb = axes[0];
                for (int i = 1; i < axes.Length; i++)
                {
                    aabb = aabb.AddCoord(axes[i].Min).AddCoord(axes[i].Max);
                }
                return aabb;
            }
            vec3 min = pos.ToVec3();
            return new AxisAlignedBB(min, min + 1f);
        }

        /// <summary>
        /// Проверить колизию блока на пересечение луча
        /// </summary>
        /// <param name="pos">позиция блока</param>
        /// <param name="a">точка от куда идёт лучь</param>
        /// <param name="dir">вектор луча</param>
        /// <param name="maxDist">максимальная дистания</param>
        public bool CollisionRayTrace(BlockPos pos, int met, vec3 a, vec3 dir, float maxDist)
        {
            if (IsAction)
            {
                if (FullBlock) return true;

                // Если блок не полный, обрабатываем хитбокс блока
                RayCross ray = new RayCross(a, dir, maxDist);
                return ray.IsCrossAABBs(GetCollisionBoxesToList(pos, met));
            }
            return false;
        }

        /// <summary>
        /// Значение для разрушения в тактах
        /// </summary>
        //public virtual int GetDamageValue() => 0;

       // public float GetBlockHardness()

        /// <summary>
        /// Получите твердость этого блока относительно способности данного игрока
        /// Тактов чтоб сломать
        /// </summary>
        public int GetPlayerRelativeBlockHardness(EntityPlayer playerIn)
        {
            if (playerIn.IsCreativeMode) return 0;
            //return 0; // креатив
            return Hardness; // выживание
            //return Hardness / 3; // выживание
            //return hardness < 0.0F ? 0.0F : (!playerIn.canHarvestBlock(this) ? playerIn.func_180471_a(this) / hardness / 100.0F : playerIn.func_180471_a(this) / hardness / 30.0F);
        }

        /// <summary>
        /// Блок не прозрачный, для расчёта освещения
        /// </summary>
        public bool IsNotTransparent() => LightOpacity > 13;

        /// <summary>
        /// Указывает, является ли материал полупрозрачным, от материала
        /// </summary>
        //public bool IsTranslucent() => Material == EnumMaterial.Air || Material == EnumMaterial.Glass 
        //    || Material == EnumMaterial.Water || Material == EnumMaterial.Debug;

        /// <summary>
        /// Создает данный ItemStack как EntityItem в мире в заданной позиции 
        /// </summary>
        /// <param name="worldIn"></param>
        /// <param name="pos"></param>
        /// <param name="itemStack"></param>
        public static void SpawnAsEntity(WorldBase worldIn, BlockPos blockPos, ItemStack itemStack)
        {
            if (!worldIn.IsRemote)
            {
                vec3 pos = new vec3(
                    blockPos.X + (float)worldIn.Rand.NextDouble() * .5f + .25f,
                    blockPos.Y + (float)worldIn.Rand.NextDouble() * .5f + .25f,
                    blockPos.Z + (float)worldIn.Rand.NextDouble() * .5f + .25f
                );
                EntityItem entityItem = new EntityItem(worldIn, pos, itemStack);
                entityItem.SetDefaultPickupDelay();
                worldIn.SpawnEntityInWorld(entityItem);
            }
        }

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        /// <param name="fortune">Чара удачи</param>
        public void DropBlockAsItem(WorldBase worldIn, BlockPos blockPos, BlockState state, int fortune)
            => DropBlockAsItemWithChance(worldIn, blockPos, state, 1.0f, fortune);

        /// <summary>
        /// Спавн предмета при разрушении этого блока
        /// </summary>
        /// <param name="chance">Вероятность выпадении предмета 1.0 всегда, 0.0 никогда</param>
        /// <param name="fortune">Чара удачи</param>
        public virtual void DropBlockAsItemWithChance(WorldBase worldIn, BlockPos blockPos, BlockState state, float chance, int fortune)
        {
            if (!worldIn.IsRemote)
            {
                int count = QuantityDroppedWithBonus(fortune, worldIn.Rand);

                for (int i = 0; i < count; i++)
                {
                    if (worldIn.Rand.NextDouble() <= chance)
                    {
                        ItemBase item = GetItemDropped(state, worldIn.Rand, fortune);

                        if (item != null)
                        {
                            SpawnAsEntity(worldIn, blockPos, new ItemStack(item, 1, DamageDropped(state)));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Получите значение урона, которое должен упасть этот блок
        /// </summary>
        public virtual int DamageDropped(BlockState state) => 0;

        /// <summary>
        /// Получите предмет, который должен выпасть из этого блока при сборе.
        /// </summary>
        public virtual ItemBase GetItemDropped(BlockState state, Random rand, int fortune) => new ItemBlock(this);

        /// <summary>
        /// Возвращает количество предметов, которые выпадают при разрушении блока.
        /// </summary>
        public virtual int QuantityDropped(Random random) => 1;

        /// <summary>
        /// Получите количество выпавших на основе данного уровня удачи
        /// </summary>
        public virtual int QuantityDroppedWithBonus(int fortune, Random random) => QuantityDropped(random);

        /// <summary>
        /// Установить блок
        /// </summary>
        /// <param name="side">Сторона на какой ставим блок</param>
        /// <param name="facing">Значение в пределах 0..1, образно фиксируем пиксел клика на стороне</param>
        public virtual bool Put(WorldBase worldIn, BlockPos blockPos, BlockState state, Pole side, vec3 facing) 
            => worldIn.SetBlockState(blockPos, state);

        /// <summary>
        /// Семпл сломанного блока
        /// </summary>
        public AssetsSample SampleBreak(WorldBase worldIn) => samplesBreak[worldIn.Rand.Next(samplesBreak.Length)];
        /// <summary>
        /// Семпл установленного блока
        /// </summary>
        public AssetsSample SamplePut(WorldBase worldIn) => samplesPut[worldIn.Rand.Next(samplesPut.Length)];
        /// <summary>
        /// Семпл хотьбы по блоку
        /// </summary>
        public AssetsSample SampleStep(WorldBase worldIn) => samplesStep[worldIn.Rand.Next(samplesStep.Length)];
        /// <summary>
        /// Тон сэмпла сломанного блока,
        /// </summary>
        public virtual float SampleBreakPitch(Random random) => 1f;

        /// <summary>
        /// Есть ли звуковой эффект шага
        /// </summary>
        public bool IsSampleStep() => samplesStep.Length > 0;

        /// <summary>
        /// Случайный эффект частички и/или звука на блоке только для клиента
        /// </summary>
        public virtual void RandomDisplayTick(WorldBase world, BlockPos blockPos, BlockState blockState, Random random) { }

        /// <summary>
        /// Строка
        /// </summary>
        public override string ToString() => EBlock.ToString();

        /// <summary>
        /// Флаги рендера блока
        /// </summary>
        [Flags]
        public enum EnumRenderType
        {
            /// <summary>
            /// Все стороны принудительно, пример: трава, стекло, вода, лава
            /// </summary>
            AllSideForcibly = 1,
            /// <summary>
            /// Сторону рисуем с двух сторон, пример: вода, лава
            /// </summary>
            BackSide = 2,
            /// <summary>
            /// Обрабатывается блок эффектом АmbientOcclusion
            /// </summary>
            АmbientOcclusion = 4,
            /// <summary>
            /// Нет бокового затемнения, пример: трава, цветы
            /// </summary>
            NoSideDimming = 8,
            /// <summary>
            /// Может ли быть тень сущности на блоке, только для целых блоков
            /// </summary>
            Shadow = 16,
            /// <summary>
            /// Не однотипные блоки, пример: трава, цветы, кактус
            /// </summary>
            BlocksNotSame = 32
        }
    }
}
