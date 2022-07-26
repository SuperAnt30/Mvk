using MvkClient.Renderer.Chunk;
using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network.Packets.Client;
using MvkServer.Network.Packets.Server;
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
        /// Список всех видимых чанков аналог chunkMapping
        /// </summary>
        //private ChunkMap chunkListing = new ChunkMap();
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
            if (packet.IsRemoved())
            {
                lock (locker) UnloadChunk((ChunkRender)GetChunk(packet.GetPos()));
            }
            else
            {
                //if (Debug.DStart && !packet.GetPos().Equals(new vec2i(-1, -1))) return;

                ChunkRender chunk = GetChunkRender(packet.GetPos());
                bool isNew = chunk == null;
                if (isNew)
                {
                    chunk = new ChunkRender(ClientWorld, packet.GetPos());
                }

                chunk.SetBinary(packet.GetBuffer(), packet.IsBiom(), packet.GetFlagsYAreas());

                if (isNew)
                {
                    // Для нового чанка у клиента, генерируем высотную карту и осветляем тут же
                   // chunk.Light.GenerateHeightMapSky();
                    lock (locker) addChunks.Add(chunk);
                }
                else
                {
                    // Пометка для рендера
                    //ModifiedAreaOne(chunk.Position, packet.GetFlagsYAreas());
                }
                //chunk.Light.GenerateHeightMapSky();
                chunk.Light.GenerateHeightMap();
                //chunk.Light.ResetRelightChecks();
            }
        }

        /// <summary>
        /// Пометить для рендера соседние псевдо чанки
        /// </summary>
        private void ModifiedAreaOne(vec2i chPos, int flagsYAreas)
        {
            List<vec3i> list = new List<vec3i>();
            for (int sy = 0; sy < ChunkRender.COUNT_HEIGHT; sy++)
            {
                if ((flagsYAreas & 1 << sy) != 0)
                {
                    vec3i pos = new vec3i(chPos.x, sy, chPos.y);
                    //list.Add(pos);

                    vec3i pos2 = new vec3i(pos.x, pos.y + 1, pos.z);
                    if (!list.Contains(pos2)) list.Add(pos2);
                    pos2 = new vec3i(pos.x, pos.y - 1, pos.z);
                    if (!list.Contains(pos2)) list.Add(pos2);

                    list.Add(new vec3i(pos.x + 1, pos.y, pos.z));
                    list.Add(new vec3i(pos.x - 1, pos.y, pos.z));
                    list.Add(new vec3i(pos.x, pos.y, pos.z + 1));
                    list.Add(new vec3i(pos.x, pos.y, pos.z - 1));
                    //ClientWorld.AreaModifiedToRender(pos - 1, pos + 1);
                }
            }
            //foreach (vec3i pos in list)
            //{
            //    ClientWorld.ModifiedToRender(pos);
            //}
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
               // chunkListing.Remove(remoteChunks[0]);
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
              //  chunkListing.Set(addChunks[0]);
                vec2i pos = addChunks[0].Position;
                // надо соседние чанки попросить перерендерить

                ClientWorld.AreaModifiedToRender(pos.x - 1, 0, pos.y - 1, pos.x + 1, ChunkRender.COUNT_HEIGHT - 1, pos.y + 1);

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

        /// <summary>
        /// Сделать запрос на обновление близ лежащих псевдо чанков для альфа блоков
        /// </summary>
        /// <param name="x">координата чанка X</param>
        /// <param name="y">координата псевдо чанка Y</param>
        /// <param name="z">координата чанка Z</param>
        public void ModifiedToRenderAlpha(int x, int y, int z)
        {
            ChunkRender chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z));
            if (chunk != null)
            {
                chunk.ModifiedToRenderAlpha(y);
                chunk.ModifiedToRenderAlpha(y - 1);
                chunk.ModifiedToRenderAlpha(y + 1);
            }
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x + 1, z));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x - 1, z));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z + 1));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
            chunk = ClientWorld.ChunkPrClient.GetChunkRender(new vec2i(x, z - 1));
            if (chunk != null) chunk.ModifiedToRenderAlpha(y);
        }

        /**
     * Unloads chunks that are marked to be unloaded. This is not guaranteed to unload every such chunk.
     */
        //public bool UnloadQueuedChunks()
        //{
        //    long var1 = System.currentTimeMillis();

        //    Hashtable ht = chunkListing.CloneMap();
        //    foreach (ChunkRender chunk in ht.Values)
        //    {
        //        chunk.
        //        if ((reset && !chunk.Position.Equals(ClientWorld.ClientMain.Player.GetChunkPos())) || !reset)
        //        {
        //            UnloadChunk(chunk);
        //        }
        //    }


        //    Iterator var3 = chunkListing.iterator();

        //    while (var3.hasNext())
        //    {
        //        Chunk var4 = (Chunk)var3.next();
        //        var4.func_150804_b(System.currentTimeMillis() - var1 > 5L);
        //    }

        //    if (System.currentTimeMillis() - var1 > 100L)
        //    {
        //        logger.info("Warning: Clientside chunk ticking took {} ms", new Object[] { Long.valueOf(System.currentTimeMillis() - var1) });
        //    }

        //    return false;
        //}

        public override string ToString()
        {
            return "Ch:" + chunkMapping.Count + " RM:" + remoteMeshChunks.Count + " R:" + remoteChunks.Count + " A:" + addChunks.Count;
        }
    }
}
