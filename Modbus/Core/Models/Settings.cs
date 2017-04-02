using Core.Misc.Enums;

namespace Core.Models
{
    public class Settings
    {
        public bool IsLoggerEnabled { set; get; }

        public int Timeout { set; get; }
         
        public PortType Port { set; get; }

        public int Period { set; get; }
        
         
    }
}
