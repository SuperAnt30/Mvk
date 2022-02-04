using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Renderer.Entity;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer;
using MvkServer.Entity;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace MvkClient.World
{
    /// <summary>
    /// Клиентский объект мира
    /// </summary>
    public class WorldClient : WorldBase
    {
        /// <summary>
        /// Основной клиент
        /// </summary>
        public Client ClientMain { get; protected set; }
        /// <summary>
        /// Объект клиента
        /// </summary>
        public EntityPlayerSP Player { get; protected set; }
        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public ChunkProviderClient ChunkPrClient => ChunkPr as ChunkProviderClient;
        /// <summary>
        /// Мир для рендера и прорисовки
        /// </summary>
        public WorldRenderer WorldRender { get; protected set; }
        /// <summary>
        /// Менеджер прорисовки сущностей
        /// </summary>
        public RenderManager RenderEntityManager { get; protected set; }
        /// <summary>
        /// Список сущностей игроков
        /// </summary>
        public Hashtable PlayerEntities { get; protected set; } = new Hashtable();
        /// <summary>
        /// Объект нажатия клавиатуры
        /// </summary>
        public Keyboard Key { get; protected set; }

        /// <summary>
        /// Объект времени c последнего тпс
        /// </summary>
        protected InterpolationTime interpolation = new InterpolationTime();
        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;
        /// <summary>
        /// Перечень игроков, отладка
        /// </summary>
        protected string strPlayers = "";
        /// <summary>
        /// Количество прорисованных сущностей, для отладки
        /// </summary>
        protected int entitiesCountShow = 0;
        /// <summary>
        /// Объект заглушка
        /// </summary>
        private object locker = new object();

        public WorldClient(Client client) : base()
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
            interpolation.Start();
            WorldRender = new WorldRenderer(this);
            RenderEntityManager = new RenderManager(this);
            Player = new EntityPlayerSP(this);
            Player.SetOverviewChunk(Setting.OverviewChunk, 0);
            Key = new Keyboard(this);
            UpStrPlayers();
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            try
            {
                interpolation.Restart();
                uint time = ClientMain.TickCounter;

                base.Tick();

                // Обновить игрока
                Player.Update();
                
                if (Player.IsDead && !ClientMain.Screen.IsScreenGameOver())
                {
                    // GameOver
                    ClientMain.SetScreen(ObjectKey.GameOver, "Boom");
                }

                // Обновить остальных сущностей
                List<EntityPlayerMP> entitiesLook = new List<EntityPlayerMP>();
                Hashtable pe = PlayerEntities.Clone() as Hashtable;
                foreach (EntityPlayerMP entity in pe.Values)
                {
                    entity.Update();
                    if (entity.IsRayEye) entitiesLook.Add(entity);
                }
                Player.SetEntitiesLook(entitiesLook);

                if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
                {
                    previousTotalWorldTime = time;
                    ChunkPrClient.FixOverviewChunk(Player);
                }

                // Выгрузка чанков в тике
                ChunkPrClient.ChunksTickUnloadLoad();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// Проверить загружены ли все ближ лижащие чанки кроме центра
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        public bool IsChunksSquareLoaded(vec2i pos)
        {
            for (int i = 0; i < MvkStatic.AreaOne8.Length; i++)
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(pos + MvkStatic.AreaOne8[i]);
                if (chunk == null || !chunk.IsChunkLoaded) return false;
            }
            return true; 
        }

        /// <summary>
        /// Остановка мира, удаляем все элементы
        /// </summary>
        public void StopWorldDelete()
        {
            ChunkPrClient.ClearAllChunks(false);
        }

        /// <summary>
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// где 0 это начало, 1 финиш
        /// </summary>
        public float TimeIndex() => interpolation.TimeIndex();

        /// <summary>
        /// Получить объект игрока по сети, по имени
        /// </summary>
        public EntityPlayerMP GetPlayerMP(ushort id)
        {
            if (PlayerEntities.ContainsKey(id))
            {
                return PlayerEntities[id] as EntityPlayerMP;
            }
            return null;
        }

        /// <summary>
        /// Заменить объект игрока по сети
        /// </summary>
        public void SetPlayerMP(EntityPlayerMP entity)
        {
            if (!PlayerEntities.ContainsKey(entity.Id))
            {
                PlayerEntities.Add(entity.Id, entity);
            }
            else
            {
                PlayerEntities[entity.Id] = entity;
            }
            UpStrPlayers();
        }

        /// <summary>
        /// Удалить игрока с базы
        /// </summary>
        public void RemovePlayerMP(ushort id)
        {
            lock(locker) PlayerEntities.Remove(id);
            UpStrPlayers();
        }

        public void MouseDown(MouseButton button)
        {
            if (button == MouseButton.Left)
            {
                vec3 pos = Player.GetPositionFrame();
                pos.y += Player.GetEyeHeightFrame();
                vec3 dir = Player.RayLook;//.GetLookFrame();
               
                MovingObjectPosition moving = RayCast(pos, dir, 10f);
                MovingObjectPosition movingE = RayCastEntity();

                Player.Action();
                //Stopwatch stopwatch0 = new Stopwatch();
                //stopwatch0.Start();

                //int j = 0;
                //float fj = 0f;
                //for (int i = 0; i < 40000000; i++) fj = fj / 2.7f * i; //j++;

                //float tf = (stopwatch0.ElapsedTicks / (float)Stopwatch.Frequency) * 1000f;
                // луч
                Debug.DStr = moving.ToString() + "\r\n" + movingE.ToString(); // + " --" + string.Format("{0:0.00}",tf);

            }
        }

        /// <summary>
        /// Получить попадает ли в луч сущность, выбрать самую близкую
        /// </summary>
        public MovingObjectPosition RayCastEntity()
        {
            MovingObjectPosition moving = new MovingObjectPosition();
            if (Player.EntitiesLook.Length > 0)
            {
                EntityPlayerMP[] entities = Player.EntitiesLook.Clone() as EntityPlayerMP[];
                vec3 pos = Player.GetPositionFrame();
                float dis = 1000f;
                foreach (EntityPlayerMP entity in entities)
                {
                    float disR = glm.distance(pos, entity.GetPositionFrame(entity.TimeIndex()));
                    if (dis > disR)
                    {
                        dis = disR;
                        moving = new MovingObjectPosition(entity);
                    }
                }
            }
            return moving;
        }

        /// <summary>
        /// Обновить перечень игроков, отладка
        /// </summary>
        protected void UpStrPlayers()
        {
            strPlayers = "[" + PlayerEntities.Count + "] ";
            if (PlayerEntities.Count > 0)
            {
                foreach (EntityPlayerMP entity in PlayerEntities.Values)
                {
                    strPlayers += entity.Name + " ";
                }
            }
        }

        #region Debug

        public void CountEntitiesShowBegin() => entitiesCountShow = 0;
        public void CountEntitiesShowAdd() => entitiesCountShow++;

        #endregion

        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("t {2} {0} E:{4}/{5}\r\n{3} {1}",
                ChunkPrClient.ToString(), Player, ClientMain.TickCounter / 20, 
                strPlayers, PlayerEntities.Count + 1, entitiesCountShow);
        }
    }
}
