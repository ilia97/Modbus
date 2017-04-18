using System;
using System.Timers;
using System.ServiceProcess;
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
            try
            {
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
                }
                else
                {
                    // Если интервал запуска равен нулю, то запускаем опрос ведомых устройств один раз.
                    _modbusService.GetDataFromSlaves(masterSettings);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteError(ex.Message);
            }
        }

        protected override void OnStop()
        {
        }
    }
}
