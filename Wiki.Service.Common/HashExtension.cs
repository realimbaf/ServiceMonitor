﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace Wiki.Service.Common
{
    public static class HashExtension
    {
        public static string Sha256(this string input)
        {
            if (input == null )
                return string.Empty;
            using (var shA256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(shA256.ComputeHash(bytes));
            }
        }
    }
}
