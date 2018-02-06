using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Denntah.Sql
{
    public static class QueryExtension
    {
        /// <summary>
        /// Read data from database
        /// </summary>
        /// <typeparam name="T">class to map data to</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-statement to be executed</param>
        /// <param name="args">Arguments to apply on SQL-statement</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>List of T</returns>
        public static IEnumerable<T> Query<T>(this IDbConnection conn, string sql, object args = null, IDbTransaction transaction = null) where T : new()
        {
            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(args);

            using (var reader = cmd.ExecuteReader())
            {
                List<string> columns = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));

                var td = TypeHandler.Get<T>();

                while (reader.Read())
                {
                    var result = new T();

                    for (var i = 0; i < columns.Count; i++)
                    {
                        object value = reader[columns[i]];

                        td.SetValue(columns[i], result, value is DBNull ? null : value);
                    }

                    yield return result;
                }
            }
        }

        /// <summary>
        /// Read data from database
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-statement to be executed</param>
        /// <param name="args">Arguments to apply on SQL-statement</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>List of array of object</returns>
        public static IEnumerable<object[]> QueryArray(this IDbConnection conn, string sql, object args = null, IDbTransaction transaction = null)
        {
            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(args);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    object[] result = new object[reader.FieldCount];

                    for (var i = 0; i < reader.FieldCount; i++)
                        result[i] = reader[i] is DBNull ? null : reader[i];

                    yield return result;
                }
            }
        }

        /// <summary>
        /// Read data from database
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-statement to be executed</param>
        /// <param name="args">Arguments to apply on SQL-statement</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>List of dictionary, where key is column name</returns>
        public static IEnumerable<IDictionary<string, object>> QueryAssoc(this IDbConnection conn, string sql, object args = null, IDbTransaction transaction = null)
        {
            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(args);

            using (var reader = cmd.ExecuteReader())
            {
                List<string> columns = new List<string>();

                for (int i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));

                while (reader.Read())
                {
                    Dictionary<string, object> result = new Dictionary<string, object>();

                    for (var i = 0; i < reader.FieldCount; i++)
                        result[columns[i]] = reader[i] is DBNull ? null : reader[i];

                    yield return result;
                }
            }
        }
    }
}
