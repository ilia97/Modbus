using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Misc
{
    public static class PackagesCounter
    {
        public static int RequestedPackagesCount { get; set; }

        public static int RecievedPackagesCount { get; set; }

        public static int LostPackagesCount
        {
            get
            {
                return RecievedPackagesCount - RecievedPackagesCount;
            }
        }
    }
}
