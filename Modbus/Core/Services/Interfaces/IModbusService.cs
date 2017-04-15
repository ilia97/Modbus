using Core.Models;

namespace Core.Services.Interfaces
{
    public interface IModbusService
    {
        void GetDataFromSlaves(MasterSettings masterSettings);
    }
}
