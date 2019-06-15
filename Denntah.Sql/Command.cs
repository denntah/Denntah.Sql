using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Denntah.Sql
{
    /// <summary>
    /// Internal extension for commands
    /// </summary>
    internal static class CommandExtension
    {
        /// <summary>
        /// Prepares a command and makes sure connection is open
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-command to be executed</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>The created command</returns>
        public static IDbCommand Prepare(this IDbConnection conn, string sql, IDbTransaction transaction = null)
        {
            IDbCommand cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = transaction;

            if (conn.State == ConnectionState.Closed)
                conn.Open();

            return cmd;
        }

        /// <summary>
        /// Add parameter to a command
        /// </summary>
        /// <param name="cmd">Command to add parameter to</param>
        /// <param name="key">Key of the parameter</param>
        /// <param name="value">Value of the parameter</param>
        public static void ApplyParameter(this IDbCommand cmd, string key, object value)
        {
            IDbDataParameter parameter = cmd.CreateParameter();
            parameter.ParameterName = key;
            parameter.Value = value;
            cmd.Parameters.Add(parameter);
        }

        /// <summary>
        /// Add parameters to a command
        /// </summary>
        /// <param name="cmd">Command to add parameters to</param>
        /// <param name="args">Object that holds the parameters</param>
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

        /// <summary>
        /// Add parameters to an indexed bulk command
        /// </summary>
        /// <param name="cmd">Command to add parameters to</param>
        /// <param name="argsList">List of objects that holds the parameters</param>
        public static void ApplyParameters(this IDbCommand cmd, IEnumerable<object> argsList = null)
        {
            if (argsList == null) return;

            var typeDescriber = TypeHandler.Get(argsList.First());

            int i = 0;
            foreach (var args in argsList)
            {
                foreach (var property in typeDescriber.Arguments)
                {
                    var value = typeDescriber.GetValue(property.Property.Name, args);

                    if (property.Property.PropertyType.IsEnum)
                        value = value.ToString();

                    cmd.ApplyParameter(property.Property.Name + i, value ?? DBNull.Value);
                }
                i++;
            }
        }
    }
}
