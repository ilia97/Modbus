using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Core.DataAccess.Interfaces;
using Core.Models;
using Core.Services.Interfaces;
using Modbus.Device;

namespace Core.Services
{
    public class ModbusService : IModbusService
    {
        private readonly IModbusMasterInitializer modbusMasterInitializer;

        public ModbusService(IModbusMasterInitializer modbusMasterInitializer)
        {
            this.modbusMasterInitializer = modbusMasterInitializer;
        }

        public void GetDataFromSlaves()
        {
            var masterSettings = modbusMasterInitializer.GetMasterSettings();

            ModbusIpMaster master = null;

            if (masterSettings is MasterSettingsIp)
            {
                TcpClient client = new TcpClient(((MasterSettingsIp) masterSettings).Host,
                    ((MasterSettingsIp) masterSettings).Port);
                master = ModbusIpMaster.CreateIp(client);
            }

            // read five input values
            ushort startAddress = 100;
            ushort numInputs = 5;
            bool[] inputs = master.ReadInputs(startAddress, numInputs);

            for (int i = 0; i < numInputs; i++)
                Console.WriteLine("Input {0}={1}", startAddress + i, inputs[i] ? 1 : 0);

        }
    }
}
