using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using Core.DataAccess.Interfaces;
using Core.Misc;
using Core.Misc.Enums;
using Core.Misc.Exceptions;
using Core.Models;
using Core.Services.Interfaces;
using Modbus.Device;
using Modbus;
using System.Collections;

namespace Core.Services
{
    public class ModbusService : IModbusService
    {
        private readonly IModbusSlavesRepository _modbusSlavesRepository;

        private static bool isConnectionLost;

        public ModbusService(IModbusSlavesRepository modbusSlavesRepository)
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

            try
            {
                var masterSettingsIp = masterSettings as MasterSettingsIp;
                if (masterSettingsIp != null)
                {
                    // Если используется IP адресс, то используем TCP клиент для установления соединения.
                    var client = new TcpClient(masterSettingsIp.Host,
                        masterSettingsIp.Port)
                    { ReceiveTimeout = masterSettings.Timeout };

                    master = ModbusIpMaster.CreateIp(client);

                    results = GetDataFromConnection(master, masterSettings);

                    client.Close();
                }
                else
                {
                    var masterSettingsCom = masterSettings as MasterSettingsCom;
                    if (masterSettingsCom != null)
                    {
                        // Случай, если используется СОМ соединение.
                        var port = new SerialPort(masterSettingsCom.PortName)
                        {
                            BaudRate = masterSettingsCom.BaudRate,
                            DataBits = masterSettingsCom.DataBits,
                            Parity = masterSettingsCom.Parity,
                            StopBits = masterSettingsCom.StopBits,
                            ReadTimeout = masterSettingsCom.Timeout
                        };

                        port.Open();

                        master = ModbusSerialMaster.CreateRtu(port);

                        results = GetDataFromConnection(master, masterSettings);

                        port.Close();
                    }
                }
            }
            catch (SocketException ex)
            {
                if (!isConnectionLost)
                {
                    Logger.Write(ex.Message);
                }
                isConnectionLost = true;
            }

            _modbusSlavesRepository.SaveData(results);
        }

