using System;
using Npgsql;

namespace Denntah.Sql.Test
{
    public class DatabaseFactory
    {
        public static NpgsqlConnection Connect()
        {
            var conn = new NpgsqlConnection("Host=localhost;Port=5432;Database=test;User ID=postgres;Password=postgres;");

            int timeZoneOffset = TimeZoneInfo.Local.GetUtcOffset(DateTime.UtcNow).Hours;
            conn.Execute($"SET TIME ZONE {timeZoneOffset}");

            return conn;
        }

        public static void CreatePostgres()
        {
            using (var connection = Connect())
            {
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS persons
                    (
                      id serial,
                      first_name text,
                      last_name text,
                      age integer,
                      gender text,
                      date_created timestamptz DEFAULT NOW(),
                      CONSTRAINT persons_pk PRIMARY KEY (id)
                    )");

                connection.Execute(
                    "TRUNCATE TABLE persons CASCADE");

                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS document
                    (
                      id uuid NOT NULL,
                      name text,
                      data bytea,
                      date_created timestamptz,
                      CONSTRAINT document_pk PRIMARY KEY (id)
                    )");

                connection.Execute(
                    "TRUNCATE TABLE document CASCADE");

                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS cars
                    (
                      id text,
                      make text,
                      date_registered timestamptz DEFAULT NOW(),
                      CONSTRAINT cars_pk PRIMARY KEY (id)
                    )");

                connection.Execute(
                    "TRUNCATE TABLE cars CASCADE");
            }
        }
    }
}
