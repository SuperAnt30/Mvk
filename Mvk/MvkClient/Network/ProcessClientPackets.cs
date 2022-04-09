using MvkAssets;
using MvkClient.Entity;
using MvkClient.Setitings;
using MvkServer.Entity;
using MvkServer.Entity.Item;
using MvkServer.Entity.Mob;
using MvkServer.Glm;
using MvkServer.Item;
using MvkServer.Item.List;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
using MvkServer.World.Block;
using System.Collections;
using System.Threading.Tasks;

namespace MvkClient.Network
{
    /// <summary>
    /// Обработка клиентсиких пакетов для сервером
    /// </summary>
    public class ProcessClientPackets : ProcessPackets
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }

        public ProcessClientPackets(Client client) : base(true) => ClientMain = client;

        /// <summary>
        /// Передача данных для клиента
        /// </summary>
        public void ReceiveBuffer(byte[] buffer) => ReceivePacket(null, buffer);

        protected override void ReceivePacketClient(IPacket packet)
        {
            Task.Factory.StartNew(() =>
            {
                byte id = GetId(packet);
                if (id == 0xF0)
                {
                   HandleF0Connection((PacketSF0Connection)packet);
                }
                else if (ClientMain.World != null)
                {
                    switch (id)
                    {
                        case 0x00: Handle00Pong((PacketS00Pong)packet); break;
                        case 0x01: Handle01KeepAlive((PacketS01KeepAlive)packet); break;
                        case 0x02: Handle02JoinGame((PacketS02JoinGame)packet); break;
                        case 0x03: Handle03TimeUpdate((PacketS03TimeUpdate)packet); break;
                        case 0x04: Handle04EntityEquipment((PacketS04EntityEquipment)packet); break;
                        case 0x06: Handle06UpdateHealth((PacketS06UpdateHealth)packet); break;
                        case 0x07: Handle07Respawn((PacketS07Respawn)packet); break;
                        case 0x08: Handle08PlayerPosLook((PacketS08PlayerPosLook)packet); break;
                        case 0x0B: Handle0BAnimation((PacketS0BAnimation)packet); break;
                        case 0x0C: Handle0CSpawnPlayer((PacketS0CSpawnPlayer)packet); break;
                        case 0x0D: Handle0DCollectItem((PacketS0DCollectItem)packet); break;
                        case 0x0E: Handle0ESpawnItem((PacketS0ESpawnItem)packet); break;
                        case 0x0F: Handle0FSpawnMob((PacketS0FSpawnMob)packet); break;
                        case 0x12: Handle12EntityVelocity((PacketS12EntityVelocity)packet); break;
                        case 0x13: Handle13DestroyEntities((PacketS13DestroyEntities)packet); break;
                        case 0x14: Handle14EntityMotion((PacketS14EntityMotion)packet); break;
                        case 0x19: Handle19EntityStatus((PacketS19EntityStatus)packet); break;
                        case 0x1C: Handle1CEntityMetadata((PacketS1CEntityMetadata)packet); break;
                        case 0x21: Packet21((PacketS21ChunkData)packet); break;
                        case 0x23: Handle23BlockChange((PacketS23BlockChange)packet); break;
                        case 0x25: Handle25BlockBreakAnim((PacketS25BlockBreakAnim)packet); break;
                        case 0x2F: Handle2FSetSlot((PacketS2FSetSlot)packet); break;
                        case 0x30: Handle30WindowItems((PacketS30WindowItems)packet); break;

                        case 0xF1: HandleF1Disconnect((PacketSF1Disconnect)packet); break;

                    }
                }
            });
        }

        /// <summary>
        /// Пакет связи
        /// </summary>
        private void Handle00Pong(PacketS00Pong packet) => ClientMain.SetPing(packet.GetClientTime());

        /// <summary>
        /// KeepAlive
        /// </summary>
        private void Handle01KeepAlive(PacketS01KeepAlive packet) => ClientMain.TrancivePacket(new PacketC01KeepAlive(packet.GetTime()));

        /// <summary>
        /// Пакет соединения с сервером
        /// </summary>
        private void Handle02JoinGame(PacketS02JoinGame packet)
        {
            ClientMain.Player.SetDataPlayer(packet.GetId(), packet.GetUuid(), packet.IsCreativeMode(), ToNikname());
            ClientMain.GameModeBegin();
            // отправляем настройки
            ClientMain.TrancivePacket(new PacketC15ClientSetting(Setting.OverviewChunk));

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(ClientMain.Player.Position + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
        }

        /// <summary>
        /// Пакет синхронизации времени с сервером
        /// </summary>
        private void Handle03TimeUpdate(PacketS03TimeUpdate packet)
        {
            ClientMain.SetTickCounter(packet.GetTime());
        }

        /// <summary>
        /// Пакет оборудования сущности
        /// </summary>
        private void Handle04EntityEquipment(PacketS04EntityEquipment packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                entity.SetCurrentItemOrArmor(packet.GetSlot(), packet.GetItemStack());
            }
        }

        /// <summary>
        /// Пакет пораметров здоровья игрока
        /// </summary>
        private void Handle06UpdateHealth(PacketS06UpdateHealth packet)
        {
            ClientMain.Player.SetHealth(packet.GetHealth());
            ClientMain.Player.PerformHurtAnimation();
        }

        /// <summary>
        /// Пакет перезапуска игрока
        /// </summary>
        private void Handle07Respawn(PacketS07Respawn packet)
        {
            ClientMain.Player.Respawn();
        }

        /// <summary>
        /// Пакет расположения игрока, при старте, телепорт, рестарте и тп
        /// </summary>
        private void Handle08PlayerPosLook(PacketS08PlayerPosLook packet)
        {
            ClientMain.Player.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            ClientMain.Player.UpFrustumCulling();
        }

        /// <summary>
        /// Пакет анимации сущности
        /// </summary>
        private void Handle0BAnimation(PacketS0BAnimation packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                switch (packet.GetAnimation())
                {
                    case PacketS0BAnimation.EnumAnimation.SwingItem: entity.SwingItem(); break;
                    case PacketS0BAnimation.EnumAnimation.Hurt: entity.PerformHurtAnimation(); break;
                    case PacketS0BAnimation.EnumAnimation.Fall: entity.ParticleFall(10); break;
                }
            }
        }

        /// <summary>
        /// Пакет спавна других игроков
        /// </summary>
        private void Handle0CSpawnPlayer(PacketS0CSpawnPlayer packet)
        {
            // Удачный вход сетевого игрока, типа приветствие
            // Или после смерти
            EntityPlayerMP entity = new EntityPlayerMP(ClientMain.World);
            entity.SetDataPlayer(packet.GetId(), packet.GetUuid(), false, packet.GetName());
            entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
            entity.Inventory.SetCurrentItemAndArmor(packet.GetStacks());
            ArrayList list = packet.GetList();
            if (list != null && list.Count > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(list);
            }

            //entity.FlagSpawn = true;
            //ClientMain.World.SpawnEntityInWorld(entity);
            ClientMain.World.AddEntityToWorld(entity.Id, entity);

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(packet.GetPos() + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
            // entity.FlagSpawn = false;
        }

        /// <summary>
        /// Пакет передачи сущности предмета к сущности игрока
        /// </summary>
        private void Handle0DCollectItem(PacketS0DCollectItem packet)
        {
            EntityBase entityItem = ClientMain.World.GetEntityByID(packet.GetItemId());
            if (entityItem != null)
            {
                EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetEntityId());
                if (entity == null) entity = ClientMain.Player;

                ClientMain.Sample.PlaySound(AssetsSample.Click, .2f);
                // HACK::2022-04-04 надо как-то вынести в игровой поток, а то ошибка вылетает
                ClientMain.World.RemoveEntityFromWorld(packet.GetItemId());
            }
        }

        /// <summary>
        /// Пакет спавна вещи
        /// </summary>
        private void Handle0ESpawnItem(PacketS0ESpawnItem packet)
        {
            ItemBase item = null;
            if (packet.IsBlock())
            {
                item = new ItemBlock(Blocks.GetBlockCache((EnumBlock)packet.GetItemId()));
            }

            if (item != null)
            {
                ItemStack stack = new ItemStack(item);
                EntityItem entity = new EntityItem(ClientMain.World, packet.GetPos(), stack);
                entity.SetEntityId(packet.GetEntityId());
                entity.SetPosSpawn(packet.GetPos());
                ArrayList list = packet.GetList();
                if (list != null && list.Count > 0)
                {
                    entity.MetaData.UpdateWatchedObjectsFromList(list);
                }
                ClientMain.World.AddEntityToWorld(entity.Id, entity);
            }
            //ItemStack stack = new ItemStack(ClientMain.World.GetBlock(new vec3i(packet.GetPos())));
            //EntityItem entity = new EntityItem(ClientMain.World, );
            //entity.SetEntityId(packet.GetEntityId());
            //entity.SetPosSpawn(packet.GetPos());
            //ClientMain.World.AddEntityToWorld(entity.Id, entity);
        }

        /// <summary>
        /// Пакет спавна мобов
        /// </summary>
        private void Handle0FSpawnMob(PacketS0FSpawnMob packet)
        {
            if (packet.GetEnum() == EnumEntities.Chicken)
            {
                EntityChicken entity = new EntityChicken(ClientMain.World);
                entity.SetEntityId(packet.GetId());
                entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());
                ArrayList list = packet.GetList();
                if (list != null && list.Count > 0)
                {
                    entity.MetaData.UpdateWatchedObjectsFromList(list);
                }
                //entity.Test();
                ClientMain.World.AddEntityToWorld(entity.Id, entity);
            }
            //else if (packet.GetEnum() == EnumEntities.Item)
            //{
            //    ItemStack stack = new ItemStack(ClientMain.World.GetBlock(new vec3i(packet.GetPos())));
            //    EntityItem entity = new EntityItem(ClientMain.World);
            //    entity.SetEntityId(packet.GetId());
            //    entity.SetPosSpawn(packet.GetPos());
            //    ClientMain.World.AddEntityToWorld(entity.Id, entity);
            //}
            //entity.FlagSpawn = true;
            
        
            //ClientMain.World.SpawnEntityInWorld(entity);

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(packet.GetPos() + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
            //entity.FlagSpawn = false;
        }

        
        private void Handle12EntityVelocity(PacketS12EntityVelocity packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                entity.MotionPush = packet.GetMotion();
            }
        }

        /// <summary>
        /// Пакет удаление сущностей
        /// </summary>
        private void Handle13DestroyEntities(PacketS13DestroyEntities packet)
        {
            int count = packet.GetIds().Length;
            for(int i = 0; i < count; i++)
            {
                ClientMain.World.RemoveEntityFromWorld(packet.GetIds()[i]);
            }
        }

        /// <summary>
        /// Пакет перемещения сущности
        /// </summary>
        private void Handle14EntityMotion(PacketS14EntityMotion packet)
        {
            EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            if (entity != null)
            {
                if (entity is EntityLiving entityLiving)
                {
                    entityLiving.SetMotionServer(
                        packet.GetPos(), packet.GetYaw(), packet.GetPitch(),
                        packet.OnGround());
                }
                else if (entity is EntityItem entityItem)
                {
                    entityItem.SetMotionServer(packet.GetPos(), packet.OnGround());
                }
            }
        }

        /// <summary>
        /// Пакет статуса сущности, умирает, урон и прочее
        /// </summary>
        private void Handle19EntityStatus(PacketS19EntityStatus packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityLivingByID(packet.GetId());
            if (entity != null)
            {
                switch (packet.GetStatus())
                {
                    case PacketS19EntityStatus.EnumStatus.Die: entity.SetHealth(0); break;
                }
            }
        }

        /// <summary>
        /// Пакет дополнительных данных сущности
        /// </summary>
        private void Handle1CEntityMetadata(PacketS1CEntityMetadata packet)
        {
            EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            ArrayList list = packet.GetList();
            if (entity != null && list != null && list.Count > 0)
            {
                entity.MetaData.UpdateWatchedObjectsFromList(list);
            }

            //PacketS1CEntityMetadata.EnumData data = packet.GetEnumData();
            //if (data == PacketS1CEntityMetadata.EnumData.Amount)
            //{
            //    EntityBase entity = ClientMain.World.GetEntityByID(packet.GetId());
            //    if (entity != null && entity is EntityItem entityItem)
            //    {
            //        entityItem.Stack.SetAmount(packet.GetAmount());
            //    }
            //} else if (data == PacketS1CEntityMetadata.EnumData.SneakingSprinting)
            //{
            //    EntityLiving entityLiving = ClientMain.World.GetEntityLivingByID(packet.GetId());
            //    if (entityLiving != null)
            //    {
            //        entityLiving.SetSneakingSprinting(packet.IsSneaking(), packet.IsSprinting());
            //    }
            //}
        }

        private void Handle23BlockChange(PacketS23BlockChange packet)
        {
            ClientMain.World.SetBlockState(packet.GetBlockPos(), packet.GetDigging());
        }

        private void Handle25BlockBreakAnim(PacketS25BlockBreakAnim packet)
        {
            ClientMain.World.SendBlockBreakProgress(packet.GetBreakerId(), packet.GetBlockPos(), packet.GetProgress());
        }

        /// <summary>
        /// Редактирование слота игрока
        /// </summary>
        private void Handle2FSetSlot(PacketS2FSetSlot packet)
        {
            ClientMain.Player.Inventory.SetInventorySlotContents(packet.GetSlot(), packet.GetItemStack());
        }

        private void Handle30WindowItems(PacketS30WindowItems packet)
        {
            ClientMain.Player.Inventory.SetMainAndArmor(packet.GetStacks());
        }

        private void Packet21(PacketS21ChunkData packet) 
            => ClientMain.World.ChunkPrClient.PacketChunckData(packet);

        #region ConnectionDisconnect

        /// <summary>
        /// Пакет соединения
        /// </summary>
        private void HandleF0Connection(PacketSF0Connection packet)
        {
            if (packet.IsConnect())
            {
                // connect
                ClientMain.TrancivePacket(new PacketC02LoginStart(ToNikname()));
            }
            else
            {
                // disconnect с причиной
                ClientMain.ExitingWorld(packet.GetCause());
            }
        }

        /// <summary>
        /// Для дебага, ник в сети с пометкой
        /// </summary>
        private string ToNikname() 
            => Setting.Nickname + (!MvkServer.MvkGlobal.IS_DEBUG_NICKNAME || ClientMain.IsServerLocalRun() ? "" : "-Net");
        /// <summary>
        /// Дисконект игрока
        /// </summary>
        private void HandleF1Disconnect(PacketSF1Disconnect packet)
        {
            ClientMain.World.RemoveEntityFromWorld(packet.GetId());
        }

        #endregion
    }
}
