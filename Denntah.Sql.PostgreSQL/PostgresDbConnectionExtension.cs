﻿using Denntah.Sql.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Denntah.Sql
{
    public static class PostgresDbConnectionExtension
    {
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
        {
            TypeDescriber td = TypeHandler.Get(data);

            string columns = td.Readable.Select(x => x.DbName).Aggregate((a, b) => a + "," + b);
            string values = td.Readable.Select(x => "@" + x.Property.Name).Aggregate((a, b) => a + "," + b);

            string sql = $"INSERT INTO {table} ({columns}) VALUES ({values}) RETURNING {pk}";

            return conn.Scalar<Tid>(sql, data, transaction);
        }

        /// <summary>
        /// Insert object to table and populates key properties with generated values from database
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Insert<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
        {
            TypeDescriber td = TypeHandler.Get<T>();
            IEnumerable<PropertyDescriber> generated = td.Generated;

            if (generated.Count() > 0)
            {
                string columns = td.Readable.Select(x => x.DbName).Aggregate((a, b) => a + "," + b);
                string values = td.Readable.Select(x => "@" + x.Property.Name).Aggregate((a, b) => a + "," + b);
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
        /// Update object in table
        /// </summary>
        /// <typeparam name="T">type of object</typeparam>
        /// <param name="conn">A connection</param>
        /// <param name="data">Object containing the data</param>
        /// <param name="transaction">Transaction to associate with the command</param>
        /// <returns>Rows affected</returns>
        public static int Update<T>(this IDbConnection conn, T data, IDbTransaction transaction = null)
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