using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Screenshoter
{
    class Program
    {
        #region DllImport

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        #endregion

        /// <summary>
        /// Как часто делать скриншот, в секундах
        /// </summary>
        static int _interval { get; set; }

        /// <summary>
        /// Размер хранилища скриншотов, в MB
        /// </summary>
        static int _limit { get; set; }

        /// <summary>
        /// Путь к хранилищу скриншотов, пример C:\temp
        /// </summary>
        static string _path { get; set; }

        static void Main(string[] args)
        {
            Log.Instance.Info($"started");

            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            ReadSettings();

            while (true)
            {
                CheckStorage();
                DoScreen();
                Thread.Sleep(_interval * 1000);
            }
        }

        /// <summary>
        /// Чтение настроек из файла конфигурации
        /// </summary>
        static void ReadSettings()
        {
            _interval = 10;
            if (Properties.Settings.Default.interval > 0) _interval = Properties.Settings.Default.interval;
            Log.Instance.Info($"set interval = {_interval} sec");

            _limit = 20;
            if (Properties.Settings.Default.limit > 0) _limit = Properties.Settings.Default.limit;
            Log.Instance.Info($"set storage = {_limit} Mb");

            _path = @"C:\temp";
            if (!string.IsNullOrEmpty(Properties.Settings.Default.path)) _path = Properties.Settings.Default.path;
            Log.Instance.Info($"set path = {_path}");
        }

        /// <summary>
        /// Проверка доступного места в хранилище
        /// </summary>
        static void CheckStorage()
        {
            var currentSize = StorageSize();

            if (currentSize > _limit)
            {
                // Сколько нужно очистить MB
                var totalToTrash = currentSize - _limit;

                // Очистить необходимое кол-во KB
                StorageClear(totalToTrash * 1024);
            }
        }

        /// <summary>
        /// Заполненность хранилища, в MB
        /// </summary>
        /// <returns></returns>
        static long StorageSize()
        {
            long i = 0;

            try
            {
                DirectoryInfo directory = new DirectoryInfo(_path);
                FileInfo[] files = directory.GetFiles();

                foreach (FileInfo file in files)
                {
                    i += file.Length;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error(3, ex.Message);
                return _limit;
            }

            return i /= (1024 * 1024);
        }

        /// <summary>
        /// Очистка хранилища
        /// </summary>
        /// <param name="sizeKb"></param>
        static void StorageClear(long sizeKb)
        {
            try
            {
                Log.Instance.Info($"clear = {sizeKb} Kb");

                DirectoryInfo directory = new DirectoryInfo(_path);
                FileInfo[] files = directory.GetFiles().OrderBy(f => f.CreationTime).ToArray();

                foreach (FileInfo file in files)
                {
                    var size = file.Length / 1024;
                    File.Delete(file.FullName);
                    sizeKb -= size;
                    if (sizeKb <= 0) break;
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error(2, ex.Message);
            }
        }

        /// <summary>
        /// Создание скриншота
        /// </summary>
        static void DoScreen()
        {
            try
            {
                Bitmap printscreen = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics graphics = Graphics.FromImage(printscreen as Image);
                graphics.CopyFromScreen(0, 0, 0, 0, printscreen.Size);
                printscreen.Save(Path.Combine(_path, GetFileName()), System.Drawing.Imaging.ImageFormat.Png);
            }
            catch (Exception ex)
            {
                Log.Instance.Error(1, ex.Message);
            }
        }

        /// <summary>
        /// Имя файла создаваемого скриншота
        /// </summary>
        /// <returns></returns>
        static string GetFileName()
        {
            var time = DateTime.Now;
            return $"{time.ToString("yyyy_MM_dd__HH_mm_ss")}.png";
        }
    }
}
