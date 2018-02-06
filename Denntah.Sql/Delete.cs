using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Denntah.Sql
{
    public static class DeleteExtension
    {
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
        /// Delete object from table
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Delete<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> keys = td.Keys;

            if (keys.Count() > 0)
                return conn.Delete(td.Table, string.Join(",", keys.Select(x => x.DbName + "=@" + x.Property.Name)), data, transaction);
            else
                throw new ArgumentException("Invalid object. Atleast one property must be marked with KeyAttribute on type " + data.GetType().Name);
        }
    }
}
