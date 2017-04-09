using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess.Interfaces;

namespace Core.DataAccess
{
    public class ModbusSlavesRepository : IModbusSlavesRepository
    {
        public void SaveData(Dictionary<int, string> registers)
        {
            var fileName = $"{DateTime.Now:yyyy-MM-dd}.csv";

            if (!File.Exists(fileName))
            {
                File.AppendAllText(fileName, $"Timestamp;{string.Join(";", registers.Keys)}\r\n");
            }

            File.AppendAllText(fileName, $"{DateTime.Now:HH:mm:ss};{string.Join(";", registers.Values)}\r\n");
        }
    }
}
