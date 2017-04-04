using Core.DataAccess.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Misc.Enums;
using Core.Models;

namespace Core.DataAccess
{
    public class ModbusMasterInitializer : IModbusMasterInitializer
    {
        public MasterSettings GetMasterSettings()
        {
            var initFileName = ConfigurationManager.AppSettings["InitFileName"];

            var fileLines = File.ReadAllLines(initFileName);

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

            var timeout = Convert.ToInt32(fileLines[2].Split('=')[1]);

            var portTypeString = fileLines[3].Split('=')[1];
            var portType = PortType.IP;
            switch (portTypeString)
            {
                case "ip":
                    break;
                case "com":
                    portType = PortType.COM;
                    break;
                default:
                    throw new FileLoadException($"The port type field has incorrect value (line 4 in {initFileName} file).");
            }

            var period = Convert.ToInt32(fileLines[5].Split('=')[1]);

            var slaves = new List<SlaveSettings>();
            for (var i = 7;  i < fileLines.Length;  i++)
            {
                var slaveDetails = fileLines[i].Split('=');
                var slaveSettings = slaveDetails[1].Split(';');

                var dataTypes = slaveSettings[2].Split(';');

                slaves.Add(new SlaveSettings()
                {
                    Id = Convert.ToInt32(slaveDetails[0]),
                    StartAddress = Convert.ToUInt16(slaveSettings[0]),
                    NumberOfRegisters = Convert.ToUInt16(slaveSettings[1]),
                    Types = dataTypes.Select(x =>
                    {
                        switch (x)
                        {
                            case "String8_18":
                                return ModbusDataType.String18;
                            case "String8_20":
                                return ModbusDataType.String20;
                            case "UTC_Timestamp":
                                return ModbusDataType.UtcTimestamp;
                            case "SInt16":
                                return ModbusDataType.SInt16;
                            case "UInt16":
                                return ModbusDataType.UInt16;
                            case "SInt32":
                                return ModbusDataType.SInt32;
                            case "UInt32":
                                return ModbusDataType.UInt32;
                            default:
                                throw new Exception();
                        }
                    }).ToList()
                });
            }

            if (portType == PortType.IP)
            {
                string ipAddress = fileLines[4].Split('=')[1];

                return new MasterSettingsIp()
                {
                    Host = ipAddress.Split(':')[0],
                    IsLoggerEnabled = isLoggerEnabled,
                    Period = period,
                    Port = Convert.ToInt32(ipAddress.Split(':')[1]),
                    PortType = portType,
                    SlaveSettings = slaves,
                    Timeout = timeout
                };
            }

            if (portType == PortType.COM)
            {
                
            }

            return null;
        }
    }
}
