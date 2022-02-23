using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets.Server;
using MvkServer.Util;
using MvkServer.World;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект прослеживания конкретной сущности
    /// </summary>
    public class EntityTrackerEntry
    {
        /// <summary>
        /// Объект сущности которую прослеживаем
        /// </summary>
        public EntityLiving TrackedEntity { get; private set; }
        /// <summary>
        /// Пороговое значение расстояния отслеживания
        /// </summary>
        public int TrackingDistanceThreshold { get; private set; }
        /// <summary>
        /// Содержит ссылки на всех игроков, которые в настоящее время получают обновления позиций для этого объекта.
        /// </summary>
        public MapListEntity TrackingPlayers { get; private set; } = new MapListEntity();
        /// <summary>
        /// Частота проверки синхронизацию,
        /// когда тикает % UpdateFrequency==0 
        /// </summary>
        public int UpdateFrequency { get; private set; }
        /// <summary>
        /// ****
        /// </summary>
        public bool PlayerEntitiesUpdated { get; private set; }
        /// <summary>
        /// Счётчитк обновлений
        /// </summary>
        public int UpdateCounter { get; private set; }

        public vec3i EncodedPos { get; private set; }
        public int EncodedRotationYaw { get; private set; }
        public int EncodedRotationPitch { get; private set; }
        public int LastHeadMotion { get; private set; }
        public vec3 LastTrackedEntityMotion { get; private set; }
        public vec3 LastTrackedEntityPos { get; private set; }

        /// <summary>
        /// Каждые 400 тиков отправляется полный пакет телепортации, а не просто команда «переместить меня + x», 
        /// так что эта позиция остается полностью синхронизированной. 
        /// </summary>
       // private int ticksSinceLastForcedTeleport;
        /// <summary>
        /// Отправлять обновления скорости 
        /// </summary>
        private bool sendVelocityUpdates;
        /// <summary>
        /// На земле
        /// </summary>
        private bool onGround;
        private bool firstUpdateDone;

        public EntityTrackerEntry(EntityLiving entity, int trackingRange, int updateFrequency, bool sendVelocityUpdates)
        {
            TrackedEntity = entity;
            TrackingDistanceThreshold = trackingRange;
            UpdateFrequency = updateFrequency;
            this.sendVelocityUpdates = sendVelocityUpdates;
            EncodedPos = new vec3i(entity.Position * 32f);
            EncodedRotationYaw = Mth.Floor(entity.RotationYaw * 256f / glm.pi360);
            EncodedRotationPitch = Mth.Floor(entity.RotationPitch * 256f / glm.pi360);
            LastHeadMotion = Mth.Floor(entity.GetRotationYawHead() * 256f / glm.pi360);
            onGround = entity.OnGround;
        }

        public override bool Equals(object obj)
            => obj is EntityTrackerEntry ? ((EntityTrackerEntry)obj).TrackedEntity.Id == TrackedEntity.Id : false;

        public override int GetHashCode() => TrackedEntity.Id;

        /// <summary>
        /// Обновить список игроков
        /// </summary>
        public void UpdatePlayerEntities(MapListEntity entityPlayers)
        {
            for (int i = 0; i < entityPlayers.Count; i++)
            {
                UpdatePlayerEntity((EntityPlayerServer)entityPlayers.GetAt(i));
            }
        }

        /// <summary>
        /// Обновить видна ли текущая сущность у игрока entityPlayer
        /// </summary>
        public void UpdatePlayerEntity(EntityPlayerServer entityPlayer)
        {
            if (entityPlayer != TrackedEntity)
            {
                if (!entityPlayer.IsDead && CheckPosition(entityPlayer))
                {
                    if (!TrackingPlayers.ContainsValue(entityPlayer))
                         //&& IsPlayerWatchingThisChunk(entityPlayer))
                         //&& (IsPlayerWatchingThisChunk(entityPlayer) || TrackedEntity.FlagSpawn))
                    {
                        TrackingPlayers.Add(entityPlayer);
                        IPacket packet = PacketSpawn();
                        entityPlayer.SendPacket(packet);


                        // Передаём доп пакеты, для сущности параметров

                        LastTrackedEntityMotion = TrackedEntity.Motion;

                        //        if (this.trackedEntity instanceof EntityLivingBase)
                        //    {
                        //            ServersideAttributeMap var4 = (ServersideAttributeMap)((EntityLivingBase)this.trackedEntity).getAttributeMap();
                        //            Collection var5 = var4.getWatchedAttributes();

                        //            if (!var5.isEmpty())
                        //            {
                        //                entityPlayer.playerNetServerHandler.sendPacket(new S20PacketEntityProperties(this.trackedEntity.getEntityId(), var5));
                        //            }
                        //        }


                    }
                }
                else if (TrackingPlayers.ContainsValue(entityPlayer))
                {
                    TrackingPlayers.Remove(entityPlayer);
                    entityPlayer.SendRemoveEntity(TrackedEntity);
                }
            }
        }

        

        /// <summary>
        /// Отправить сетевой пакет всем игрокам которые видят эту сущность без этой
        /// </summary>
        public void SendPacketPlayers(IPacket packet)
        {
            for (int i = 0; i < TrackingPlayers.Count; i++)
            {
                EntityPlayerServer entity = (EntityPlayerServer)TrackingPlayers.GetAt(i);
                entity.SendPacket(packet);
            }
        }

        /// <summary>
        /// Отправить сетевой пакет всем игрокам которые видят эту сущность вместе с этой
        /// </summary>
        public void SendPacketPlayersCurrent(IPacket packet)
        {
            SendPacketPlayers(packet);
            if (TrackedEntity is EntityPlayerServer)
            {
                ((EntityPlayerServer)TrackedEntity).SendPacket(packet);
            }
        }

        /// <summary>
        /// Удалить у всех игроков, тикущую отслеживаемую сущность
        /// </summary>
        public void SendDestroyEntityPacketToTrackedPlayers()
        {
            for (int i = 0; i < TrackingPlayers.Count; i++)
            {
                EntityPlayerServer entity = (EntityPlayerServer)TrackingPlayers.GetAt(i);
                entity.SendRemoveEntity(TrackedEntity);
            }
        }

        /// <summary>
        /// Удалите отслеживаемого игрока из нашего списка и 
        /// прикажите отслеживаемому игроку уничтожить нас из своего мира. 
        /// </summary>
        public void RemoveTrackedPlayerSymmetric(EntityPlayerServer entity)
        {
            if (TrackingPlayers.ContainsValue(entity))
            {
                TrackingPlayers.Remove(entity);
                entity.SendRemoveEntity(TrackedEntity);
            }
        }

        /// <summary>
        /// Обновить видимость сущностей с списком игроков
        /// </summary>
        /// <param name="playerEntities">список игроков</param>
        public void UpdatePlayerList(MapListEntity playerEntities)
        {
            PlayerEntitiesUpdated = false;

            if (!firstUpdateDone || TrackedEntity.IsDead || glm.distance(TrackedEntity.Position, LastTrackedEntityPos) > 2f)
            {
                LastTrackedEntityPos = TrackedEntity.Position;
                firstUpdateDone = true;
                PlayerEntitiesUpdated = true;
                UpdatePlayerEntities(playerEntities);
            }

            if (TrackedEntity.ActionChanged != EnumActionChanged.None)
            {
                SendPacketPlayers(new PacketS14EntityMotion(TrackedEntity));
                TrackedEntity.ActionNone();
            }

            if (UpdateCounter % UpdateFrequency == 0)
            {
        //        int var23;
        //        int var24;

        //        if (this.trackedEntity.ridingEntity == null)
        //        {
        //            ++this.ticksSinceLastForcedTeleport;
        //            var23 = MathHelper.floor_double(this.trackedEntity.posX * 32.0D);
        //            var24 = MathHelper.floor_double(this.trackedEntity.posY * 32.0D);
        //            int var25 = MathHelper.floor_double(this.trackedEntity.posZ * 32.0D);
        //            int var27 = MathHelper.floor_float(this.trackedEntity.rotationYaw * 256.0F / 360.0F);
        //            int var28 = MathHelper.floor_float(this.trackedEntity.rotationPitch * 256.0F / 360.0F);
        //            int var29 = var23 - this.encodedPosX;
        //            int var30 = var24 - this.encodedPosY;
        //            int var9 = var25 - this.encodedPosZ;
        //            Object var10 = null;
        //            boolean var11 = Math.abs(var29) >= 4 || Math.abs(var30) >= 4 || Math.abs(var9) >= 4 || this.updateCounter % 60 == 0;
        //            boolean var12 = Math.abs(var27 - this.encodedRotationYaw) >= 4 || Math.abs(var28 - this.encodedRotationPitch) >= 4;

        //            if (this.updateCounter > 0 || this.trackedEntity instanceof EntityArrow)
        //        {
        //                if (var29 >= -128 && var29 < 128 && var30 >= -128 && var30 < 128 && var9 >= -128 && var9 < 128 && this.ticksSinceLastForcedTeleport <= 400 && !this.ridingEntity && this.field_180234_y == this.trackedEntity.onGround)
        //                {
        //                    if (var11 && var12)
        //                    {
        //                        var10 = new S14PacketEntity.S17PacketEntityLookMove(this.trackedEntity.getEntityId(), (byte)var29, (byte)var30, (byte)var9, (byte)var27, (byte)var28, this.trackedEntity.onGround);
        //                    }
        //                    else if (var11)
        //                    {
        //                        var10 = new S14PacketEntity.S15PacketEntityRelMove(this.trackedEntity.getEntityId(), (byte)var29, (byte)var30, (byte)var9, this.trackedEntity.onGround);
        //                    }
        //                    else if (var12)
        //                    {
        //                        var10 = new S14PacketEntity.S16PacketEntityLook(this.trackedEntity.getEntityId(), (byte)var27, (byte)var28, this.trackedEntity.onGround);
        //                    }
        //                }
        //                else
        //                {
        //                    this.field_180234_y = this.trackedEntity.onGround;
        //                    this.ticksSinceLastForcedTeleport = 0;
        //                    var10 = new S18PacketEntityTeleport(this.trackedEntity.getEntityId(), var23, var24, var25, (byte)var27, (byte)var28, this.trackedEntity.onGround);
        //                }
        //            }

        //            if (this.sendVelocityUpdates)
        //            {
        //                double var13 = this.trackedEntity.motionX - this.lastTrackedEntityMotionX;
        //                double var15 = this.trackedEntity.motionY - this.lastTrackedEntityMotionY;
        //                double var17 = this.trackedEntity.motionZ - this.motionZ;
        //                double var19 = 0.02D;
        //                double var21 = var13 * var13 + var15 * var15 + var17 * var17;

        //                if (var21 > var19 * var19 || var21 > 0.0D && this.trackedEntity.motionX == 0.0D && this.trackedEntity.motionY == 0.0D && this.trackedEntity.motionZ == 0.0D)
        //                {
        //                    this.lastTrackedEntityMotionX = this.trackedEntity.motionX;
        //                    this.lastTrackedEntityMotionY = this.trackedEntity.motionY;
        //                    this.motionZ = this.trackedEntity.motionZ;
        //                    this.func_151259_a(new S12PacketEntityVelocity(this.trackedEntity.getEntityId(), this.lastTrackedEntityMotionX, this.lastTrackedEntityMotionY, this.motionZ));
        //                }
        //            }

        //            if (var10 != null)
        //            {
        //                this.func_151259_a((Packet)var10);
        //            }

        //            this.sendMetadataToAllAssociatedPlayers();

        //            if (var11)
        //            {
        //                this.encodedPosX = var23;
        //                this.encodedPosY = var24;
        //                this.encodedPosZ = var25;
        //            }

        //            if (var12)
        //            {
        //                this.encodedRotationYaw = var27;
        //                this.encodedRotationPitch = var28;
        //            }

        //            this.ridingEntity = false;
        //        }
        //        else
        //        {
        //            var23 = MathHelper.floor_float(this.trackedEntity.rotationYaw * 256.0F / 360.0F);
        //            var24 = MathHelper.floor_float(this.trackedEntity.rotationPitch * 256.0F / 360.0F);
        //            boolean var26 = Math.abs(var23 - this.encodedRotationYaw) >= 4 || Math.abs(var24 - this.encodedRotationPitch) >= 4;

        //            if (var26)
        //            {
        //                this.func_151259_a(new S14PacketEntity.S16PacketEntityLook(this.trackedEntity.getEntityId(), (byte)var23, (byte)var24, this.trackedEntity.onGround));
        //                this.encodedRotationYaw = var23;
        //                this.encodedRotationPitch = var24;
        //            }

        //            this.encodedPosX = MathHelper.floor_double(this.trackedEntity.posX * 32.0D);
        //            this.encodedPosY = MathHelper.floor_double(this.trackedEntity.posY * 32.0D);
        //            this.encodedPosZ = MathHelper.floor_double(this.trackedEntity.posZ * 32.0D);
        //            this.sendMetadataToAllAssociatedPlayers();
        //            this.ridingEntity = true;
        //        }

        //        var23 = MathHelper.floor_float(this.trackedEntity.getRotationYawHead() * 256.0F / 360.0F);

        //        if (Math.abs(var23 - this.lastHeadMotion) >= 4)
        //        {
        //            this.func_151259_a(new S19PacketEntityHeadLook(this.trackedEntity, (byte)var23));
        //            this.lastHeadMotion = var23;
        //        }

        //        this.trackedEntity.isAirBorne = false;
            }

             UpdateCounter++;

        //    if (this.trackedEntity.velocityChanged)
        //    {
        //        this.func_151261_b(new S12PacketEntityVelocity(this.trackedEntity));
        //        this.trackedEntity.velocityChanged = false;
        //    }
        }

        /// <summary>
        /// Проверка позиции
        /// </summary>
        private bool CheckPosition(EntityPlayerServer entityPlayer)
        {
            return glm.distance(TrackedEntity.Position, entityPlayer.Position) < TrackingDistanceThreshold;
            //float x = entityPlayer.Position.x - EncodedPos.x / 32f;
            //float z = entityPlayer.Position.z - EncodedPos.z / 32f;
            //return x >= -TrackingDistanceThreshold && x <= TrackingDistanceThreshold 
            //    && z >= -TrackingDistanceThreshold && z <= TrackingDistanceThreshold;
        }

        /// <summary>
        /// Находится ли игрок в чанке с тикущей сущностью
        /// </summary>
        private bool IsPlayerWatchingThisChunk(EntityPlayerServer entityPlayer)
        {
            return ((WorldServer)entityPlayer.World).Players.IsPlayerWatchingChunk(entityPlayer, TrackedEntity.PositionChunk);
        }

        /// <summary>
        /// Определяем кого надо спавнить
        /// </summary>
        private IPacket PacketSpawn()
        {
            if (TrackedEntity is EntityPlayerServer)
            {
                return new PacketS0CSpawnPlayer((EntityPlayer)TrackedEntity);
            }
            //if (TrackedEntity is EntityPlayerServer)
            else
            {
                return new PacketS0FSpawnMob((EntityLivingHead)TrackedEntity);
            }
        }

        public override string ToString()
        {
            string list = "";
            for (int i = 0; i < TrackingPlayers.Count; i++)
            {
                list += TrackingPlayers.GetAt(i).Id + ", ";
            }
            return string.Format("#{0} {1} c:{2} ({3})", TrackedEntity.Id, TrackedEntity.Name, TrackingPlayers.Count, list);
        }
    }
}
