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
        private string path;
        //private static object locker = new object();
        private bool isEmpty = true;

        public Logger() { }
        public Logger(string prefixFileName) : this(prefixFileName, "Logs") { }
        public Logger(string prefixFileName, string folder)
        {
            path = folder + Path.DirectorySeparatorChar;
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
                try
                {
                    using (StreamWriter w = File.AppendText(path + fileName))
                    {
                        w.Write(log);
                    }
                }
                catch
                {
                    using (StreamWriter w = File.AppendText(path + "ERROR_" + fileName))
                    {
                        w.Write(log);
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


        /// <summary>
        /// Создать файл c ошибкой краша
        /// </summary>
        public static void Crach(string logMessage, params object[] args)
        {
            Logger logger = new Logger("", "Crach");
            logger.Error(logMessage, args);
            logger.Close();
        }

        /// <summary>
        /// Создать файл c ошибкой краша
        /// </summary>
        public static void Crach(Exception e)
        {
            Logger logger = new Logger("", "Crach");
            logger.Error("{0}: {1}\r\n------\r\n{2}", e.Source, e.Message, e.StackTrace);
            logger.Close();
        }
    }
}
