using System;
using System.Collections.Generic;
using System.IO;
using Core.DataAccess.Interfaces;

namespace Core.DataAccess
{
    public class ModbusSlavesRepository : IModbusSlavesRepository
    {
        public void SaveData(Dictionary<int, string> registers)
        {
            // Генерируем имя файла, исходя из текущей даты.
            var fileName = $"{DateTime.Now:yyyy-MM-dd}.csv";

            if (!File.Exists(fileName))
            {
                // Если файла с таким именем не существует, то создаём его и пишем строку вида "Timestamp;{номер первого стартового регистра};{номер второго стартового регистра};..."
                File.AppendAllText(fileName, $"Timestamp;{string.Join(";", registers.Keys)}\r\n");
            }

            // Добавляем строку, содержащую текущее время суток и значение для каждого из ведомых устройств.
            File.AppendAllText(fileName, $"{DateTime.Now:HH:mm:ss};{string.Join(";", registers.Values)}\r\n");
        }
    }
}
