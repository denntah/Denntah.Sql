using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Denntah.Sql
{
    /// <summary>
    /// Extensions for IDbConnection that simplifies database communication.
    /// </summary>
    public static class ConnectionExtension
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

            return conn.Query<T>(cmd);
        }

        /// <summary>
        /// Read data from database
        /// </summary>
        /// <typeparam name="T">class to map data to</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="cmd">Command to be executed</param>
        /// <returns>List of T</returns>
        public static IEnumerable<T> Query<T>(this IDbConnection conn, IDbCommand cmd) where T : new()
        {
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

        /// <summary>
        /// Insert row in a table
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="table">Name of table to insert into</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Insert(this IDbConnection conn, string table, object data, IDbTransaction transaction = null)
        {
            TypeDescriber td = TypeHandler.Get(data);

            string columns = td.WriteableColumns.Select(x => x.DbName).Aggregate((a, b) => a + "," + b);
            string values = td.WriteableColumns.Select(x => "@" + x.Property.Name).Aggregate((a, b) => a + "," + b);

            string sql = $"INSERT INTO {table} ({columns}) VALUES ({values})";

            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(data);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Insert row in a table and returns generated id
        /// </summary>
        /// <typeparam name="Tid">type of primary key field</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="table">Name of table to insert into</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="pk">Name of primary key field</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>value of generated id</returns>
        public static Tid Insert<Tid>(this IDbConnection conn, string table, object data, string pk, IDbTransaction transaction = null)
            where Tid : struct
        {
            TypeDescriber td = TypeHandler.Get(data);

            string columns = td.WriteableColumns.Select(x => x.DbName).Aggregate((a, b) => a + "," + b);
            string values = td.WriteableColumns.Select(x => "@" + x.Property.Name).Aggregate((a, b) => a + "," + b);

            string sql = $"INSERT INTO {table} ({columns}) VALUES ({values}) RETURNING {pk}";

            return conn.Scalar<Tid>(sql, data, transaction);
        }

        /// <summary>
        /// Update row(s) in a table matching the where clause
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="table">Name of table to update</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="where">Where clause e.g. "id=@id"</param>
        /// <param name="args">Additional arguments to apply other then data e.g. new { id = 1 }</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Update(this IDbConnection conn, string table, object data, string where, object args = null, IDbTransaction transaction = null)
        {
            TypeDescriber td = TypeHandler.Get(data);

            string set = td.WriteableColumns.Select(x => x.DbName + "=@" + x.Property.Name).Aggregate((a, b) => a + "," + b);

            string sql = $"UPDATE {table} SET {set} WHERE {where}";

            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(data);
            cmd.ApplyParameters(args);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Delete row(s) in a table matching the where clause
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="table">Name of table to update</param>
        /// <param name="where">Where clause e.g. "id=@id"</param>
        /// <param name="args">Arguments to apply e.g. new { id = 1 }</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Delete(this IDbConnection conn, string table, string where, object args = null, IDbTransaction transaction = null)
        {
            return conn.Execute($"DELETE FROM {table} WHERE {where}", args, transaction);
        }

        /// <summary>
        /// Read first value of first row
        /// </summary>
        /// <typeparam name="T">type to read</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-statement to be executed</param>
        /// <param name="data">Arguments to apply on SQL-statement</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>First value of first row as T</returns>
        public static T Scalar<T>(this IDbConnection conn, string sql, object data = null, IDbTransaction transaction = null)
        {
            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(data);

            return (T)cmd.ExecuteScalar();
        }

        /// <summary>
        /// Executes SQL-statement
        /// </summary>
        /// <param name="conn">A connection</param>
        /// <param name="sql">SQL-statement to be executed</param>
        /// <param name="data">Arguments to apply on SQL-statement</param>
        /// <returns>Rows affected</returns>
        public static int Execute(this IDbConnection conn, string sql, object data = null, IDbTransaction transaction = null)
        {
            IDbCommand cmd = conn.Prepare(sql, transaction);
            cmd.ApplyParameters(data);

            return cmd.ExecuteNonQuery();
        }
    }
}
