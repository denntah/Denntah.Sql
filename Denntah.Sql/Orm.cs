using Denntah.Sql.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Denntah.Sql
{
    /// <summary>
    /// Extensions for objects to insert, update, delete etc.
    /// </summary>
    public static class OrmExtension
    {
        /// <summary>
        /// Get a object of T by its ID
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="ids">Id(s) in same order as marked with KeyAttribute</param>
        /// <returns>T</returns>
        public static T Get<T>(this IDbConnection conn, params object[] ids)
            where T : class, new()
        {
            TypeDescriber td = TypeHandler.Get<T>();
            PropertyDescriber[] keys = td.Keys.ToArray();

            if (keys.Length == 0)
                throw new ArgumentException("T must be a type with properties decorated with KeyAttribute");
            else if (keys.Length != ids.Length)
                throw new ArgumentException(string.Format("KeyAttribute-count ({0}) and argument-count ({1}) must match", keys.Length, ids.Length));
            else
            {
                string where = string.Join(",", keys.Select(x => x.DbName + "=@" + x.Property.Name));
                string sql = $"SELECT * FROM {td.Table} WHERE {where}";

                IDbCommand cmd = conn.Prepare(sql);
                for (var i = 0; i < ids.Length; i++)
                    cmd.ApplyParameter(keys[i].Property.Name, ids[i]);

                return conn.Query<T>(cmd).FirstOrDefault();
            }
        }

        /// <summary>
        /// Insert object in table. Will populate generated fields with values from database.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Insert<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> generated = td.Generated;

            if (generated.Count() > 0)
            {
                string columns = td.WriteableColumns.Select(x => x.DbName).Aggregate((a, b) => a + "," + b);
                string values = td.WriteableColumns.Select(x => "@" + x.Property.Name).Aggregate((a, b) => a + "," + b);
                string returns = string.Join(",", generated.Select(x => x.DbName));

                string sql = $"INSERT INTO {td.Table} ({columns}) VALUES ({values}) RETURNING {returns}";

                var result = conn.QueryAssoc(sql, data, transaction).FirstOrDefault();

                foreach (var prop in generated)
                    td.SetValue(prop.Property.Name, data, result[prop.DbName]);

                return 1;
            }
            else
                return conn.Insert(td.Table, data, transaction);
        }

        /// <summary>
        /// Insert objects in table. Will not populate generated fields.
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="dataList">List of objects containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Insert<T>(this IDbConnection conn, IEnumerable<T> dataList, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();

            return conn.Insert(td.Table, dataList, transaction);
        }

        /// <summary>
        /// Insert object in table or ignores if exists
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int InsertIfMissing<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
            where T : class
        {
            return conn.InsertIfMissing<T>(new List<T> { data }, transaction);
        }

        /// <summary>
        /// Insert objects in table or ignores if exists
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="dataList">List of objects containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int InsertIfMissing<T>(this IDbConnection conn, IEnumerable<T> dataList, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> keys = td.Keys;

            if (keys.Count() > 0)
            {
                return conn.InsertIfMissing(td.Table, dataList, string.Join(",", keys.Select(x => x.DbName)), transaction);
            }
            else
                throw new ArgumentException("Invalid object. Atleast one property must be marked with KeyAttribute on type " + dataList.First().GetType().Name);
        }

        /// <summary>
        /// Insert object in table or update if exists
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>true when inserted, false when updated</returns>
        public static bool Upsert<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
            where T : class
        {
            return conn.Upsert<T>(new List<T> { data }, transaction).First();
        }

        /// <summary>
        /// Insert objects in table or updates if exists
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="dataList">List of objects containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>List of true when inserted, false when updated</returns>
        public static IEnumerable<bool> Upsert<T>(this IDbConnection conn, IEnumerable<T> dataList, IDbTransaction transaction = null)
            where T : class
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> keys = td.Keys;

            if (keys.Count() > 0)
            {
                return conn.Upsert(td.Table, dataList, string.Join(",", keys.Select(x => x.DbName)), transaction);
            }
            else
                throw new ArgumentException("Invalid object. Atleast one property must be marked with KeyAttribute on type " + dataList.First().GetType().Name);
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
