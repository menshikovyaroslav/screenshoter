using System;
using System.IO;
using System.Text;

namespace Screenshoter
{
    public sealed class Log
    {
        private static volatile Log _instance;
        private static readonly object SyncRoot = new object();
        private readonly object _logLocker = new object();

        private Log()
        {
            CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            LogDirectory = Path.Combine(CurrentDirectory, "log");
        }

        public string CurrentDirectory { get; set; }
        public string LogDirectory { get; set; }

        public static Log Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (SyncRoot)
                    {
                        if (_instance == null) _instance = new Log();
                    }
                }
                return _instance;
            }
        }

        public void Error(int errorNumber, string errorText)
        {
            Add($"Ошибка {(errorNumber.ToString()).PadLeft(4, '0')}: {errorText}", "[ERROR]");
        }

        public void Info(string log)
        {
            Add(log, "[INFO]");
        }

        private void Add(string log, string logLevel)
        {
            lock (_logLocker)
            {
                try
                {
                    if (!Directory.Exists(LogDirectory))
                    {
                        // Создание директории log в случае отсутствия
                        Directory.CreateDirectory(LogDirectory);
                    }
                    // Запись в лог файл вместе с датой и уровнем лога.
                    string newFileName = Path.Combine(LogDirectory, String.Format("{0}.txt", DateTime.Now.ToString("yyyyMMdd")));
                    File.AppendAllText(newFileName, $"{DateTime.Now} {logLevel} {log} \r\n", Encoding.UTF8);
                }
                catch { }
            }
        }
    }
}
