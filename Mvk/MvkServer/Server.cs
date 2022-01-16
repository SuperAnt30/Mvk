using MvkServer.Entity.Player;
using MvkServer.Glm;
using MvkServer.Network;
using MvkServer.Network.Packets;
using MvkServer.Util;
using MvkServer.World;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace MvkServer
{
    public class Server
    {
        
        /// <summary>
        /// Объект лога
        /// </summary>
        public Logger Log { get; protected set; }
        /// <summary>
        /// Указывает, запущен сервер или нет. Установите значение false, чтобы инициировать завершение работы. 
        /// </summary>
        public bool ServerRunning { get; protected set; } = true;
        /// <summary>
        /// Сервер уже в рабочем цикле
        /// </summary>
        public bool ServerIsInRunLoop { get; protected set; } = false;
        /// <summary>
        /// Указывает другим классам, что сервер безопасно остановлен 
        /// </summary>
        public bool ServerStopped { get; protected set; } = false;
        /// <summary>
        /// Увеличивается каждый тик 
        /// </summary>
        public uint TickCounter { get; protected set; } = 0;
        /// <summary>
        /// Серверный объект мира
        /// </summary>
        public WorldServer World { get; protected set; }

        /// <summary>
        /// Устанавливается при появлении предупреждения «Не могу угнаться», которое срабатывает снова через 15 секунд. 
        /// </summary>
        protected long timeOfLastWarning;
        /// <summary>
        /// Хранение тактов за 1/5 секунды игры, для статистики определения среднего времени такта
        /// </summary>
        protected long[] tickTimeArray = new long[4];
        protected int tickRx = 0;
        protected int tickTx = 0;
        /// <summary>
        /// статус запуска сервера
        /// </summary>
        protected string strNet = "";
        /// <summary>
        /// Часы для Tps
        /// </summary>
        protected Stopwatch stopwatchTps = new Stopwatch();
        /// <summary>
        /// Для перевода тактов в мили секунды Stopwatch.Frequency / 1000;
        /// </summary>
        protected long frequencyMs;
        /// <summary>
        /// Сокет для сети
        /// </summary>
        protected SocketServer server;
        /// <summary>
        /// Объект работы с пакетами
        /// </summary>
        protected ProcessServerPackets packets;
        /// <summary>
        /// Пауза в игре, только для одиночной версии
        /// </summary>
        protected bool isGamePaused = false;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <returns>вёрнуть цифру тактов загрузки</returns>
        public void Initialize(int slot)
        {
            Log = new Logger("");
            Log.Log("server.runing slot={0}", slot);
            World = new WorldServer(this);
            packets = new ProcessServerPackets(this);
            frequencyMs = Stopwatch.Frequency / 1000;
            stopwatchTps.Start();
            // Отправляем основному игроку пинг
            ResponsePacket(null, new PacketS10Connection(""));
        }

        #region Net

        /// <summary>
        /// Получить истину запущен ли сетевой сервер
        /// </summary>
        public bool IsRunNet() => server != null && server.IsConnected;

        /// <summary>
        /// Запустить на сервере сеть
        /// </summary>
        public void RunNet()
        {
            if (!IsRunNet())
            {
                server = new SocketServer(32021);
                server.ReceivePacket += (sender, e) => LocalReceivePacket(e.Packet.WorkSocket, e.Packet.Bytes);
                server.Receive += Server_Receive;
                server.Run();
                isGamePaused = false;
                Log.Log("server.run.net");
            }
        }

        private void Server_Receive(object sender, ServerPacketEventArgs e)
        {
            if (e.Packet.Status == StatusNet.Disconnect)
            {
                World.Players.PlayerRemove(e.Packet.WorkSocket);
            }
            else if (e.Packet.Status == StatusNet.Connect)
            {
                // Отправляем игроку пинг
                ResponsePacket(e.Packet.WorkSocket, new PacketS10Connection(""));
            }
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(Socket socket, byte[] buffer)
        {
            tickRx++;
            packets.ReceiveBufferServer(socket, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients() => strNet = IsRunNet() ? "net[" + World.Players.PlayerCount + "]" : "";

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket(Socket socket, IPacket packet)
        {
            using (MemoryStream writeStream = new MemoryStream())
            {
                using (StreamBase stream = new StreamBase(writeStream))
                {
                    writeStream.WriteByte(ProcessPackets.GetId(packet));
                    packet.WritePacket(stream);
                    byte[] buffer = writeStream.ToArray();
                    tickTx++;
                    ServerPacket spacket = new ServerPacket(socket, buffer);
                    if (socket != null)
                    {
                        server.SetActiveSocket(socket);
                        server.SendPacket(buffer);
                    }
                    else
                    {
                        OnRecievePacket(new ServerPacketEventArgs(spacket));
                    }
                }
            }
        }

        #endregion

        

        

        /// <summary>
        /// Запрос остановки сервера
        /// </summary>
        public void ServerStop() => ServerRunning = false;

        /// <summary>
        /// Метод должен работать в отдельном потоке
        /// </summary>
        public void ServerLoop()
        {
            try
            {
                StartServerLoop();
                long currentTime = stopwatchTps.ElapsedMilliseconds;
                long cacheTime = 0;
                Log.Log("server.runed");

                World.Players.LoginStart();

                // Рабочий цикл сервера
                while (ServerRunning)
                {
                    long realTime = stopwatchTps.ElapsedMilliseconds;
                    // Разница в милсекунда с прошлого такта
                    long differenceTime = realTime - currentTime;
                    if (differenceTime < 0) differenceTime = 0;

                    // Если выше 2 секунд задержка
                    if (differenceTime > 2000 && currentTime - timeOfLastWarning >= 15000)
                    {
                        // Не успеваю!Изменилось ли системное время, или сервер перегружен?
                        // Отставание на {differenceTime} мс, пропуск тиков({differenceTime / 50}) 
                        Log.Log("Не успеваю! Отставание на {0} мс, пропуск тиков {1}", differenceTime, differenceTime / 50);
                        differenceTime = 2000;
                        timeOfLastWarning = currentTime;
                    }

                    cacheTime += differenceTime;
                    currentTime = realTime;

                    
                    //if (false)
                    //{
                    //    // проверка игроков, что все спят
                    //    // Почему-то если спят, то отрабатывать накопленные такты не надо
                    //    Tick();
                    //    cacheTime = 0;
                    //}
                    //else
                    {
                        while (cacheTime > 50)
                        {
                            cacheTime -= 50;
                            if (!isGamePaused) Tick();
                        }
                    }

                    Thread.Sleep(Mth.Max(1, 50 - (int)cacheTime));
                    ServerIsInRunLoop = true;
                }
            }
            catch
            {
                // ошибки в сервере для краша
            }
            finally
            {
                StopServerLoop();
            }
        }

        /// <summary>
        /// Запустить сервер в отдельном потоке
        /// </summary>
        public void StartServer()
        {
            Thread myThread = new Thread(ServerLoop);
            myThread.Start();
        }

        protected void StartServerLoop()
        {
            ServerRunning = true;

            EntityPlayerServer entityPlayer = World.Players.GetEntityPlayerMain();
            vec2i pos = entityPlayer != null ? entityPlayer.GetChunkPos() : new vec2i(0, 0);

            int radius = MvkGlobal.OVERVIEW_CHUNK_START;
            OnLoadStepCount((radius + radius + 1) * (radius + radius + 1));

            // Запуск чанков для старта
            for (int x = -radius; x <= radius; x++)
            {
                for (int z = -radius; z <= radius; z++)
                {
                    World.ChunkPrServ.LoadChunk(new vec2i(pos.x + x, pos.y + z));
                    OnLoadingTick();
                }
            }
            return;
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        protected void StopServerLoop()
        {
            Log.Log("server.stoping");

            World.Players.PlayerClear();

            // тут будет сохранение мира
            //Thread.Sleep(100);
            if (server != null) server.Stop();
            OnLogDebug("");
            ServerStopped = true;
            Log.Log("server.stoped");
            Log.Close();
            OnStoped();
        }

        /// <summary>
        /// Основная функция вызывается run () в каждом цикле. 
        /// </summary>
        protected void Tick()
        {
            long realTime = stopwatchTps.ElapsedTicks;
            TickCounter++;

            // Выполнение такта

            //Random r = new Random();
            //int rn = r.Next(100);
            //Thread.Sleep(rn);


            // Прошла секунда, или 20 тактов
            if (TickCounter % 20 == 0)
            {
                UpCountClients();

                World.Players.ResponsePacketAll(new PacketS14TimeUpdate(TickCounter));
                
                //this.serverConfigManager.sendPacketToAllPlayersInDimension(new S03PacketTimeUpdate(
                //var4.getTotalWorldTime(), 
                //var4.getWorldTime(), 
                //var4.getGameRules().getGameRuleBooleanValue("doDaylightCycle")),

                //var4.provider.getDimensionId());
            }

            try
            {
                World.Tick();
                World.Players.UpdatePlayerInstances();
            }
            catch (Exception e)
            {
                Log.Error("Server.Tick {0}", e.Message);
                throw;
            }

            // ---------------
            long differenceTime = stopwatchTps.ElapsedTicks - realTime;

            // Прошла 1/5 секунда, или 4 такта
            if (TickCounter % 4 == 0)
            {
                // лог статистика за это время
                OnLogDebug(ToStringDebugTps());

                if (MvkGlobal.IS_DRAW_DEBUG_CHUNK)
                {
                    // отладка чанков
                    DebugChunk chunks = new DebugChunk()
                    {
                        listChunkServer = World.ChunkPr.GetListDebug(),
                        listChunkPlayers = World.Players.GetListDebug(),
                        isRender = true
                    };
                    OnLogDebugCh(chunks);
                }

                tickRx = 0;
                tickTx = 0;
            }

            // фиксируем время выполнения такта
            tickTimeArray[TickCounter % 4] = differenceTime;
        }

        /// <summary>
        /// Задать паузу для одиночной игры
        /// </summary>
        public void SetGamePauseSingle(bool value)
        {
            isGamePaused = !IsRunNet() && value;
            OnLogDebug(ToStringDebugTps());
        }

        /// <summary>
        /// Строка для дебага, формируется по запросу
        /// </summary>
        protected string ToStringDebugTps()
        {
            //TODO:: дебаг для сервера
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(tickTimeArray) / frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > 50f ? 50f / averageTime * 20f : 20f;
            return string.Format("tps {0:0.00} tick {1:0.00} ms Rx {2} Tx {3} {4}{6}\r\n{5}", 
                tps, averageTime, tickRx, tickTx, strNet, World.ToStringDebug(), isGamePaused ? "PAUSE" : "");
        }

        #region Event

        /// <summary>
        /// Событие такта для объекта с ключом
        /// </summary>
        public event EventHandler LoadingTick;
        protected virtual void OnLoadingTick() => LoadingTick?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие количество шигов загрузки
        /// </summary>
        public event IntEventHandler LoadStepCount;
        protected virtual void OnLoadStepCount(int count) => LoadStepCount?.Invoke(this, new IntEventArgs(count));
        /// <summary>
        /// Событие остановки сервера
        /// </summary>
        public event EventHandler Stoped;
        protected virtual void OnStoped() => Stoped?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие лог для дебага
        /// </summary>
        public event StringEventHandler LogDebug;
        protected virtual void OnLogDebug(string text) => LogDebug?.Invoke(this, new StringEventArgs(text));
        /// <summary>
        /// Событие лог для дебага листа чанков
        /// </summary>
        public event ObjectEventHandler LogDebugCh;
        protected virtual void OnLogDebugCh(DebugChunk list) => LogDebugCh?.Invoke(this, new ObjectEventArgs(list));

        /// <summary>
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        protected void OnRecievePacket(ServerPacketEventArgs e) => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