        private static Dictionary<int, string> GetDataFromConnection(ModbusMaster master, MasterSettings masterSettings)
        {
            if (master == null) return null;

            var results = new Dictionary<int, string>();

            foreach (var slave in masterSettings.SlaveSettings)
            {
                string hexResults = "";

                PackagesCounter.RequestedPackagesCount += 1;

                try
                {
                    if (masterSettings.IsLoggerEnabled)
                    {
                        Logger.WriteDebug($"Sent request to a slave:\r\nDeviceId = {masterSettings.DeviceId};\r\nSlaveAddress={slave.StartAddress};\r\nNumberOfRegisters={slave.NumberOfRegisters}.");
                    }

                    var registers = master.ReadHoldingRegisters(masterSettings.DeviceId, slave.StartAddress,
                        slave.NumberOfRegisters);

                    if (registers == null || registers.Length == 0)
                    {
                        throw new EmptyResultException(
                            $"Slave with address {masterSettings.DeviceId} returned an empty result when reading {slave.NumberOfRegisters} registers starting with register number {slave.StartAddress}.");
                    }

                    PackagesCounter.RecievedPackagesCount += 1;
                    isConnectionLost = false;

                    var inputs = registers.ConvertToBitArray();

                    hexResults = inputs.ConvertToHex();

                    var startAddress = slave.StartAddress;

                    foreach (var type in slave.Types)
                    {
                        switch (type.Item2)
                        {
                            case ModbusDataType.SInt16:
                                // Берём из массива битов первые 16, чтобы сконвертировать их в знаковое 16-битное целое число.
                                var sInt16Part = inputs.Take(8 * type.Item1).ToArray();

                                // Обрезаем у массива эти первые 16 бит (16 бит = 2 байта).
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var sInt16BinaryString = string.Join("", sInt16Part.Select(x => x ? 1 : 0));

                                // Конвертируем полученное двоичное число в знаковое 16-битное целое число.
                                var sInt16 = Convert.ToInt16(sInt16BinaryString, 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, sInt16.ToString());

                                // Перемещаем указатель на следующий регистр (16 бит = 2 байта = 1 регистр)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.UInt16:
                                // Берём из массива битов первые 16, чтобы сконвертировать их в беззнаковое 16-битное целое число.
                                var uInt16Part = inputs.Take(8 * type.Item1).ToArray();

                                // Обрезаем у массива эти первые 16 бит (16 бит = 2 байта).
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryString = uInt16Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 16-битное целое число.
                                var uShort = Convert.ToUInt16(string.Join("", binaryString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, uShort.ToString());

                                // Перемещаем указатель на следующий регистр (16 бит = 2 байта = 1 регистр)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.SInt32:
                                // Берём из массива битов первые 32, чтобы сконвертировать их в знаковое 32-битное целое число.
                                var sInt32Part = inputs.Take(8 * type.Item1).ToArray();

                                // Обрезаем у массива эти первые 32 бит (32 бит = 4 байта).
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var sInt32BinaryString = sInt32Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в знаковое 32-битное целое число.
                                var sInt32 = Convert.ToInt32(string.Join("", sInt32BinaryString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, sInt32.ToString());

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.UInt32:
                                // Берём из массива битов первые 32, чтобы сконвертировать их в беззнаковое 32-битное целое число.
                                var uInt32Part = inputs.Take(8 * type.Item1).ToArray();

                                // Обрезаем у массива эти первые 32 бит (32 бит = 4 байта).
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryUInt32String = uInt32Part.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 32-битное целое число.
                                var uInt32 = Convert.ToUInt32(string.Join("", binaryUInt32String), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, uInt32.ToString());

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.Hex:
                                // Берём из массива битов первые 32, чтобы сконвертировать их в беззнаковое 32-битное целое число.
                                var hexPart = inputs.Take(8 * type.Item1).ToArray();

                                // Обрезаем у массива эти первые 32 бит (32 бит = 4 байта).
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryHexString = hexPart.Select(x => x ? 1 : 0);

                                // Конвертируем полученное десятичное число в знаковое 32-битное целое число, после этого конвертируем число в 16-ричную систему счисления.
                                var hex = Convert.ToUInt32(string.Join("", binaryHexString), 2).ToString("X");

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, $"0x{hex}");

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.UtcTimestamp:
                                // В данном случае мы должны считать целое число (тоже беззнаковое 32-битное) и преобразовать к дате.
                                // Берём из массива битов первые 32, чтобы сконвертировать их в знаковое 32-битное целое число.
                                var utcTimestampPart = inputs.Take(8 * type.Item1).ToArray();
                                inputs = inputs.Skip(8 * type.Item1).ToArray();

                                // Преобразуем массив битов в строку, состоящую из единиц и нулей.
                                var binaryUtcTimestampString = utcTimestampPart.Select(x => x ? 1 : 0);

                                // Конвертируем полученное двоичное число в беззнаковое 32-битное целое число.
                                var utcTimestamp = Convert.ToUInt32(string.Join("", binaryUtcTimestampString), 2);

                                // Добавляем полученное число в список значений.
                                results.Add(startAddress, new DateTime(1970, 1, 1).AddSeconds(utcTimestamp)
                                    .ToLocalTime()
                                    .ToString("yyyy.MM.dd HH:mm:ss"));

                                // Перемещаем указатель на следующий регистр (32 бита = 4 байта = 2 регистра)
                                startAddress += (ushort)(type.Item1 / 2);

                                break;
                            case ModbusDataType.String:
                                bool[] stringPart;

                                if (inputs.Length > 8 * type.Item1)
                                {
                                    stringPart = inputs.Take(8 * type.Item1).ToArray();
                                    inputs = inputs.Skip(8 * type.Item1).ToArray();

                                    results.Add(startAddress, stringPart.ConvertToString());

                                    startAddress += (ushort)(type.Item1 / 2);
                                }
                                else
                                {
                                    stringPart = inputs;
                                    inputs = new bool[0];

                                    results.Add(startAddress, stringPart.ConvertToString());

                                    startAddress += (ushort)(stringPart.Length / 16);
                                }

                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                    
                    if (masterSettings.IsLoggerEnabled)
                    {
                        Logger.WriteDebug($"Recieved data from slave:\r\nDeviceId = {masterSettings.DeviceId};\r\nSlaveAddress={slave.StartAddress};\r\nNumberOfRegisters={slave.NumberOfRegisters};\r\n{hexResults}");
                    }
                }
                catch (SlaveException slaveException)
                {
                    switch (slaveException.SlaveExceptionCode)
                    {
                        case 130:
                            if (!isConnectionLost)
                            {
                                Logger.Write(slaveException.Message);
                            }
                            isConnectionLost = true;
                            break;
                        default:
                            Logger.Write(slaveException.Message);
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Logger.Write(exception.Message);
                }
            }

            return results;
        }
    }
}
