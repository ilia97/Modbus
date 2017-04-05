using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class MasterSettingsCom : MasterSettings
    {
        public string PortName { set; get; }

        public int BaudRate { set; get; }

        public int DataBits { set; get; }

        public Parity Parity { set; get; }

        public StopBits StopBits { set; get; }
    }
}
