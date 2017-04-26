using System;
using System.Collections.Generic;

namespace Wiki.ServiceMonitor.Utils
{
    class VersionCompare : Comparer<string>
    {
        public override int Compare(string x, string y)
        {
            if (x == y)
            {
                return 0;
            }
            var aComponents = x.Split('.');
            var bComponents = y.Split('.');

            var len = Math.Min(aComponents.Length, bComponents.Length);

            for (int i = 0; i < len; i++)
            {
                if (int.Parse(aComponents[i]) > int.Parse(bComponents[i]))
                {
                    return -1;
                }
                if (int.Parse(aComponents[i]) < int.Parse(bComponents[i]))
                {
                    return 1;
                }
            }

            if (aComponents.Length > bComponents.Length)
            {
                return -1;
            }

            if (aComponents.Length < bComponents.Length)
            {
                return 1;
            }

            return 0;
        }

    }
}
