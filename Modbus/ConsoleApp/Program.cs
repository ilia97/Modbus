using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Core.DataAccess;
using Core.Services;
using Core.Services.Interfaces;
using Core.DataAccess.Interfaces;
using Core.Misc;
using System.Timers;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = AutofacConfig.ConfigureContainer();
            Logger.IsConsoleApplication = true;

            using (var scope = container.BeginLifetimeScope())
            {
                var _modbusMasterInitializer = scope.Resolve<IModbusMasterInitializer>();
                var _modbusService = scope.Resolve<IModbusService>();

                // Получаем данные из репозитория
                var masterSettings = _modbusMasterInitializer.GetMasterSettings();

                if (masterSettings.Period > 0)
                {
                    // Если интервал запуска не равен нулю, то запускаем опрос ведомых устройств с этим интервалом (1с = 1000мс).
                    var timer = new Timer(masterSettings.Period * 1000);
                    timer.Elapsed += (sender, e) => _modbusService.GetDataFromSlaves(masterSettings);

                    // Так как таймер запускает функцию только по окончанию периода времени, то вначале запускаем таймер, а потом таймер.
                    timer.Start();
                    _modbusService.GetDataFromSlaves(masterSettings);

                    // Запускаем бесконечный цикл 
                    while (timer.Enabled)
                    {
                        var str = Console.ReadLine();

                        switch (str)
                        {
                            case "q":
                                timer.Stop();
                                break;
                            case "?":
                                break;
                            default:
                                Console.WriteLine("This symbol is not supported by the program. Please press \"?\" to get possible commands.");
                                break;
                        }
                    }
                }
                else
                {
                    // Если интервал запуска равен нулю, то запускаем опрос ведомых устройств один раз.
                    _modbusService.GetDataFromSlaves(masterSettings);
                }
            }
        }
    }
}
