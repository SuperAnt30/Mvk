using MvkServer.Network;
using MvkServer.Util;
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
        public int TickCounter { get; protected set; } = 0;
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
        /// test
        /// </summary>
        protected int countLoading = 64;

        /// <summary>
        /// Инициализация
        /// </summary>
        /// <returns>вёрнуть цифру тактов загрузки</returns>
        public int Initialize()
        {
            packets = new ProcessServerPackets(this);
            frequencyMs = Stopwatch.Frequency / 1000;
            stopwatchTps.Start();
            return countLoading;
        }

        #region Net

        /// <summary>
        /// Получить истину запущен ли сервер
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
                server.Run();
            }
        }

        /// <summary>
        /// Локальная передача пакета
        /// </summary>
        public void LocalReceivePacket(Socket socket, byte[] buffer)
        {
            tickRx++;
            packets.LocalReceivePacket(socket, buffer);
        }

        /// <summary>
        /// Обновить количество клиентов
        /// </summary>
        public void UpCountClients() => strNet = IsRunNet() ? "net[" + (server.Clients().Length + 1) + "]" : "";

        /// <summary>
        /// Отправить пакет клиенту
        /// </summary>
        public void ResponsePacket(Socket socket, IPacket packet)
        {
            using (MemoryStream writeStream = new MemoryStream())
            {
                using (StreamBase stream = new StreamBase(writeStream))
                {
                    writeStream.WriteByte(packet.Id);
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
                StartServer();
                long currentTime = stopwatchTps.ElapsedMilliseconds;
                long cacheTime = 0;

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
                            Tick();
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
                StopServer();
            }
        }

        protected void StartServer()
        {
            // Тут запуск чанков для старта
            int speed = 5;
            for (int i = 0; i < countLoading; i++)
            {
                OnLoadingTick();
                Thread.Sleep(speed);
            }
            Thread.Sleep(5); // Тест пауза чтоб увидеть загрузчик
            // TODO:: вынести в опции открытия игры по сети
            RunNet();
            OnLoadingEnd();
        }

        /// <summary>
        /// Сохраняет все необходимые данные для подготовки к остановке сервера
        /// </summary>
        protected void StopServer()
        {
            // тут будет сохранение мира
            Thread.Sleep(50);
            if (server != null) server.Stop();
            OnLogDebug("");
            ServerStopped = true;
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
            }

            long differenceTime = stopwatchTps.ElapsedTicks - realTime;

            // Прошла 1/5 секунда, или 4 такта
            if (TickCounter % 4 == 0)
            {
                // лог статистика за это время
                OnLogDebug(ToStringDebugTps());
                tickRx = 0;
                tickTx = 0;
            }

            // фиксируем время выполнения такта
            tickTimeArray[TickCounter % 4] = differenceTime;
        }

        protected string ToStringDebugTps()
        {
            // Среднее время выполнения 4 тактов, должно быть меньше 50
            float averageTime = Mth.Average(tickTimeArray) / frequencyMs;
            // TPS за последние 4 тактов (1/5 сек), должен быть 20
            float tps = averageTime > 50f ? 50f / averageTime * 20f : 20f;
            return string.Format("tps {0:0.00} tick {1:0.00} ms Rx {2} Tx {3} {4}", tps, averageTime, tickRx, tickTx, strNet);
        }

        #region Event

        /// <summary>
        /// Событие такта для объекта с ключом
        /// </summary>
        public event EventHandler LoadingTick;
        protected virtual void OnLoadingTick() => LoadingTick?.Invoke(this, new EventArgs());
        /// <summary>
        /// Событие окончания загрузки
        /// </summary>
        public event EventHandler LoadingEnd;
        protected virtual void OnLoadingEnd() => LoadingEnd?.Invoke(this, new EventArgs());
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
        /// Событие получить от сервера пакет
        /// </summary>
        public event ServerPacketEventHandler RecievePacket;
        protected void OnRecievePacket(ServerPacketEventArgs e) => RecievePacket?.Invoke(this, e);

        #endregion
    }
}
