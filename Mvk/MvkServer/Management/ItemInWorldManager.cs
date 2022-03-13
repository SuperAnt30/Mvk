using MvkServer.Entity.Player;
using MvkServer.Util;
using MvkServer.World;
using MvkServer.World.Block;

namespace MvkServer.Management
{
    /// <summary>
    /// Объект работы над игроком
    /// </summary>
    public class ItemInWorldManager
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; private set; }
        /// <summary>
        /// Объект игрока
        /// </summary>
        public EntityPlayerServer Player { get; private set; }
        /// <summary>
        /// Объект логотладки
        /// </summary>
        private Profiler profiler;

        /// <summary>
        /// Истинно, если игрок уничтожает блок
        /// </summary>
        private bool isDestroyingBlock = false;
        private int initialDamage;
        private BlockPos blockPosDestroy = new BlockPos();
        private int curblockDamage;

        /// <summary>
        /// Установите значение true, когда получен пакет «завершенное уничтожение блока»,
        /// но блок еще не был полностью поврежден. Блок не будет уничтожен, пока это ложно.
        /// </summary>
       // private bool receivedFinishDiggingPacket = false;
       // private BlockPos field_180241_i = new BlockPos();
        //private int initialBlockDamage;
        private int durabilityRemainingOnBlock;

        public ItemInWorldManager(WorldServer worldServer, EntityPlayerServer entityPlayer)
        {
            World = worldServer;
            profiler = new Profiler(worldServer.ServerMain.Log);
            Player = entityPlayer;

            durabilityRemainingOnBlock = -1;
        }

        public void DiggingStart(BlockPos blockPos)
        {
            BlockBase block = World.GetBlock(blockPos);
            if (block != null && !block.IsAir)
            {
                durabilityRemainingOnBlock = -1;
                isDestroyingBlock = true;
                blockPosDestroy = blockPos;
                curblockDamage = 0;
                initialDamage = block.GetDamageValue();
                // (initialDamage - blockHitDelay) * 10 / initialDamage
                //int var5 = (int)(var6 * 10.0F);
                World.SendBlockBreakProgress(Player.Id, blockPos, 0);// + var5);
                //durabilityRemainingOnBlock = var5;
            }
        }

        public void DiggingAbout()
        {
            World.SendBlockBreakProgress(Player.Id, blockPosDestroy, -1);
            isDestroyingBlock = false;
            blockPosDestroy = new BlockPos();
        }

        /// <summary>
        /// Обновление разрушающих блоков
        /// </summary>
        public void UpdateBlockRemoving()
        {
            //server.maneger.ItemInWorldManager.UpdateBlockRemoving
            curblockDamage++;
            //float var3;
            int var4;

            //if (receivedFinishDiggingPacket)
            //{
            //    int var1 = curblockDamage - initialBlockDamage;
            //    BlockBase block = World.GetBlock(field_180241_i);

            //    if (block == null || block.IsAir)
            //    {
            //        receivedFinishDiggingPacket = false;
            //    }
            //    else
            //    {
            //        //var3 = block.GetPlayerRelativeBlockHardness(Player, this.thisPlayerMP.worldObj, this.field_180241_i) * (float)(var1 + 1);
            //        var3 = block.GetDamageValue();
                    
            //        var4 = (int)(var3 * 10.0F);

            //        if (var4 != durabilityRemainingOnBlock)
            //        {
            //            World.SendBlockBreakProgress(Player.Id, field_180241_i, var4);
            //            durabilityRemainingOnBlock = var4;
            //        }

            //        if (var3 >= 1.0F)
            //        {
            //            receivedFinishDiggingPacket = false;
            //            func_180237_b(field_180241_i);
            //        }
            //    }
            //}
            //else 
            if (isDestroyingBlock)
            {
                BlockBase block = World.GetBlock(blockPosDestroy);

                if (block == null || block.IsAir)
                {
                    World.SendBlockBreakProgress(Player.Id, blockPosDestroy, -1);
                    durabilityRemainingOnBlock = -1;
                    isDestroyingBlock = false;
                }
                else
                {
                    // int var6 = initialDamage - curblockDamage;
                    //var3 = block.GetPlayerRelativeBlockHardness(this.thisPlayerMP, this.thisPlayerMP.worldObj, field_180241_i) * (float)(var6 + 1);
                    //var3 = block.GetDamageValue();
                    //var4 = (int)(var3 * 10.0F);
                    var4 = initialDamage == 0 ? 0 : curblockDamage * 10 / initialDamage;
                    if (var4 != durabilityRemainingOnBlock)
                    {
                        World.SendBlockBreakProgress(Player.Id, blockPosDestroy, var4);
                        durabilityRemainingOnBlock = var4;
                    }
                }
            }
        }

        public bool func_180237_b(BlockPos p_180237_1_)
        {
            //if (this.gameType.isCreative() && this.thisPlayerMP.getHeldItem() != null && this.thisPlayerMP.getHeldItem().getItem() instanceof ItemSword)
            //{
                return false;
            //}
            //else
            //{
            //    IBlockState var2 = this.theWorld.getBlockState(p_180237_1_);
            //    TileEntity var3 = this.theWorld.getTileEntity(p_180237_1_);

            //    if (this.gameType.isAdventure())
            //    {
            //        if (this.gameType == WorldSettings.GameType.SPECTATOR)
            //        {
            //            return false;
            //        }

            //        if (!this.thisPlayerMP.func_175142_cm())
            //        {
            //            ItemStack var4 = this.thisPlayerMP.getCurrentEquippedItem();

            //            if (var4 == null)
            //            {
            //                return false;
            //            }

            //            if (!var4.canDestroy(var2.getBlock()))
            //            {
            //                return false;
            //            }
            //        }
            //    }

            //    this.theWorld.playAuxSFXAtEntity(this.thisPlayerMP, 2001, p_180237_1_, Block.getStateId(var2));
            //    boolean var7 = this.func_180235_c(p_180237_1_);

            //    if (this.isCreative())
            //    {
            //        this.thisPlayerMP.playerNetServerHandler.sendPacket(new S23PacketBlockChange(this.theWorld, p_180237_1_));
            //    }
            //    else
            //    {
            //        ItemStack var5 = this.thisPlayerMP.getCurrentEquippedItem();
            //        boolean var6 = this.thisPlayerMP.canHarvestBlock(var2.getBlock());

            //        if (var5 != null)
            //        {
            //            var5.onBlockDestroyed(this.theWorld, var2.getBlock(), p_180237_1_, this.thisPlayerMP);

            //            if (var5.stackSize == 0)
            //            {
            //                this.thisPlayerMP.destroyCurrentEquippedItem();
            //            }
            //        }

            //        if (var7 && var6)
            //        {
            //            var2.getBlock().harvestBlock(this.theWorld, this.thisPlayerMP, p_180237_1_, var2, var3);
            //        }
            //    }

            //    return var7;
            //}
        }
    }
}
