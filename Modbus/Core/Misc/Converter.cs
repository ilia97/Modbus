using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Misc
{
    /// <summary>
    /// Класс, в котором хранятся все дополнительные методы для преобразования типов.
    /// </summary>
    public static class Converter
    {
        public static string ConvertToString(this bool[] bitsArray)
        {
            byte[] strArr = new byte[bitsArray.Length / 8];

            for (int i = 0; i < bitsArray.Length / 8; i++)
            {
                for (int index = i * 8 + 7, m = 1; index >= i * 8; index--, m *= 2)
                {
                    strArr[i] += bitsArray[index] ? (byte)m : (byte)0;
                }
            }

            return new ASCIIEncoding().GetString(strArr);
        }

        public static bool[] ConvertToBitArray(this ushort[] registersArray)
        {
            var result = new List<bool>();

            for (var i = 0; i < registersArray.Length; i++)
            {
                var register = registersArray[i];

                for (var k = 15; k >= 0; k--)
                {
                    if (register >= Math.Pow(2, k))
                    {
                        result.Add(true);
                        register -= (ushort)Math.Pow(2, k);
                    }
                    else
                    {
                        result.Add(false);
                    }
                }
            }

            return result.ToArray();
        }

        public static string ConvertToHex(this bool[] inputs)
        {
            // Преобразуем массив битов в строку, состоящую из единиц и нулей.
            var binaryHexString = inputs.Select(x => x ? 1 : 0);

            // Конвертируем полученное десятичное число в знаковое 32-битное целое число, после этого конвертируем число в 16-ричную систему счисления.
            return $"0x{Convert.ToUInt32(string.Join("", binaryHexString), 2).ToString("X")}";
        }
    }
}
