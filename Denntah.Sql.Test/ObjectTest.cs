using Denntah.Sql.Test.Models;
using System;
using System.Linq;
using Xunit;

namespace Denntah.Sql.Test
{
    [Collection("DBTest")]
    public class ObjectTest
    {
        [Fact]
        public void Get()
        {
            Person person = null;
            int id = 0;

            using (var db = DatabaseFactory.Connect())
            {
                id = db.Insert<int>("persons", new Person { FirstName = "Foo" }, "id");
                person = db.Get<Person>(id);
            }

            Assert.NotNull(person);
            Assert.Equal(id, person.Id);
            Assert.Equal("Foo", person.FirstName);
        }

        [Fact]
        public void GetObjectWithoutKey()
        {
            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.Get<Document>(Guid.NewGuid()));
        }

        [Fact]
        public void GetObjectWhereKeyCountDoesntMatch()
        {
            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.Get<Person>(1, 2));
        }

        [Fact]
        public void InsertObject()
        {
            Person person = new Person
            {
                FirstName = "Foo",
                LastName = "Bar"
            };

            using (var db = DatabaseFactory.Connect())
                db.Insert(person);

            Assert.True(person.Id > 0);
            Assert.True(person.DateCreated > DateTime.Now.AddSeconds(-10));
        }

        [Fact]
        public void InsertObjectWithoutKey()
        {
            Document document = new Document
            {
                Name = "foo.txt"
            };

            int affected;

            using (var db = DatabaseFactory.Connect())
                affected = db.Insert(document);

            Assert.True(affected == 1);
        }

        [Fact]
        public void UpdateObject()
        {
            int affected;

            Person person = new Person
            {
                FirstName = "Foo",
                LastName = "Bar"
            };

            using (var db = DatabaseFactory.Connect())
            {
                db.Insert(person);
                person.FirstName = "Baz";
                person.Age = 20;
                person.Gender = Gender.Male;

                affected = db.Update(person);

                person = db.Query<Person>("SELECT * FROM persons WHERE id=@Id", person).FirstOrDefault();
            }

            Assert.True(affected == 1);
            Assert.NotNull(person);
            Assert.Equal("Baz", person.FirstName);
            Assert.Equal("Bar", person.LastName);
            Assert.Equal(Gender.Male, person.Gender);
            Assert.Equal(20, person.Age);
        }

        [Fact]
        public void UpsertObject()
        {
            bool isInsert;
            bool isUpdate;

            Car car = new Car
            {
                Id = "BBB001",
                Make = "VW Golf"
            };

            using (var db = DatabaseFactory.Connect())
            {
                db.Delete(car);

                isInsert = db.Upsert(car);

                car.Make = "Ford Focus";
                isUpdate = !db.Upsert(car);

                car = db.Get<Car>(car.Id);
            }

            Assert.True(isInsert);
            Assert.True(isUpdate);
            Assert.Equal("BBB001", car.Id);
            Assert.Equal("Ford Focus", car.Make);
        }

        [Fact]
        public void UpsertObjectWithoutKey()
        {
            Document document = new Document
            {
                Name = "somedocument.txt"
            };

            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.Upsert(document));
        }

        [Fact]
        public void InsertObjectIfMissing()
        {
            int affected;
            int affected2;

            Car car = new Car
            {
                Id = "BBB002",
                Make = "Saab 9000"
            };

            using (var db = DatabaseFactory.Connect())
            {
                db.Delete(car);

                affected = db.InsertIfMissing(car);

                car.Make = "Seat Leon";
                affected2 = db.InsertIfMissing(car);

                car = db.Get<Car>(car.Id);
            }

            Assert.Equal(1, affected);
            Assert.Equal(0, affected2);
            Assert.Equal("BBB002", car.Id);
            Assert.Equal("Saab 9000", car.Make);
        }

        [Fact]
        public void InsertObjectIfMissingWithoutKey()
        {
            Document document = new Document
            {
                Name = "somedocument.txt"
            };

            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.InsertIfMissing(document));
        }

        [Fact]
        public void UpdateObjectWithoutKey()
        {
            Document document = new Document
            {
                Name = "foo.txt"
            };

            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.Update(document));
        }

        [Fact]
        public void DeleteObject()
        {
            int affected;

            Person person = new Person
            {
                FirstName = "Foo",
                LastName = "Bar"
            };

            using (var db = DatabaseFactory.Connect())
            {
                db.Insert(person);

                affected = db.Delete(person);
            }

            Assert.True(affected == 1);
        }

        [Fact]
        public void DeleteObjectWithoutKey()
        {
            Document document = new Document
            {
                Name = "foo.txt"
            };

            using (var db = DatabaseFactory.Connect())
                Assert.Throws<ArgumentException>(() => db.Delete(document));
        }
    }
}
