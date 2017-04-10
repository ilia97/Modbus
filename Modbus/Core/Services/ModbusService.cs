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
using Modbus;

namespace Core.Services
{
    public class ModbusService : IModbusService
    {
        /// <summary>
        /// Инициализатор настроек приложения.
        /// </summary>
        private readonly IModbusMasterInitializer _modbusMasterInitializer;

        /// <summary>
        /// Репозиторий для хранения данных ведомых устройств. 
        /// </summary>
        private readonly IModbusSlavesRepository _modbusSlavesRepository;

        public ModbusService(IModbusMasterInitializer modbusMasterInitializer,
            IModbusSlavesRepository modbusSlavesRepository)
        {
            _modbusMasterInitializer = modbusMasterInitializer;
            _modbusSlavesRepository = modbusSlavesRepository;
        }

        /// <summary>
        /// Метод, используемый для опроса ведомых устройств. Настройки инициализации берутся из файла настроек.
        /// </summary>
        public void GetDataFromSlaves()
        {
            // Получаем данные из репозитория
            var masterSettings = _modbusMasterInitializer.GetMasterSettings();

            if (masterSettings.Period > 0)
            {
                // Если интервал запуска не равен нулю, то запускаем опрос ведомых устройств с этим интервалом.
                var timer = new Timer(masterSettings.Period);
                timer.Elapsed += (sender, e) => GetDataFromSlaves(masterSettings);
                timer.Start();
            }
            else
            {
                // Если интервал запуска равен нулю, то запускаем опрос ведомых устройств один раз.
                GetDataFromSlaves(masterSettings);
            }
        }

        /// <summary>
        /// Метод, используемый для опроса ведомых устройств на основе настроек инициализации.
        /// </summary>
        /// <param name="masterSettings">Объект, содержащий настройки инициализации</param>
        private void GetDataFromSlaves(MasterSettings masterSettings)
        {
            var results = new Dictionary<int, string>();
            ModbusMaster master = null;

            var masterSettingsIp = masterSettings as MasterSettingsIp;
            if (masterSettingsIp != null)
            {
                var client = new TcpClient(masterSettingsIp.Host,
                    masterSettingsIp.Port)
                { ReceiveTimeout = masterSettings.Timeout };

                master = ModbusIpMaster.CreateIp(client);
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

                    master = ModbusSerialMaster.CreateRtu(port);
                }
            }

            if (master == null) return;

            foreach (var slave in masterSettings.SlaveSettings)
            {
                try
                {
                    var registers = master.ReadHoldingRegisters(masterSettings.DeviceId, slave.StartAddress,
                        slave.NumberOfRegisters);

                    AddBitsToResult(results, registers.ConvertToBitArray(), slave);
                }
                catch (SlaveException slaveException)
                {
                    if (masterSettings.IsLoggerEnabled)
                    {
                        Logger.WriteError(slaveException.Message);
                    }
                }
            }

            _modbusSlavesRepository.SaveData(results);
        }

        private static void AddBitsToResult(IDictionary<int, string> results, bool[] inputs, GroupSettings slave)
        {
            var startAddress = slave.StartAddress;

            foreach (var type in slave.Types)
            {
                switch (type)
                {
                    case ModbusDataType.SInt16:
                        var sInt16Part = inputs.Take(8 * 2).ToArray();
                        inputs = inputs.Skip(8 * 2).ToArray();

                        var shortArray = new short[1];
                        new BitArray(sInt16Part).CopyTo(shortArray, 0);

                        results.Add(startAddress, shortArray[0].ToString());

                        startAddress += 1;

                        break;
                    case ModbusDataType.UInt16:
                        var uInt16Part = inputs.Take(8 * 2).ToArray();
                        inputs = inputs.Skip(8 * 2).ToArray();

                        var uShortArray = new ushort[1];
                        new BitArray(uInt16Part).CopyTo(uShortArray, 0);

                        results.Add(startAddress, uShortArray[0].ToString());

                        startAddress += 1;

                        break;
                    case ModbusDataType.SInt32:
                        var sInt32Part = inputs.Take(8 * 4).ToArray();
                        inputs = inputs.Skip(8 * 4).ToArray();

                        var intArray = new int[1];
                        new BitArray(sInt32Part).CopyTo(intArray, 0);

                        results.Add(startAddress, intArray[0].ToString());

                        startAddress += 2;

                        break;
                    case ModbusDataType.UInt32:
                    case ModbusDataType.UtcTimestamp:
                        var uInt32Part = inputs.Take(8 * 4).ToArray();
                        inputs = inputs.Skip(8 * 4).ToArray();

                        var uIntArray = uInt32Part.ConvertToUInt();

                        results.Add(startAddress, uIntArray.ToString());

                        startAddress += 2;

                        break;
                    case ModbusDataType.String18:
                        bool[] string18Part;

                        if (inputs.Length > 8 * 18)
                        {
                            string18Part = inputs.Take(8 * 18).ToArray();
                            inputs = inputs.Skip(8 * 18).ToArray();

                            results.Add(startAddress, string18Part.ConvertToString());

                            startAddress += 9;
                        }
                        else
                        {
                            string18Part = inputs;
                            inputs = new bool[0];

                            results.Add(startAddress, string18Part.ConvertToString());

                            startAddress += (ushort)(string18Part.Length / 16);
                        }
                        break;
                    case ModbusDataType.String20:
                        bool[] string20Part;

                        if (inputs.Length > 8 * 20)
                        {
                            string20Part = inputs.Take(8 * 20).ToArray();
                            inputs = inputs.Skip(8 * 20).ToArray();

                            results.Add(startAddress, string20Part.ConvertToString());

                            startAddress += 10;
                        }
                        else
                        {
                            string20Part = inputs;
                            inputs = new bool[0];

                            results.Add(startAddress, string20Part.ConvertToString());

                            startAddress += (ushort)(string20Part.Length / 16);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
