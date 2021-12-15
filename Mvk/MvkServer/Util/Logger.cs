using System;
using System.IO;

namespace MvkServer.Util
{
    /// <summary>
    /// Объект фиксирующий лог
    /// </summary>
    public class Logger
    {
        protected static string fileName;

        public static void Initialized()
        {
            string path = ToPath();
            string sd = DateTime.Now.ToString("yyyy-MM-dd");
            int i = 1;

            CheckPath(path);
            while (true)
            {
                fileName = string.Format("{0}-{1}.txt", sd, i);
                if (!File.Exists(path + fileName)) break;
                i++;
                //fileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd_HHmm"));
            }
            
        }

        public static void Log(string logMessage, params object[] args)
        {
            CheckPath(ToPath());
            using (StreamWriter w = File.AppendText(ToPath() + fileName))
            {
                w.Write($"[{DateTime.Now.ToLongTimeString()}]");
                w.WriteLine(" " + string.Format(logMessage, args));
            }
        }

        /// <summary>
        /// Путь к директории мира с именем
        /// </summary>
        public static string ToPath() => "Logs" + Path.DirectorySeparatorChar;

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
    }
}
