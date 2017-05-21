using System;
using System.Timers;
using System.ServiceProcess;
using System.Diagnostics;
using Autofac;
using Core.Services.Interfaces;
using Core.DataAccess.Interfaces;
using Core.Misc;

namespace ServiceApp
{
    public partial class Service1 : ServiceBase
    {
        private readonly IModbusMasterInitializer _modbusMasterInitializer;
        private readonly IModbusService _modbusService;

        public Service1()
        {
            var container = AutofacConfig.ConfigureContainer();

            using (var scope = container.BeginLifetimeScope())
            {
                _modbusMasterInitializer = scope.Resolve<IModbusMasterInitializer>();
                _modbusService = scope.Resolve<IModbusService>();
            }

            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;
            Logger.Write($"Service 3MBP v{version} starting poll");

            try
            {
                // Получаем данные из репозитория
                var masterSettings = _modbusMasterInitializer.GetMasterSettings();

                if (masterSettings.Period > 0)
                {
                    // Если интервал запуска не равен нулю, то запускаем опрос ведомых устройств с этим интервалом (1с = 1000мс).
                    var slavesDataReaderTimer = new Timer(masterSettings.Period * 1000);
                    slavesDataReaderTimer.Elapsed += (sender, e) => _modbusService.GetDataFromSlaves(masterSettings);

                    // Так как таймер запускает функцию только по окончанию периода времени, то вначале запускаем таймер, а потом таймер.
                    slavesDataReaderTimer.Start();
                    _modbusService.GetDataFromSlaves(masterSettings);
                }
                else
                {
                    // Если интервал запуска равен нулю, то запускаем опрос ведомых устройств один раз.
                    _modbusService.GetDataFromSlaves(masterSettings);
                }

                if (masterSettings.StatFlushPeriod > 0)
                {
                    var packagesLogTimer = new Timer(masterSettings.StatFlushPeriod * 1000 * 60);
                    packagesLogTimer.Elapsed += (sender, e) => Logger.Write($"Sent={PackagesCounter.RequestedPackagesCount}; Rec={PackagesCounter.RecievedPackagesCount}: RecNOK={PackagesCounter.LostPackagesCount}");
                    packagesLogTimer.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Write(ex.Message);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
