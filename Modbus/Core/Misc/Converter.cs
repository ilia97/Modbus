using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Misc
{
    public static class Converter
    {
        public static string ConvertToString(this bool[] bitsArray)
        {
            byte[] strArr = new byte[bitsArray.Length / 8];

            for (int i = 0; i < bitsArray.Length / 8; i++)
            {
                for (int index = i * 8, m = 1; index < i * 8 + 8; index++, m *= 2)
                {
                    strArr[i] += bitsArray[index] ? (byte)m : (byte)0;
                }
            }

            return new ASCIIEncoding().GetString(strArr);
        }
    }
}
