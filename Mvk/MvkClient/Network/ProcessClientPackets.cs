using MvkClient.Entity;
using MvkClient.Setitings;
using MvkServer.Entity;
using MvkServer.Entity.Mob;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
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
                        case 0x06: Handle06UpdateHealth((PacketS06UpdateHealth)packet); break;
                        case 0x07: Handle07Respawn((PacketS07Respawn)packet); break;
                        case 0x08: Handle08PlayerPosLook((PacketS08PlayerPosLook)packet); break;
                        case 0x0B: Handle0BAnimation((PacketS0BAnimation)packet); break;
                        case 0x0C: Handle0CSpawnPlayer((PacketS0CSpawnPlayer)packet); break;
                        case 0x0F: Handle0FSpawnMob((PacketS0FSpawnMob)packet); break;
                        case 0x12: Handle12EntityVelocity((PacketS12EntityVelocity)packet); break;
                        case 0x13: Handle13DestroyEntities((PacketS13DestroyEntities)packet); break;
                        case 0x14: Handle14EntityMotion((PacketS14EntityMotion)packet); break;
                        case 0x19: Handle19EntityStatus((PacketS19EntityStatus)packet); break;
                        case 0x21: Packet21((PacketS21ChunkData)packet); break;
                        case 0x23: Handle23BlockChange((PacketS23BlockChange)packet); break;
                        case 0x25: Handle25BlockBreakAnim((PacketS25BlockBreakAnim)packet); break;

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
            ClientMain.Player.SetDataPlayer(packet.GetId(), packet.GetUuid(), ToNikname());
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
            EntityLiving entity = ClientMain.World.GetEntityByID(packet.GetId());
            if (entity != null)
            {
                switch (packet.GetAnimation())
                {
                    case PacketS0BAnimation.EnumAnimation.SwingItem: entity.SwingItem(); break;
                    case PacketS0BAnimation.EnumAnimation.Hurt: entity.PerformHurtAnimation(); break;
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
            entity.SetDataPlayer(packet.GetId(), packet.GetUuid(), packet.GetName());
            entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());

            //entity.FlagSpawn = true;
            ClientMain.World.SpawnEntityInWorld(entity);

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(packet.GetPos() + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
           // entity.FlagSpawn = false;
        }

        /// <summary>
        /// Пакет спавна мобов
        /// </summary>
        private void Handle0FSpawnMob(PacketS0FSpawnMob packet)
        {
            EntityLiving entity = new EntityChicken(ClientMain.World);
            entity.SetEntityId(packet.GetId());
            entity.SetPosLook(packet.GetPos(), packet.GetYaw(), packet.GetPitch());

            //entity.FlagSpawn = true;
            ClientMain.World.SpawnEntityInWorld(entity);

            //EntityChicken entityChicken = new EntityChicken(ClientMain.World);
            //entityChicken.SetEntityId(101);
            //entityChicken.SetPosition(packet.GetPos() + new vec3(3, 0, 0));
            //ClientMain.World.SpawnEntityInWorld(entityChicken);
            //entity.FlagSpawn = false;
        }

        
        private void Handle12EntityVelocity(PacketS12EntityVelocity packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityByID(packet.GetId());
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
            EntityLiving entity = ClientMain.World.GetEntityByID(packet.GetId());
            if (entity != null)
            {
                entity.SetMotionServer(
                    packet.GetPos(), packet.GetYaw(), packet.GetPitch(),
                    packet.IsSneaking(), packet.OnGround());
            }
        }


        /// <summary>
        /// Пакет статуса сущности, умирает, урон и прочее
        /// </summary>
        private void Handle19EntityStatus(PacketS19EntityStatus packet)
        {
            EntityLiving entity = ClientMain.World.GetEntityByID(packet.GetId());
            if (entity != null)
            {
                switch (packet.GetStatus())
                {
                    case PacketS19EntityStatus.EnumStatus.Die: entity.SetHealth(0); break;
                }
            }
        }

        private void Handle23BlockChange(PacketS23BlockChange packet)
        {
            ClientMain.World.SetBlockState(packet.GetBlockPos(), packet.GetDigging());
        }

        private void Handle25BlockBreakAnim(PacketS25BlockBreakAnim packet)
        {
            ClientMain.World.SendBlockBreakProgress(packet.GetBreakerId(), packet.GetBlockPos(), packet.GetProgress());
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
