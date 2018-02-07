using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Denntah.Sql
{
    public static class UpdateExtension
    {
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
        /// Update object in table
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Update<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> keys = td.Keys;

            if (keys.Count() > 0)
                return conn.Update(td.Table, data, string.Join(",", keys.Select(x => x.DbName + "=@" + x.Property.Name)), transaction);
            else
                throw new ArgumentException("Invalid object. Atleast one property must be marked with KeyAttribute on type " + data.GetType().Name);
        }
    }
}
