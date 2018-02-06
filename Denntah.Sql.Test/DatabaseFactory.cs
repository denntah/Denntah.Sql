using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace Denntah.Sql.Test
{
    public class DatabaseFactory
    {
        public static NpgsqlConnection CreatePostgres()
        {
            NpgsqlConnection connection = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;User ID=postgres;Password=qwe123;");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS persons
                (
                  id serial,
                  first_name character varying(50),
                  last_name character varying(50),
                  age integer,
                  gender character varying(10),
                  date_created timestamp with time zone DEFAULT NOW(),
                  CONSTRAINT persons_pk PRIMARY KEY (id)
                )");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS document
                (
                  id uuid NOT NULL,
                  name character varying(100),
                  data bytea,
                  date_created timestamp with time zone
                )");

            return connection;
        }
    }
}
