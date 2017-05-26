using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Misc
{
    public static class Logger
    {
        /// <summary>
        /// Поле, определяющее, какой тип приложения запущен.
        /// </summary>
        public static bool WriteLogsToConsole;

        private static string logFileName = ConfigurationManager.AppSettings["LogFileName"];
        private static string dataFolderName = ConfigurationManager.AppSettings["DataFolderName"];

        public static void Write(string error)
        {
            if (!Directory.Exists(dataFolderName))
            {
                // Если такой директории не существует, создаём её.
                Directory.CreateDirectory(dataFolderName);
            }

            // Генерируем пусть к файлу исходя из его имени и имени подкаталога.
            var filePath = Path.Combine(dataFolderName, logFileName);

            File.AppendAllText(filePath, $"{DateTime.Now:yyyy:MM:dd HH:mm:ss}\r\n{error}\r\n\r\n\r\n");

            // Если у нас запущено консольное приложение, то ошибку надо выводить и в консоль.
            if (WriteLogsToConsole)
            {
                Console.WriteLine(error);
            }
        }

        public static void WriteDebug(string text)
        {
            var dataFolderName = ConfigurationManager.AppSettings["DataFolderName"];

            if (!Directory.Exists(dataFolderName))
            {
                // Если такой директории не существует, создаём её.
                Directory.CreateDirectory(dataFolderName);
            }

            // Генерируем имя файла, исходя из текущей даты.
            var fileName = $"3MBP_{DateTime.Now:yyyy-MM-dd}.dbg";

            // Генерируем пусть к файлу исходя из его имени и имени подкаталога.
            var filePath = Path.Combine(dataFolderName, fileName);

            // Добавляем строку, содержащую текущее время суток и значение для каждого из ведомых устройств.
            File.AppendAllText(filePath, $"{DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc):HH:mm:ss}\r\n{text}\r\n\r\n\r\n");
        }
    }
}
