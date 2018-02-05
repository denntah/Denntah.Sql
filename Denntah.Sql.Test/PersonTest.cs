using Denntah.Sql.Test.Models;
using System;
using System.Data;
using System.Linq;
using System.Text;
using Xunit;

namespace Denntah.Sql.Test
{
    public class PersonTest : IDisposable
    {
        private IDbConnection _db;
        private Person _data;

        public PersonTest()
        {
            _db = DatabaseFactory.CreatePostgres();

            _data = new Person
            {
                FirstName = "Foo",
                LastName = "Bar",
                Gender = Gender.Male,
                DateCreated = DateTime.Now
            };
        }

        [Fact]
        public void Insert()
        {
            int id = _db.Insert<int>("persons", _data, "id");

            Assert.True(id > 0);
        }

        [Fact]
        public void Update()
        {
            _data.Id = _db.Insert<int>("persons", _data, "id");
            _data.FirstName = "Baz";
            _data.Age = 20;

            int affected = _db.Update("persons", _data, "id=@Id");

            Person updated = _db.Query<Person>("SELECT * FROM persons WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(updated);
            Assert.Equal("Baz", updated.FirstName);
            Assert.Equal(20, updated.Age);
        }

        [Fact]
        public void Delete()
        {
            _data.Id = _db.Insert<int>("persons", _data, "id");

            int affected = _db.Delete("persons", "id=@id", _data);

            Assert.Equal(1, affected);
        }

        [Fact]
        public void Query()
        {
            _data.Id = _db.Insert<int>("persons", _data, "id");

            Person person = _db.Query<Person>("SELECT * FROM persons WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(person);
            Assert.Equal(_data.Id, person.Id);
            Assert.Equal(_data.FirstName, person.FirstName);
            Assert.Equal(_data.Age, person.Age);
            Assert.Equal(_data.Gender, person.Gender);
            Assert.Equal(_data.DateCreated.ToString(), person.DateCreated.ToString());
        }

        [Fact]
        public void QueryAssoc()
        {
            _data.Id = _db.Insert<int>("persons", _data, "id");

            var person = _db.QueryAssoc("SELECT * FROM persons WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(person);
            Assert.Equal(_data.Id, int.Parse(person["id"].ToString()));
            Assert.Equal(_data.FirstName, person["first_name"]);
            Assert.Equal(_data.Age, person["age"]);
            Assert.Equal(_data.Gender.ToString(), person["gender"]);
            Assert.Equal(_data.DateCreated.ToString(), person["date_created"].ToString());
        }

        [Fact]
        public void QueryArray()
        {
            _data.Id = _db.Insert<int>("persons", _data, "id");

            var person = _db.QueryArray("SELECT id,first_name,age,gender,date_created FROM persons WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(person);
            Assert.Equal(_data.Id, int.Parse(person[0].ToString()));
            Assert.Equal(_data.FirstName, person[1]);
            Assert.Equal(_data.Age, person[2]);
            Assert.Equal(_data.Gender.ToString(), person[3]);
            Assert.Equal(_data.DateCreated.ToString(), person[4].ToString());
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}