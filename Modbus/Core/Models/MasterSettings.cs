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

        /// <summary>
        /// Свойство, указывающее, включено ли логирование или нет.
        /// </summary>
        public bool IsLoggerEnabled { set; get; }

        /// <summary>
        /// Свойство, указывающее таймаут ожидания ответа от ведомого устройства.
        /// </summary>
        public int Timeout { set; get; }

        /// <summary>
        /// Номер устройства.
        /// </summary>
        public byte DeviceId { set; get; }

        /// <summary>
        /// Тип соединения.
        /// </summary>
        public PortType PortType { set; get; }

        /// <summary>
        /// Интервал опроса ведомых устройств.
        /// </summary>
        public int Period { set; get; }

        /// <summary>
        /// Список групп.
        /// </summary>
        public List<GroupSettings> SlaveSettings { set; get; }
    }
}
