using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.World.Chunk;
using System;
using System.Collections.Generic;
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
        /// Cписок, содержащий все чанки которые нужны клиенту согласно его обзору
        /// </summary>
        public List<vec2i> LoadedChunks { get; protected set; } = new List<vec2i>();

        public EntityPlayerServer(Server server, Socket socket, string name)
        {
            ServerMain = server;
            SocketClient = socket;
            Name = name;
            UUID = GetHash(name);
        }

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

            if (LoadedChunks.Count > 0)
            {

                int i = 0;
                List<vec2i> list = new List<vec2i>(LoadedChunks);

                foreach (vec2i pos in list)
                {
                    LoadedChunks.Remove(pos);
                    ChunkBase chunk = ServerMain.World.ChunkPr.LoadGenChunk(pos);
                    if (chunk != null)
                    {
                        ServerMain.ResponsePacket(SocketClient, new PacketS21ChunckData(chunk));
                        i++;
                        if (i > 10)
                            break;
                    }
                }

                
            }

        }

    }
}
