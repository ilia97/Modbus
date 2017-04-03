using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Models
{
    public class SlaveSettings
    {
        public int Id { set; get; }

        public int StartAddress { set; get; }

        public int NumberOfRegisters { set; get; }

        public string Type { set; get; }
    }
}
