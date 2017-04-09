using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MasterSettingsIp : MasterSettings
    {
        public string Host { set; get; }

        public int Port { set; get; }
    }
}
