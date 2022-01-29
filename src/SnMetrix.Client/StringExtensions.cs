using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnMetrix.Client
{
    public static class StringExtensions
    {
        /// <summary>
        /// Trims the schema and trailing slashes from a url.
        /// </summary>
        public static string TrimSchema(this string url)
        {
            if (url == null)
                return null;

            var schIndex = url.IndexOf("://", StringComparison.OrdinalIgnoreCase);

            return (schIndex >= 0 ? url.Substring(schIndex + 3) : url).Trim('/', ' ');
        }

        /// <summary>
        /// Appends an 'https://' prefix to a url if it is missing.
        /// </summary>
        public static string AppendSchema(this string url)
        {
            if (string.IsNullOrEmpty(url) || url.StartsWith("http"))
                return url;

            return "https://" + url;
        }
    }
}
