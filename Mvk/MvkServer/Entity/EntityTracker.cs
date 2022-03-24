using MvkServer.Entity.Player;
using MvkServer.Network;
using MvkServer.World;
using System;
using System.Collections.Generic;

namespace MvkServer.Entity
{
    /// <summary>
    /// Объект прослеживания всех видимых сущностей на сервере
    /// </summary>
    public class EntityTracker
    {
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; protected set; }
        /// <summary>
        /// Список всех треков сущностей
        /// </summary>
        private MapListEntityTrackerEntry trackedEntities = new MapListEntityTrackerEntry();

        /// <summary>
        /// Максимальное пороговое значение расстояния отслеживания 
        /// </summary>
        private readonly int maxTrackingDistanceThreshold = 512;

        public EntityTracker(WorldServer world) => World = world;

        /// <summary>
        /// Добавить сущность
        /// </summary>
        /// <param name="entity">сущность</param>
        public void EntityAdd(EntityBase entity)
        {
            if (entity is EntityPlayerServer)
            {
                AddEntityToTracker(entity, 256, 2, false);
                
                for (int i = 0; i < trackedEntities.Count; i++)
                {
                    EntityTrackerEntry trackerEntry = trackedEntities.GetAt(i);
                    if (trackerEntry != null)
                    {
                        trackerEntry.UpdatePlayerEntity((EntityPlayerServer)entity);
                    }
                }
            }
            else
            {
                AddEntityToTracker(entity, 64, 10, false);


                //AddEntityToTracker(entity, 64, 20, true); // item
            }
        }

        /// <summary>
        /// Добавить сущность в трек
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="trackingRange">Пороговое значение расстояния отслеживания</param>
        /// <param name="updateFrequency">Частота проверки синхронизации</param>
        /// <param name="sendVelocityUpdates">Отправлять обновления скорости</param>
        private void AddEntityToTracker(EntityBase entity, int trackingRange, int updateFrequency, bool sendVelocityUpdates)
        {
            if (trackingRange > maxTrackingDistanceThreshold)
            {
                trackingRange = maxTrackingDistanceThreshold;
            }

            try
            {
                if (trackedEntities.ContainsId(entity.Id))
                {
                    World.Log.Log("EntityTracker: Сущность уже отслеживается!");
                    return;
                }

                EntityTrackerEntry trackerEntry = new EntityTrackerEntry(entity, trackingRange, updateFrequency, sendVelocityUpdates);
                trackedEntities.Add(trackerEntry);
                trackerEntry.UpdatePlayerEntities(World.PlayerEntities);
            }
            catch (Exception ex)
            {
                World.Log.Error("EntityTracker: Обнаружение ошибки отслеживания объекта: " + ex.Message);
            }
        }

        /// <summary>
        /// Убрать трек с этой сущностью
        /// </summary>
        public void UntrackEntity(EntityBase entity)
        {
            EntityTrackerEntry trackerEntry;

            if (entity is EntityPlayerServer entityPlayer)
            {
                for (int i = 0; i < trackedEntities.Count; i++)
                {
                    trackerEntry = trackedEntities.GetAt(i);
                    if (trackerEntry != null)
                    {
                        trackerEntry.RemoveTrackedPlayerSymmetric(entityPlayer);
                    }
                }
            }

            trackerEntry = trackedEntities.Get(entity.Id);

            if (trackerEntry != null)
            {
                trackedEntities.Remove(trackerEntry);
                trackerEntry.SendDestroyEntityPacketToTrackedPlayers();
            }
        }

        /// <summary>
        /// Удалить игрока из трекеров 
        /// </summary>
        /// <param name="entityPlayer">сущность игрока</param>
        public void RemovePlayerFromTrackers(EntityPlayerServer entityPlayer)
        {
            for (int i = 0; i < trackedEntities.Count; i++)
            {
                EntityTrackerEntry trackerEntry = trackedEntities.GetAt(i);
                if (trackerEntry != null)
                {
                    trackerEntry.RemoveTrackedPlayerSymmetric(entityPlayer);
                }
            }
        }

        /// <summary>
        /// Обновить все отслеживаемые сущности
        /// </summary>
        public void UpdateTrackedEntities()
        {
            // массив игроков
            List<EntityPlayerServer> entities = new List<EntityPlayerServer>();
            
            for (int i = 0; i < trackedEntities.Count; i++)
            {
                EntityTrackerEntry trackerEntry = trackedEntities.GetAt(i);
                if (trackerEntry != null)
                {
                    trackerEntry.UpdatePlayerList(World.PlayerEntities);

                    if (trackerEntry.PlayerEntitiesUpdated && trackerEntry.TrackedEntity is EntityPlayerServer)
                    {
                        entities.Add((EntityPlayerServer)trackerEntry.TrackedEntity);
                    }
                }
            }

            for (int i = 0; i < entities.Count; i++)
            {
                EntityPlayerServer entityPlayer = entities[i];

                for (int j = 0; j < trackedEntities.Count; j++)
                {
                    EntityTrackerEntry trackerEntry = trackedEntities.GetAt(j);
                    if (trackerEntry != null && trackerEntry.TrackedEntity != entityPlayer)
                    {
                        trackerEntry.UpdatePlayerEntity(entityPlayer);
                    }
                }
            }
        }

        //public void func_180245_a(EntityPlayerServer entityPlayer)
        //{
        //    for (int i = 0; i < trackedEntities.Count; i++)
        //    {
        //        EntityTrackerEntry trackerEntry = trackedEntities.GetAt(i);
        //        if (trackerEntry != null)
        //        {
        //            if (trackerEntry.TrackedEntity == entityPlayer)
        //            {
        //                trackerEntry.UpdatePlayerEntities(World.PlayerEntities);
        //            } else
        //            {
        //                trackerEntry.UpdatePlayerEntity(entityPlayer);
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Отправить всем отслеживаемым игрока пакет, кроме тикущей
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="packet">пакет</param>
        public void SendToAllTrackingEntity(EntityLiving entity, IPacket packet)
        {
            EntityTrackerEntry entityTracker = trackedEntities.Get(entity.Id);
            if (entityTracker != null)
            {
                entityTracker.SendPacketPlayers(packet);
            }
        }

        /// <summary>
        /// Отправить всем отслеживаемым игрока пакет, с тикущей
        /// </summary>
        /// <param name="entity">сущность</param>
        /// <param name="packet">пакет</param>
        public void SendToAllTrackingEntityCurrent(EntityLiving entity, IPacket packet)
        {
            EntityTrackerEntry entityTracker = trackedEntities.Get(entity.Id);
            if (entityTracker != null)
            {
                entityTracker.SendPacketPlayersCurrent(packet);
            }
        }

        public override string ToString()
        {
            string list = "";
            for (int i = 0; i < trackedEntities.Count; i++)
            {
                list += "[" + trackedEntities.GetAt(i).ToString() + "] ";
            }
            return string.Format("Tracker: {0} {1}", trackedEntities.Count, list);
        }


        //public void func_85172_a(EntityPlayerMP p_85172_1_, Chunk p_85172_2_)
        //{
        //    Iterator var3 = this.trackedEntities.iterator();

        //    while (var3.hasNext())
        //    {
        //        EntityTrackerEntry var4 = (EntityTrackerEntry)var3.next();

        //        if (var4.trackedEntity != p_85172_1_ && var4.trackedEntity.chunkCoordX == p_85172_2_.xPosition && var4.trackedEntity.chunkCoordZ == p_85172_2_.zPosition)
        //        {
        //            var4.updatePlayerEntity(p_85172_1_);
        //        }
        //    }
        //}
    }       
}
