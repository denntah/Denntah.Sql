using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Denntah.Sql
{
    public static class ExecuteExtension
    {
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
