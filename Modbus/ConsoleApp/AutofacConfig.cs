using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Core.DataAccess;
using Core.DataAccess.Interfaces;
using Core.Services;
using Core.Services.Interfaces;

namespace ConsoleApp
{
    public class AutofacConfig
    {
        public static IContainer ConfigureContainer()
        {
            var builder = new ContainerBuilder();
            
            builder.RegisterType<ModbusMasterInitializer>().As<IModbusMasterInitializer>();
            builder.RegisterType<ModbusSlavesRepository>().As<IModbusSlavesRepository>();
            builder.RegisterType<ModbusService>().As<IModbusService>();
            
            return builder.Build();
        }
    }
}
