using System.Collections.Generic;
using Core.Misc.Enums;

namespace Core.Models
{
    public class MasterSettings
    {
        public MasterSettings()
        {
            this.SlaveSettings = new List<SlaveSettings>();
        }

        public bool IsLoggerEnabled { set; get; }

        public int Timeout { set; get; }
         
        public PortType PortType { set; get; }

        public int Period { set; get; }

        public List<SlaveSettings> SlaveSettings { set; get; }
    }
}
