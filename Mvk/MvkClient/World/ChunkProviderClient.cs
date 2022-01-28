using MvkClient.Renderer.Chunk;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.World.Chunk;
using System.Collections;
using System.Collections.Generic;

namespace MvkClient.World
{
    /// <summary>
    /// Объект Клиент который хранит и отвечает за кэш чанков
    /// </summary>
    public class ChunkProviderClient : ChunkProvider
    {
        /// <summary>
        /// Клиентский объект мира
        /// </summary>
        public WorldClient ClientWorld { get; protected set; }

        /// <summary>
        /// Список чанков Для удаления сетки основного потока
        /// </summary>
        private List<ChunkRender> remoteMeshChunks = new List<ChunkRender>();
        /// <summary>
        /// Список чанков на удаление
        /// </summary>
        private List<ChunkRender> remoteChunks = new List<ChunkRender>();
        /// <summary>
        /// Список чанков на дабавление
        /// </summary>
        private List<ChunkRender> addChunks = new List<ChunkRender>();
        /// <summary>
        /// Перезапуск чанки по итоку удаления (смена обзора к примеру)
        /// </summary>
        private bool isResetLoadingChunk = false;

        public ChunkProviderClient(WorldClient worldIn)
        {
            ClientWorld = worldIn;
            world = worldIn;
        }

        /// <summary>
        /// Очистить все чанки, ТОЛЬКО для клиента
        /// </summary>
        /// <param name="reset">надо ли перезапустить чанки по итоку удаления (смена обзора к примеру)</param>
        public void ClearAllChunks(bool reset)
        {
            isResetLoadingChunk = reset;
            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkRender chunk in ht.Values)
            {
                UnloadChunk(chunk);
            }
        }

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        //public override void RemoveChunk(vec2i pos) => chunkMapping.Remove(pos);

        /// <summary>
        /// Загрузить, если нет такого создаём для клиента
        /// </summary>
        public ChunkRender GetChunkRender(vec2i pos, bool isCreate)
        {
            if (!(chunkMapping.Get(pos) is ChunkRender chunk))
            {
                //if (isCreate)
                //{
                //    chunk = new ChunkRender((WorldClient)world, pos);
                //    addChunks.Add(chunk);
                //    //chunkMapping.Set(chunk);
                //    return chunk;
                //}
                return null;
            }
            return chunk;
        }

        public void AddChunck(PacketS21ChunckData packet)
        {
            ChunkRender chunk = new ChunkRender(ClientWorld, packet.GetPos());

            if (packet.GetBuffer().Length == 0 || packet.GetHeight() == 0)
            {
                return;
            }
            //ClientMain.World.ChunkPrClient.GetChunkRender(packet.GetPos(), true);
            chunk.SetBinary(packet.GetBuffer(), packet.GetHeight());
            chunk.ModifiedToRender();
            addChunks.Add(chunk);
        }

        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(vec2i pos) => UnloadChunk((ChunkRender)GetChunk(pos));
        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkRender chunk)
        {
            if (chunk != null)
            {
                if (chunk.IsChunkLoaded)
                {
                    chunk.ChunkUnload();
                    // заносим в массив чистки чанков по сетки для основного потока
                    remoteMeshChunks.Add(chunk);
                }
                else
                {
                    remoteChunks.Add(chunk);
                }
            }
        }

        /// <summary>
        /// Выгрузка загрузка чанков в тике
        /// </summary>
        public void ChunksTickUnloadLoad()
        {
            // Выгрузка чанков, не больше 100 шт за такт
            int count = 100;
            while (remoteChunks.Count > 0 && count > 0)
            {
                chunkMapping.Remove(remoteChunks[0].Position);
                remoteChunks.RemoveAt(0);
                count--;
            }
            if (isResetLoadingChunk && remoteChunks.Count == 0)
            {
                isResetLoadingChunk = false;
                //ClientWorld.Player.UpFrustumCulling();
                ClientWorld.ClientMain.TrancivePacket(new PacketC13ClientSetting(ClientWorld.Player.OverviewChunk));
            }
            // Загрузка
            count = 50;
            while (addChunks.Count > 0 && count > 0)
            {
                chunkMapping.Set(addChunks[0]);
                addChunks.RemoveAt(0);
                count--;
            }
        }

        /// <summary>
        /// Чистка сетки опенгл
        /// </summary>
        public void RemoteMeshChunks()
        {
            long time = Client.Time();
            long timeNew = time;
            // 4 мc на чистку чанков
            while (remoteMeshChunks.Count > 0 && timeNew - time <= 4)
            {
                if (!remoteMeshChunks[0].IsChunkLoaded)
                {
                    remoteMeshChunks[0].MeshDelete();
                }
                remoteChunks.Add(remoteMeshChunks[0]);
                remoteMeshChunks.RemoveAt(0);
                timeNew = Client.Time();
            }
        }

        /// <summary>
        /// Перепроверить чанки игроков в попадание в обзоре, если нет, убрать
        /// для клиента
        /// Tick
        /// </summary>
        public void FixOverviewChunk(EntityPlayer entity)
        {
            // дополнительно к обзору для кэша из-за клона обработки, разных потоков
            int additional = 6;
            vec2i chunkCoor = entity.GetChunkPos();
            vec2i min = chunkCoor - (entity.OverviewChunk + additional);
            vec2i max = chunkCoor + (entity.OverviewChunk + additional);

            Hashtable ht = chunkMapping.CloneMap();
            foreach (ChunkRender chunk in ht.Values)
            {
                if (chunk.Position.x < min.x || chunk.Position.x > max.x || chunk.Position.y < min.y || chunk.Position.y > max.y)
                {
                    UnloadChunk(chunk);
                }
            }
        }

        public override string ToString()
        {
            return "Ch:" + chunkMapping.Count + " RM:" + remoteMeshChunks.Count + " R:" + remoteChunks.Count + " A:" + addChunks.Count;
        }
    }
}
