using MvkServer.Glm;
using MvkServer.Network.Packets;
using MvkServer.Util;
using MvkServer.World;
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
        /// Cписок, содержащий все чанки которые нужны клиенту согласно его обзору для загрузки
        /// </summary>
        public MapList LoadedChunks { get; protected set; } = new MapList();

        // должен быть список чанков которые может видеть игрок
        // должен быть список чанков которые надо догрузить игроку

        public EntityPlayerServer(Server server, Socket socket, string name, WorldBase world) : base()
        {
            World = world; 
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
        public override void Update()
        {
            base.Update();
            try
            {
                if (isMotion)
                {
                    isMotion = false;
                    vec3 pos = Position + Motion;
                    SetPosition(pos);
                    if (isMotionHitbox)
                    {
                        isMotionHitbox = false;
                        ServerMain.ResponsePacket(SocketClient, new PacketB20Player().Position(pos, Hitbox.GetHeight(), Hitbox.GetEyes()));
                    }
                    else
                    {
                        ServerMain.ResponsePacket(SocketClient, new PacketB20Player().Position(pos));
                    }
                }

                int i = 0;
                while (LoadedChunks.Count > 0 && i < MvkGlobal.COUNT_PACKET_CHUNK_TPS)
                {
                    // передача пакетов чанков по сети
                    vec2i pos = LoadedChunks.FirstRemove();
                    ChunkBase chunk = ServerMain.World.ChunkPrServ.LoadChunk(pos);

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
