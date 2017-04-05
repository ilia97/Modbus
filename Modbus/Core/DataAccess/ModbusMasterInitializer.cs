using Core.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Core.Misc.Enums;
using Core.Models;

namespace Core.DataAccess
{
    public class ModbusMasterInitializer : IModbusMasterInitializer
    {
        public MasterSettings GetMasterSettings()
        {
            // Получаем имя файла настроек из файла конфигураций.
            var initFileName = ConfigurationManager.AppSettings["InitFileName"];

            // Считываем все строки из файла конфигураций.
            var fileLines = File.ReadAllLines(initFileName).ToList();

            // Удаляем все комментарии, чтобы не учитывать их при считывании файла.
            // Если в строке содержится символ комментария, то обрезаем строку до этого символа.
            fileLines =
                fileLines.Select(
                    fileLine =>
                        fileLine.IndexOf("//", StringComparison.Ordinal) > -1
                            ? fileLine.Substring(0, fileLine.IndexOf("//", StringComparison.Ordinal)).Trim()
                            : fileLine.Trim()).ToList();

            // Удаляем все пустуе строки, которые образовались в результате предыдущего шага.
            fileLines.RemoveAll(string.IsNullOrWhiteSpace);

            // На первой строке написана строка "[Main]" просто для удобства чтения, её игнорируем.
            // На второй строке после знака равно должна располагаться настройка, отвечающая за то, включено логирование или нет.
            var isLoggerEnabledString = fileLines[1].Split('=')[1].ToLower();
            var isLoggerEnabled = true;
            switch (isLoggerEnabledString)
            {
                case "yes":
                    break;
                case "no":
                    isLoggerEnabled = false;
                    break;
                default:
                    throw new FileLoadException($"The logger field has incorrect value (line 2 in {initFileName} file).");
            }

            // На третьей строке после знака равно располагается настройка отвечающая за величину таймаута запроса.
            var timeout = Convert.ToInt32(fileLines[2].Split('=')[1]);

            // На четвёртой строке после знака равно располагается тип соединения (COM или IP).
            var portTypeString = fileLines[3].Split('=')[1];
            var portType = PortType.IP;
            switch (portTypeString.ToLower())
            {
                case "ip":
                    break;
                case "com":
                    portType = PortType.COM;
                    break;
                default:
                    throw new FileLoadException(
                        $"The port type field has incorrect value (line 4 in {initFileName} file).");
            }

            // На шестой строке после знака равно располагается идентификатор устройства.
            var deviceId = Convert.ToInt32(fileLines[5].Split('=')[1]);

            // На седьмой строке после знака равно располагается интервал опроса контроллеров.
            var period = Convert.ToInt32(fileLines[6].Split('=')[1]);

            // На восьмой строке написана строка "[Reading]" просто для удобства чтения, её игнорируем.
            // На строках, начиная с девятой расположена информация о группах контроллеров.
            var groups = new List<GroupSettings>();
            for (var i = 8; i < fileLines.Count; i++)
            {
                // До знака равно находится номер группы, после знака равно информация о принимаемых данных для этой группы.
                var slaveDetails = fileLines[i].Split('=');

                // Все данные для группы расположены после знака "=". 
                var slaveSettings = slaveDetails[1];

                // До первой точки с запятой располагается номер первого регистра, заполненного данными.
                // Обрезаем строку по этому символу.
                var startAddress = slaveSettings.Substring(0, slaveSettings.IndexOf(";", StringComparison.Ordinal));
                slaveSettings = slaveSettings.Substring(slaveSettings.IndexOf(";", StringComparison.Ordinal) + 1);

                // Между первой и второй точкой с запятой располагается количество регистров, заполненных данными.
                var registersCount = slaveSettings.Substring(0, slaveSettings.IndexOf(";", StringComparison.Ordinal));
                slaveSettings = slaveSettings.Substring(slaveSettings.IndexOf(";", StringComparison.Ordinal) + 1);

                // Остальная часть строки содержит типы данных, перечисленные черезточку с запятой.
                var dataTypes = slaveSettings.Split(';');

                // Добавляем в список объектов новый объект, содержащий всю вышеперечисленную информацию.
                groups.Add(new GroupSettings()
                {
                    Id = Convert.ToInt32(slaveDetails[0]),
                    StartAddress = Convert.ToUInt16(startAddress),
                    NumberOfRegisters = Convert.ToUInt16(registersCount),
                    Types = dataTypes.Select(x =>
                    {
                        switch (x.ToLower())
                        {
                            case "string8_18":
                                return ModbusDataType.String18;
                            case "string8_20":
                                return ModbusDataType.String20;
                            case "utc_timestamp":
                                return ModbusDataType.UtcTimestamp;
                            case "sint16":
                                return ModbusDataType.SInt16;
                            case "uint16":
                                return ModbusDataType.UInt16;
                            case "sint32":
                                return ModbusDataType.SInt32;
                            case "uint32":
                                return ModbusDataType.UInt32;
                            default:
                                throw new Exception();
                        }
                    }).ToList()
                });
            }

            // В зависимости от того, какой тип соединения прописан на пятой строке файла,
            // мы создаём разные типы объектов, содержащие полную информацию о соединении.
            switch (portType)
            {
                case PortType.IP:
                    var ipAddress = fileLines[4].Split('=')[1];

                    return new MasterSettingsIp()
                    {
                        Host = ipAddress.Split(':')[0],
                        IsLoggerEnabled = isLoggerEnabled,
                        Period = period,
                        DeviceId = deviceId,
                        Port = Convert.ToInt32(ipAddress.Split(':')[1]),
                        PortType = portType,
                        SlaveSettings = groups,
                        Timeout = timeout
                    };
                case PortType.COM:

                    break;
            }

            return null;
        }
    }
}
