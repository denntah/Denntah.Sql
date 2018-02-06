using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Denntah.Sql
{
    public static class ScalarExtension
    {
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
    }
}
