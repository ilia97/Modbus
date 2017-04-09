using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DataAccess.Interfaces
{
    public interface IModbusSlavesRepository
    {
        void SaveData(Dictionary<int, string> registers);
    }
}
