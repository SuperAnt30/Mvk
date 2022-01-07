using System;
using System.Diagnostics;
using System.IO;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект фиксирующий лог
    /// </summary>
    public class Logger
    {
        protected string fileName;
        protected Stopwatch stopwatch;
        protected long time;
        protected string log;
        protected string path = "Logs" + Path.DirectorySeparatorChar;

        public Logger(string prefixFileName)
        {
            stopwatch = new Stopwatch();
            stopwatch.Start();
            string sd = DateTime.Now.ToString("yyyy-MM-dd");
            int i = 1;

            CheckPath(path);
            while (true)
            {
                fileName = string.Format("{2}{0}-{1}.txt", sd, i, prefixFileName);
                if (!File.Exists(path + fileName)) break;
                i++;
            }
        }

        public void Log(string logMessage, params object[] args)
        {
            log += $"[{DateTime.Now.ToLongTimeString()}] " + string.Format(logMessage, args) + "\r\n";
            if (time < stopwatch.ElapsedMilliseconds)
            {
                Save(log);
                log = "";
                // Таймер, чтоб чаще раз 5 секунд не записывать
                time = stopwatch.ElapsedMilliseconds + 10000;
            }
        }

        public void Error(string logMessage, params object[] args)
        {
            Log("[ERROR] " + logMessage, args);
        }

        protected void Save(string log)
        {
            if (log != "")
            {
                using (StreamWriter w = File.AppendText(path + fileName))
                {
                    w.WriteAsync(log);
                }
            }
        }

        /// <summary>
        /// Проверка пути, если нет, то создаём
        /// </summary>
        public static void CheckPath(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        /// <summary>
        /// Закрыть лог
        /// </summary>
        public void Close() => Save(log);
    }
}
