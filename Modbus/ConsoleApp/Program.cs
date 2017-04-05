using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess;
using Core.Services;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            new ModbusService(new ModbusMasterInitializer(), new ModbusSlavesRepository()).GetDataFromSlaves();
        }
    }
}
