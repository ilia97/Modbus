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
        /// <summary>
        /// Номер группы.
        /// </summary>
        public int Id { set; get; }

        /// <summary>
        /// Номер первого регистра, с которого необходимо начинать считывание.
        /// </summary>
        public ushort StartAddress { set; get; }

        /// <summary>
        /// Количество регистров для считывания.
        /// </summary>
        public ushort NumberOfRegisters { set; get; }

        /// <summary>
        /// Список типов данных, которые содержатся в группе.
        /// </summary>
        public List<ModbusDataType> Types { set; get; }
    }
}
