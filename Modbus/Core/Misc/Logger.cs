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
        public static void WriteError(string error)
        {
            // Берём имя файла логирования из настроек приложения.
            var logFileName = ConfigurationManager.AppSettings["LogFileName"];


            File.AppendAllText(logFileName, $"{DateTime.Now:yyyy:MM:dd HH:mm:ss}\r\n{error}\r\n\r\n\r\n");
        }
    }
}
