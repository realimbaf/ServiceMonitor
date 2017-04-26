using System;
using System.Text.RegularExpressions;

namespace Wiki.DiscoveryService.Utils
{
    public static class CompareDates
    {
        public static bool IsCompareWithNow(string input)
        {
            string pattern = "(?<hour>[0-9]{2}):(?<minute>[0-9]{2}):(?<sec>[0-9]{2})";        
            var matchCollection = Regex.Matches(input, pattern);
            var now = DateTime.Now;
            foreach (Match match in matchCollection)
            {
                if (int.Parse(match.Groups[1].Value) < now.Hour)
                    return false;
                if (int.Parse(match.Groups[2].Value) < now.Minute)
                   return false;
            }
            return true;
        }
    }
}