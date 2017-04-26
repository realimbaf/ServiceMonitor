using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Wiki.Service.Common.Clients
{
    /// <summary>
    /// Расширение для HTTP
    /// </summary>
    public static class HttpExtension
    {
        /// <summary>
        /// Преобразовывает содержимое объекта к строке параметров GET запроса
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetUrlParams<T>(this T value)
        {
            if (value == null)
                return "";

            var type = value.GetType();
            var fields = type.GetProperties();
            var strs = new List<string>();
            foreach (var field in fields)
            {
                strs.Add(field.Name + "=" + Uri.EscapeDataString(field.GetValue(value) + ""));
            }
            return string.Join("&",strs);
        }
    }
}