using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Denntah.Sql
{
    public static class DbCommandExtension
    {
        public static void ApplyParameter(this IDbCommand cmd, string key, object value)
        {
            IDbDataParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = key;
            parameter.Value = value;
            cmd.Parameters.Add(parameter);
        }

        public static void ApplyParameters(this IDbCommand cmd, object args = null)
        {
            if (args == null) return;

            var typeDescriber = TypeHandler.Get(args);

            foreach (var property in typeDescriber.Arguments)
            {
                var value = typeDescriber.GetValue(property.Property.Name, args);

                if (property.Property.PropertyType.IsEnum)
                    value = value.ToString();

                cmd.ApplyParameter(property.Property.Name, value ?? DBNull.Value);
            }
        }
    }
}
