using MvkClient.Renderer.Chunk;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Network.Packets.Client;
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
        private List<vec2i> remoteMeshChunks = new List<vec2i>();
        /// <summary>
        /// Список чанков на удаление
        /// </summary>
        private List<vec2i> remoteChunks = new List<vec2i>();
        /// <summary>
        /// Список чанков на дабавление
        /// </summary>
        private List<ChunkRender> addChunks = new List<ChunkRender>();
        /// <summary>
        /// Объект заглушка
        /// </summary>
        private object locker = new object();
        /// <summary>
        /// Перезапуск чанки по итоку удаления (смена обзора к примеру)
        /// </summary>
        private bool isResetLoadingChunk = false;

        public ChunkProviderClient(WorldClient worldIn)
        {
            ClientWorld = worldIn;
            world = worldIn;
        }

        public void SetOverviewChunk()
        {
            isResetLoadingChunk = true;
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
                if ((reset && !chunk.Position.Equals(ClientWorld.ClientMain.Player.GetChunkPos())) || !reset)
                {
                    UnloadChunk(chunk);
                }
            }
        }

        /// <summary>
        /// удалить чанк без сохранения
        /// </summary>
        //public override void RemoveChunk(vec2i pos) => chunkMapping.Remove(pos);

        /// <summary>
        /// Загрузить, если нет такого создаём для клиента
        /// </summary>
        public ChunkRender GetChunkRender(vec2i pos) => !(chunkMapping.Get(pos) is ChunkRender chunk) ? null : chunk;

        /// <summary>
        /// Заносим данные с пакета сервера
        /// </summary>
        public void PacketChunckData(PacketS21ChunkData packet)
        {
            if (packet.Status() == PacketS21ChunkData.EnumChunk.Remove)
            {
                lock (locker) UnloadChunk((ChunkRender)GetChunk(packet.GetPos()));
            }
            else
            {
                if (packet.Status() == PacketS21ChunkData.EnumChunk.All)
                {
                    ChunkRender chunk = new ChunkRender(ClientWorld, packet.GetPos());
                    chunk.SetBinary(packet.GetBuffer(), packet.GetHeight());
                    chunk.ModifiedToRender();
                    lock (locker) addChunks.Add(chunk);
                }
                else
                {
                    ChunkRender chunk = GetChunkRender(packet.GetPos());
                    if (chunk != null)
                    {
                        chunk.SetBinaryY(packet.GetBuffer(), packet.GetY());
                        chunk.ModifiedToRender(packet.GetY());
                    }
                }
            }
        }
        
        /// <summary>
        /// Выгрузить чанк
        /// </summary>
        public void UnloadChunk(ChunkRender chunk)
        {
            if (chunk != null)
            {
                vec2i pos = chunk.Position;
                if (chunk.IsChunkLoaded)
                {
                    chunk.OnChunkUnload();
                    // заносим в массив чистки чанков по сетки для основного потока
                    if (chunk != null)
                    {
                        remoteMeshChunks.Add(pos);
                    }
                }
                else
                {
                    remoteChunks.Add(pos);
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
                chunkMapping.Remove(remoteChunks[0]);
                remoteChunks.RemoveAt(0);
                count--;
            }
            if (isResetLoadingChunk && remoteChunks.Count == 0)
            {
                isResetLoadingChunk = false;
                addChunks.Clear();
                //ClientWorld.Player.UpFrustumCulling();
                ClientWorld.ClientMain.TrancivePacket(new PacketC15ClientSetting(ClientWorld.ClientMain.Player.OverviewChunk));
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
                ChunkRender chunk = GetChunkRender(remoteMeshChunks[0]);
                if (chunk != null)
                {
                    if (!chunk.IsChunkLoaded) chunk.MeshDelete();
                    remoteChunks.Add(remoteMeshChunks[0]);
                    remoteMeshChunks.RemoveAt(0);
                }
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
