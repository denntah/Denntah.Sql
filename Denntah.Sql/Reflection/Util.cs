using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.RegularExpressions;

namespace Denntah.Sql.Reflection
{
    public class Util
    {
        public static string ToUnderscore(string input)
        {
            return Regex.Replace(input.Trim(), @"(?<=.)([A-Z])", "_$1").ToLower();
        }
    }
}
