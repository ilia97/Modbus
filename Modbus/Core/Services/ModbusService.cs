using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Core.DataAccess.Interfaces;
using Core.Misc.Enums;
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

            if (masterSettings.Period > 0)
            {
                var timer = new Timer(masterSettings.Period);
                timer.Elapsed += (sender, e) => GetDataFromSlaves(masterSettings);
                timer.Start();
            }
            else
            {
                GetDataFromSlaves(masterSettings);
            }
        }

        private void GetDataFromSlaves(MasterSettings masterSettings)
        {
            ModbusIpMaster master = null;

            var masterSettingsIp = masterSettings as MasterSettingsIp;
            if (masterSettingsIp != null)
            {
                TcpClient client = new TcpClient(masterSettingsIp.Host,
                    masterSettingsIp.Port);
                client.ReceiveTimeout = masterSettings.Timeout;

                master = ModbusIpMaster.CreateIp(client);
            }

            if (master != null)
            {
                var results = new Dictionary<int, string>();

                foreach (var slave in masterSettings.SlaveSettings)
                {
                    var currentRegisterNumber = slave.StartAddress;

                    foreach (var type in slave.Types)
                    {
                        bool[] inputs = master.ReadInputs(slave.StartAddress, slave.NumberOfRegisters);

                        byte[] strArr = new byte[inputs.Length / 8];

                        for (int i = 0; i < inputs.Length / 8; i++)
                        {
                            for (int index = i * 8, m = 1; index < i * 8 + 8; index++, m *= 2)
                            {
                                strArr[i] += inputs[index] ? (byte) m : (byte) 0;
                            }
                        }

                        switch (type)
                        {
                                case ModbusDataType.SInt16:
                                break;
                        }
                        var result = new ASCIIEncoding().GetString(strArr);

                        results.Add(slave.StartAddress, result);
                    }
                }

                _modbusSlavesRepository.SaveData(results);
            }
        }
    }
}
