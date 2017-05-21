using System;
using System.Diagnostics;
using System.Threading;
using Autofac;
using Core.Services.Interfaces;
using Core.DataAccess.Interfaces;
using Core.Misc;
using Timer = System.Timers.Timer;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = AutofacConfig.ConfigureContainer();
            Logger.WriteLogsToConsole = true;

            using (var scope = container.BeginLifetimeScope())
            {
                var modbusMasterInitializer = scope.Resolve<IModbusMasterInitializer>();
                var modbusService = scope.Resolve<IModbusService>();

                try
                {

                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                    string version = fvi.FileVersion;
                    Logger.Write($"Console Application 3MBP v{version} starting poll");

                    // Получаем данные из репозитория
                    var masterSettings = modbusMasterInitializer.GetMasterSettings();

                    if (masterSettings.Period > 0)
                    {
                        var timerLock = new object();

                        // Если интервал запуска не равен нулю, то запускаем опрос ведомых устройств с этим интервалом (1с = 1000мс).
                        var timer = new Timer(masterSettings.Period * 1000);
                        timer.Elapsed += (sender, e) =>
                        {
                            // Устанавливаем монитор для того, чтобы контроллировать, завершился ли процесс считывания данных.
                            Monitor.Enter(timerLock);
                            modbusService.GetDataFromSlaves(masterSettings);
                            Monitor.Exit(timerLock);
                        };

                        // Так как таймер запускает функцию только по окончанию периода времени, то вначале запускаем таймер, а потом таймер.
                        timer.Start();

                        // Устанавливаем монитор для того, чтобы контроллировать, завершился ли процесс считывания данных.
                        Monitor.Enter(timerLock);
                        modbusService.GetDataFromSlaves(masterSettings);
                        Monitor.Exit(timerLock);

                        if (masterSettings.StatFlushPeriod > 0)
                        {
                            var packagesLogTimer = new Timer(masterSettings.StatFlushPeriod * 1000 * 60);
                            packagesLogTimer.Elapsed += (sender, e) => Logger.Write($"Sent={PackagesCounter.RequestedPackagesCount}; Rec={PackagesCounter.RecievedPackagesCount}: RecNOK={PackagesCounter.LostPackagesCount}");
                            packagesLogTimer.Start();
                        }

                        // Запускаем бесконечный цикл 
                        while (timer.Enabled)
                        {
                            var str = Console.ReadLine();

                            switch (str)
                            {
                                case "-q":
                                    while (!Monitor.TryEnter(timerLock))
                                    {
                                        // Если монитор занят в данный момент, то ждём, пока он освободится (завершится процесс считывания данных).
                                    }

                                    // Завершаем процесс считывания данных.
                                    timer.Stop();
                                    break;
                                case "-?":
                                    Console.WriteLine(
                                        "This is a program created for reading data from Modbus slave devices. Settings are read from 3MBP.ini file. The requirements for this file are:");
                                    Console.WriteLine(
                                        "1) The first line contains string \"[Main]\". This is just a thing of buity.");
                                    Console.WriteLine(
                                        "2) Logger settings must be placed on the line 2. This setting says whether logs are enabled in the app. Logger value must be equal to \"Yes\" or \"No\". For example:\r\n    Logging=Yes");
                                    Console.WriteLine(
                                        "3) Timeout settings must be placed on the line 3. This setting sets the connection timeout. Timeout must be a positive integer number. For example:\r\n    Timeout=1000");
                                    Console.WriteLine(
                                        "4) Port settings must be placed on the line 4. Port must be a equal to \"IP\" or \"COM\". For example:\r\n    Port=IP");
                                    Console.WriteLine(
                                        "5) Connection settings must be placed on the line 5. If IP port type was selected at the previous line we should place here IP connection settings.\r\n    The correct declaration is: IP=[Ip address]:[Port] For example:\r\n    IP=127.0.0.1:502");
                                    Console.WriteLine(
                                        "    If COM port type was selected at the previous line we should place here COM connection settings.\r\n    The correct declaration is: COM=[Port name];[Baud rate];[Data Bits][Parity][Stop Bits] Example of connection declaration:\r\n    COM=COM9;9600;8N1");
                                    Console.WriteLine(
                                        "6) DeviceID settings must be placed on the line 6. DeviceId must be a positive byte number. For example:\r\n    DeviceID=10");
                                    Console.WriteLine(
                                        "7) Period settings must be placed on the line 7. This setting sets the period of modbus slaves reading. Period must be a positive integer number (line 6). For example:\r\n    Period=10");
                                    Console.WriteLine(
                                        "8) The eight line contains string \"[Reading]\". This is just a thing of buity.");
                                    Console.WriteLine(
                                        "9) All next lines contain information about groups of data that must be read from slaves.\r\n    The correct declaration is: [Group number]=[StartingRegister];[Number of Registers];[Types splitted with \";\"] Example of group declaration:\r\n    2=2050;4;Uint32;Uint32");
                                    Console.WriteLine(
                                        "10) All comments start with \"//\". They will be ignored when reading a file. All lines that are commented out won't be taken into lines numeration.");
                                    break;
                                default:
                                    Console.WriteLine(
                                        "This symbol is not supported by the program. Please type \"-?\" to get possible commands.");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        // Если интервал запуска равен нулю, то запускаем опрос ведомых устройств один раз.
                        modbusService.GetDataFromSlaves(masterSettings);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Write(ex.Message);
                    Console.ReadLine();
                }
            }
        }
    }
}
