using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Misc.Enums;

namespace Core.Models
{
    public class GroupSettings
    {
        public int Id { set; get; }

        public ushort StartAddress { set; get; }

        public ushort NumberOfRegisters { set; get; }

        public List<ModbusDataType> Types { set; get; }
    }
}
