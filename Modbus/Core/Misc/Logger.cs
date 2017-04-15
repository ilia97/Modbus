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
        public static bool IsConsoleApplication;

        public static void WriteError(string error)
        {
            // Берём имя файла логирования из настроек приложения.
            var logFileName = ConfigurationManager.AppSettings["LogFileName"];
            
            File.AppendAllText(logFileName, $"{DateTime.Now:yyyy:MM:dd HH:mm:ss}\r\n{error}\r\n\r\n\r\n");

            // Если у нас запущено консольное приложение, то ошибку надо выводить и в консоль.
            if (IsConsoleApplication)
            {
                Console.WriteLine(error);
            }
        }
    }
}
