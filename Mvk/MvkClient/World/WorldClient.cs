using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkServer;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;

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
        public EntityPlayerClient Player { get; protected set; }
        /// <summary>
        /// Посредник клиентоского чанка
        /// </summary>
        public ChunkProviderClient ChunkPrClient => ChunkPr as ChunkProviderClient;
        /// <summary>
        /// Мир для рендера и прорисовки
        /// </summary>
        public WorldRenderer WorldRender { get; protected set; }
        /// <summary>
        /// Объект управления клавиатурой
        /// </summary>
        public KeyboardLife KeyLife { get; protected set; }


        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;

        public WorldClient(Client client)
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
            WorldRender = new WorldRenderer(this);
            KeyLife = new KeyboardLife(this);
            Player = new EntityPlayerClient(this);
            Player.SetOverviewChunk(Setting.OverviewChunk, 0);
        }

        /// <summary>
        /// Обработка каждый тик
        /// </summary>
        public override void Tick()
        {
            base.Tick();
            uint time = ClientMain.TickCounter;

            if (time - previousTotalWorldTime > MvkGlobal.CHUNK_CLEANING_TIME)
            {
                previousTotalWorldTime = time;
                ChunkPrClient.FixOverviewChunk(Player);
            }

            Player.Update();
        }

        /// <summary>
        /// Проверить загружены ли все ближ лижащие чанки кроме центра
        /// </summary>
        /// <param name="pos">позиция чанка</param>
        public bool IsChunksSquareLoaded(vec2i pos)
        {
            for (int i = 0; i < ArrayStatic.AreaOne8.Length; i++)
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(pos + ArrayStatic.AreaOne8[i], false);
                if (chunk == null || !chunk.IsChunkLoaded) return false;
            }
            return true; 
        }

        /// <summary>
        /// Остановка мира, удаляем все элементы
        /// </summary>
        public void StopWorldDelete()
        {
            ChunkPrClient.ClearAllChunks();
        }
        /// <summary>
        /// Строка для дебага
        /// </summary>
        public override string ToStringDebug()
        {
            return string.Format("t {2} Ch {0} ChDel {3}\r\nXYZ {1}", ChunkPr.Count, Player.Position, ClientMain.TickCounter / 20, ChunkPrClient.RemoteMeshChunks.Count);
        }
    }
}
