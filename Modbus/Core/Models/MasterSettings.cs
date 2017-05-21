using System.Collections.Generic;
using Core.Misc.Enums;

namespace Core.Models
{
    public class MasterSettings
    {
        protected MasterSettings()
        {
            SlaveSettings = new List<GroupSettings>();
        }

        /// <summary>
        /// Свойство, указывающее, включено ли логирование или нет.
        /// </summary>
        public bool IsLoggerEnabled { set; get; }

        /// <summary>
        /// Свойство, указывающее период, с которым необходимо выводить количество отправленных и обработанных пакетов.
        /// </summary>
        public int StatFlushPeriod { set; get; }

        /// <summary>
        /// Свойство, указывающее таймаут ожидания ответа от ведомого устройства.
        /// </summary>
        public int Timeout { set; get; }

        /// <summary>
        /// Номер устройства.
        /// </summary>
        public byte DeviceId { set; get; }

        /// <summary>
        /// Интервал в секундах опроса ведомых устройств.
        /// </summary>
        public int Period { set; get; }

        /// <summary>
        /// Список групп.
        /// </summary>
        public List<GroupSettings> SlaveSettings { set; get; }
    }
}
