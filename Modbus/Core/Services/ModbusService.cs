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
        private readonly IModbusMasterInitializer _modbusMasterInitializer;
        private readonly IModbusSlavesRepository _modbusSlavesRepository;

        public ModbusService(IModbusMasterInitializer modbusMasterInitializer,
            IModbusSlavesRepository modbusSlavesRepository)
        {
            this._modbusMasterInitializer = modbusMasterInitializer;
            this._modbusSlavesRepository = modbusSlavesRepository;
        }

        public void GetDataFromSlaves()
        {
            var masterSettings = _modbusMasterInitializer.GetMasterSettings();

            ModbusIpMaster master = null;

            var masterSettingsIp = masterSettings as MasterSettingsIp;
            if (masterSettingsIp != null)
            {
                TcpClient client = new TcpClient(masterSettingsIp.Host,
                    masterSettingsIp.Port);
                master = ModbusIpMaster.CreateIp(client);
            }
            else
            {
                var masterSettingsCom = masterSettings as MasterSettingsIpmo
                if (masterSettingsIp != null)
                {
                    TcpClient client = new TcpClient(masterSettingsIp.Host,
                        masterSettingsIp.Port);
                    master = ModbusIpMaster.CreateIp(client);
                }
                SerialPort port = new SerialPort("COM1");

                // configure serial port
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.Open();
            }

            if (master != null)
            {
                foreach (var slave in masterSettings.SlaveSettings)
                {
                    bool[] inputs = master.ReadInputs(slave.StartAddress, slave.NumberOfRegisters);
                }
            }

            // read five input values
            ushort startAddress = 100;
            ushort numInputs = 5;

            

            for (int i = 0; i < numInputs; i++)
                Console.WriteLine("Input {0}={1}", startAddress + i, inputs[i] ? 1 : 0);

        }
    }
}
