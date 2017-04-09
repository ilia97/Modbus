using System.Collections.Generic;
using Core.Misc.Enums;

namespace Core.Models
{
    public class MasterSettings
    {
        public MasterSettings()
        {
            this.SlaveSettings = new List<GroupSettings>();
        }

        public bool IsLoggerEnabled { set; get; }

        public int Timeout { set; get; }

        public byte DeviceId { set; get; }

        public PortType PortType { set; get; }

        public int Period { set; get; }

        public List<GroupSettings> SlaveSettings { set; get; }
    }
}
