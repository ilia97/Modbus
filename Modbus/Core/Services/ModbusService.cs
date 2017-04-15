using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Timers;
using Core.DataAccess.Interfaces;
using Core.Misc;
using Core.Misc.Enums;
using Core.Models;
using Core.Services.Interfaces;
using Modbus.Device;
using Modbus;

namespace Core.Services
{
    public class ModbusService : IModbusService
    {
        private readonly IModbusSlavesRepository _modbusSlavesRepository;

        public ModbusService(IModbusMasterInitializer modbusMasterInitializer,
            IModbusSlavesRepository modbusSlavesRepository)
        {
            _modbusSlavesRepository = modbusSlavesRepository;
        }

        /// <summary>
        /// Метод, используемый для опроса ведомых устройств на основе настроек инициализации.
        /// </summary>
        /// <param name="masterSettings">Объект, содержащий настройки инициализации</param>
        public void GetDataFromSlaves(MasterSettings masterSettings)
        {
            ModbusMaster master;
            var results = new Dictionary<int, string>();

            var masterSettingsIp = masterSettings as MasterSettingsIp;
            if (masterSettingsIp != null)
            {
                var client = new TcpClient(masterSettingsIp.Host,
                    masterSettingsIp.Port)
                { ReceiveTimeout = masterSettings.Timeout };

                master = ModbusIpMaster.CreateIp(client);

                results = GetDataFromConnection(master, masterSettings);
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

                    results = GetDataFromConnection(master, masterSettings);

                    port.Close();
                }
            }

            _modbusSlavesRepository.SaveData(results);
        }

        private static Dictionary<int, string> GetDataFromConnection(ModbusMaster master, MasterSettings masterSettings)
        {
            if (master == null) return null;

            var results = new Dictionary<int, string>();

            foreach (var slave in masterSettings.SlaveSettings)
            {
                try
                {
                    var registers = master.ReadHoldingRegisters(masterSettings.DeviceId, slave.StartAddress,
                        slave.NumberOfRegisters);

                    var inputs = registers.ConvertToBitArray();

                    var startAddress = slave.StartAddress;

                    foreach (var type in slave.Types)
                    {
                        switch (type)
                        {
                            case ModbusDataType.SInt16:
                                // Берём из массива битов первые 16, чтобы сконвертировать их в знаковое 16-битное целое число.
                                var sInt16Part = inputs.Take(8 * 2).ToArray();

                                // Обрезаем у массива эти первые 16 бит (16 бит = 2 байта).
                                inputs = inputs.Skip(8 * 2).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var sInt16BinaryString = sInt16Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в знаковое 16-битное целое число.
                                var sInt16 = Convert.ToInt16(string.Join("", sInt16BinaryString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, sInt16.ToString());

                                // Перемещаем указатель на следующий регистр (16 бит = 2 байта = 1 регистр)
                                startAddress += 1;

                                break;
                            case ModbusDataType.UInt16:
                                // Берём из массива битов первые 16, чтобы сконвертировать их в беззнаковое 16-битное целое число.
                                var uInt16Part = inputs.Take(8 * 2).ToArray();

                                // Обрезаем у массива эти первые 16 бит (16 бит = 2 байта).
                                inputs = inputs.Skip(8 * 2).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryString = uInt16Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 16-битное целое число.
                                var uShort = Convert.ToUInt16(string.Join("", binaryString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, uShort.ToString());

                                // Перемещаем указатель на следующий регистр (16 бит = 2 байта = 1 регистр)
                                startAddress += 1;

                                break;
                            case ModbusDataType.SInt32:
                                // Берём из массива битов первые 32, чтобы сконвертировать их в знаковое 32-битное целое число.
                                var sInt32Part = inputs.Take(8 * 4).ToArray();

                                // Обрезаем у массива эти первые 32 бит (32 бит = 4 байта).
                                inputs = inputs.Skip(8 * 4).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var sInt32BinaryString = sInt32Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в знаковое 32-битное целое число.
                                var sInt32 = Convert.ToInt32(string.Join("", sInt32BinaryString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, sInt32.ToString());

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += 2;

                                break;
                            case ModbusDataType.UInt32:
                                // Берём из массива битов первые 32, чтобы сконвертировать их в беззнаковое 32-битное целое число.
                                var uInt32Part = inputs.Take(8 * 4).ToArray();

                                // Обрезаем у массива эти первые 32 бит (32 бит = 4 байта).
                                inputs = inputs.Skip(8 * 4).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryUInt32String = uInt32Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 32-битное целое число.
                                var uInt32 = Convert.ToUInt32(string.Join("", binaryUInt32String), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, uInt32.ToString());

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += 2;

                                break;
                            case ModbusDataType.UtcTimestamp:
                                // В данном случае мы должны считать целое число (тоже беззнаковое 32-битное) и преобразовать к дате.
                                // Берём из массива битов первые 32, чтобы сконвертировать их в знаковое 32-битное целое число.
                                var utcTimestampPart = inputs.Take(8 * 4).ToArray();
                                inputs = inputs.Skip(8 * 4).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryUtcTimestampString = utcTimestampPart.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 32-битное целое число.
                                var utcTimestamp = Convert.ToUInt32(string.Join("", binaryUtcTimestampString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress,
                                    new DateTime(1970, 1, 1).AddSeconds(utcTimestamp).ToString("yyyy.MM.dd HH:mm:ss"));

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
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
                catch (SlaveException slaveException)
                {
                    if (masterSettings.IsLoggerEnabled)
                    {
                        Logger.WriteError(slaveException.Message);
                    }
                }
            }

            return results;
        }
    }
}
