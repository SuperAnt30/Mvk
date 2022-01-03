using MvkClient.Actions;
using MvkClient.Entity;
using MvkClient.Renderer;
using MvkClient.Renderer.Chunk;
using MvkClient.Setitings;
using MvkClient.Util;
using MvkServer;
using MvkServer.Glm;
using MvkServer.Util;
using MvkServer.World;
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
        /// Объект времени c последнего тпс
        /// </summary>
        protected Stopwatch stopwatchTps = new Stopwatch();
        /// <summary>
        /// фиксатор чистки мира
        /// </summary>
        protected uint previousTotalWorldTime;

       

        public WorldClient(Client client)
        {
            ChunkPr = new ChunkProviderClient(this);
            ClientMain = client;
            stopwatchTps.Start();
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
            stopwatchTps.Restart();
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
            for (int i = 0; i < MvkStatic.AreaOne8.Length; i++)
            {
                ChunkRender chunk = ChunkPrClient.GetChunkRender(pos + MvkStatic.AreaOne8[i], false);
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
        /// Получить коэффициент времени от прошлого TPS клиента в диапазоне 0 .. 1
        /// где 0 это финиш, 1 начало
        /// </summary>
        public float TimeIndex()
        {
            float f = stopwatchTps.ElapsedTicks / (float)MvkStatic.TimerFrequencyTps;
            if (f > 1f) return 1f;
            if (f < 0) return 0;
            return f;
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
