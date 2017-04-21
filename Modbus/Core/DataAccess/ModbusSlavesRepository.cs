using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Core.DataAccess.Interfaces;

namespace Core.DataAccess
{
    public class ModbusSlavesRepository : IModbusSlavesRepository
    {
        public void SaveData(Dictionary<int, string> registers)
        {
            // Получаем имя директории, в которой хранятся файлы с данными ведомых устройств.
            var dataFolderName = ConfigurationManager.AppSettings["DataFolderName"];

            if (!Directory.Exists(dataFolderName))
            {
                // Если такой директории не существует, создаём её.
                Directory.CreateDirectory(dataFolderName);
            }

            // Генерируем имя файла, исходя из текущей даты.
            var fileName = $"{DateTime.Now:yyyy-MM-dd}.csv";

            // Генерируем пусть к файлу исходя из его имени и имени подкаталога.
            var filePath = Path.Combine(dataFolderName, fileName);

            if (!File.Exists(filePath))
            {
                // Если файла с таким именем не существует, то создаём его и пишем строку вида "Timestamp;{номер первого стартового регистра};{номер второго стартового регистра};..."
                File.AppendAllText(filePath, $"Timestamp;{string.Join(";", registers.Keys)}\r\n");
            }

            // Добавляем строку, содержащую текущее время суток и значение для каждого из ведомых устройств.
            File.AppendAllText(filePath, $"{DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc):HH:mm:ss};{string.Join(";", registers.Values)}\r\n");
        }
    }
}