using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using MvkServer.World.Chunk;
using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace MvkServer.Entity.Player
{
    /// <summary>
    /// Сущность игрока для сервера
    /// </summary>
    public class EntityPlayerServer : EntityPlayer
    {
        /// <summary>
        /// Сетевой сокет клиента
        /// </summary>
        public Socket SocketClient { get; protected set; }
        /// <summary>
        /// Основной сервер
        /// </summary>
        public Server ServerMain { get; protected set; }
        /// <summary>
        /// Обзор чанков
        /// </summary>
        public int OverviewChunk { get; protected set; } = MvkGlobal.OVERVIEW_CHUNK_START;

        /// <summary>
        /// Cписок, содержащий все чанки которые нужны клиенту согласно его обзору для загрузки
        /// </summary>
        public MapList LoadedChunks { get; protected set; } = new MapList();

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name)
        {
            ServerMain = server;
            SocketClient = socket;
            Name = name;
            UUID = GetHash(name);
        }

        /// <summary>
        /// Задать обзор чанков у клиента
        /// </summary>
        public void SetOverviewChunk(int overviewChunk) => OverviewChunk = overviewChunk;

        /// <summary>
        /// Получить хэш по строке
        /// </summary>
        protected string GetHash(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
        }

        /// <summary>
        /// Вызывается для обновления позиции / логики объекта
        /// </summary>
        public override void Update() // Пока не знаю где вызывать EntityPlayerMP.onUpdate()
        {
            // по LoadedChunks надо отправлять запрос пакета для сервера для чанков, при этом не более 10 чанков за раз
            try
            {
                int i = 0;
                while (LoadedChunks.Count > 0 && i < 10)
                {
                    vec2i pos = LoadedChunks.FirstRemove();
                    ChunkBase chunk = ServerMain.World.ChunkPr.LoadChunk(pos);

                    if (chunk != null)
                    {
                        ServerMain.ResponsePacket(SocketClient, new PacketS21ChunckData(chunk));
                        i++;
                    }
                }
            }
            catch (Exception e)
            {
                ServerMain.Log.Error("EntityPlayerServer.Update {0}", e.Message);
                throw;
            }
        }
    }
}
