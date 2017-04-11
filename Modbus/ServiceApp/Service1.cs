using System.ServiceProcess;
using Autofac;
using Core.Services.Interfaces;

namespace ServiceApp
{
    public partial class Service1 : ServiceBase
    {
        private readonly Autofac.IContainer _container;

        public Service1()
        {
            _container = AutofacConfig.ConfigureContainer();
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            using (var scope = _container.BeginLifetimeScope())
            {
                scope.Resolve<IModbusService>().GetDataFromSlaves();
            }
        }

        protected override void OnStop()
        {
        }
    }
}
