using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Timers;
using Core.DataAccess.Interfaces;
using Core.Misc;
using Core.Misc.Enums;
using Core.Models;
using Core.Services.Interfaces;
using Modbus.Device;
using System.IO.Ports;

namespace Core.Services
{
    public class ModbusService : IModbusService
    {
        private readonly IModbusMasterInitializer _modbusMasterInitializer;
        private readonly IModbusSlavesRepository _modbusSlavesRepository;

        public ModbusService(IModbusMasterInitializer modbusMasterInitializer,
            IModbusSlavesRepository modbusSlavesRepository)
        {
            _modbusMasterInitializer = modbusMasterInitializer;
            _modbusSlavesRepository = modbusSlavesRepository;
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
            var results = new Dictionary<int, string>();

            var masterSettingsIp = masterSettings as MasterSettingsIp;
            if (masterSettingsIp != null)
            {
                var client = new TcpClient(masterSettingsIp.Host,
                    masterSettingsIp.Port) {ReceiveTimeout = masterSettings.Timeout};

                var master = ModbusIpMaster.CreateIp(client);

                foreach (var slave in masterSettings.SlaveSettings)
                {
                    var inputs = master.ReadInputs(slave.StartAddress, slave.NumberOfRegisters);

                    foreach (var type in slave.Types)
                    {
                        switch (type)
                        {
                            case ModbusDataType.SInt16:
                                var sInt16Part = inputs.Take(8 * 2).ToArray();
                                inputs = inputs.Skip(8 * 2).ToArray();

                                var shortArray = new short[1];
                                new BitArray(sInt16Part).CopyTo(shortArray, 0);

                                results.Add(slave.StartAddress, shortArray[0].ToString());
                                break;
                            case ModbusDataType.UInt16:
                                var uInt16Part = inputs.Take(8 * 2).ToArray();
                                inputs = inputs.Skip(8 * 2).ToArray();

                                var uShortArray = new ushort[1];
                                new BitArray(uInt16Part).CopyTo(uShortArray, 0);

                                results.Add(slave.StartAddress, uShortArray[0].ToString());
                                break;
                            case ModbusDataType.SInt32:
                                var sInt32Part = inputs.Take(8 * 4).ToArray();
                                inputs = inputs.Skip(8 * 4).ToArray();

                                var intArray = new int[1];
                                new BitArray(sInt32Part).CopyTo(intArray, 0);

                                results.Add(slave.StartAddress, intArray[0].ToString());
                                break;
                            case ModbusDataType.UInt32:
                            case ModbusDataType.UtcTimestamp:
                                var uInt32Part = inputs.Take(8 * 4).ToArray();
                                inputs = inputs.Skip(8 * 4).ToArray();

                                var uIntArray = new int[1];
                                new BitArray(uInt32Part).CopyTo(uIntArray, 0);

                                results.Add(slave.StartAddress, uIntArray[0].ToString());
                                break;
                            case ModbusDataType.String18:
                                bool[] string18Part;

                                if (inputs.Length > 8 * 18)
                                {
                                    string18Part = inputs.Take(8 * 18).ToArray();
                                    inputs = inputs.Skip(8 * 18).ToArray();
                                }
                                else
                                {
                                    string18Part = inputs;
                                    inputs = new bool[0];
                                }

                                results.Add(slave.StartAddress, string18Part.ConvertToString());
                                break;
                            case ModbusDataType.String20:
                                bool[] string20Part;

                                if (inputs.Length > 8 * 20)
                                {
                                    string20Part = inputs.Take(8 * 20).ToArray();
                                    inputs = inputs.Skip(8 * 20).ToArray();
                                }
                                else
                                {
                                    string20Part = inputs;
                                    inputs = new bool[0];
                                }

                                results.Add(slave.StartAddress, string20Part.ConvertToString());
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
            }
            else
            {
                var masterSettingsCom = masterSettings as MasterSettingsCom;
                if (masterSettingsCom != null)
                {
                    var port = new SerialPort(masterSettingsCom.PortName)
                    {
                        BaudRate = masterSettingsCom.BaudRate,
                        DataBits = masterSettingsCom.DataBits,
                        Parity = masterSettingsCom.Parity,
                        StopBits = masterSettingsCom.StopBits,
                        ReadTimeout = masterSettingsCom.Timeout
                    };

                    // configure serial port
                    port.Open();

                    var master = ModbusSerialMaster.CreateRtu(port);

                    if (master == null) return;

                    foreach (var slave in masterSettings.SlaveSettings)
                    {
                        var registers = master.ReadHoldingRegisters(masterSettingsCom.DeviceId, slave.StartAddress,
                            slave.NumberOfRegisters);
                    }
                }
            }

            _modbusSlavesRepository.SaveData(results);
        }
    }
}
