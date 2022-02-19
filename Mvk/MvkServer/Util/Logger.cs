using System;
using System.IO;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект фиксирующий лог
    /// </summary>
    public class Logger
    {
        private string fileName;
        private string log;
        private string path = "Logs" + Path.DirectorySeparatorChar;
        private static object locker = new object();
        private bool isEmpty = true;

        public Logger() { }

        public Logger(string prefixFileName)
        {
            isEmpty = false;
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
            if (isEmpty) return;
            
            log += $"[{DateTime.Now.ToLongTimeString()}] " + string.Format(logMessage, args) + "\r\n";
        }

        public void Error(string logMessage, params object[] args)
        {
            Log("[ERROR] " + logMessage, args);
        }

        /// <summary>
        /// В такте, раз в минуту для записи в файл
        /// </summary>
        public void Tick()
        {
            string logCache = log;
            log = "";
            Save(logCache);
        }

        protected void Save(string log)
        {
            if (log != "")
            {
                lock (locker)
                {
                    using (StreamWriter w = File.AppendText(path + fileName))
                    {
                        w.WriteAsync(log);
                    }
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
