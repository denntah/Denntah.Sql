using System;
using System.Data;
using System.Linq;
using Denntah.Sql.Test.Models;
using Xunit;

namespace Denntah.Sql.Test
{
    [Collection("DBTest")]
    public class BulkTest : IDisposable
    {
        private IDbConnection _db;

        public BulkTest()
        {
            _db = DatabaseFactory.Connect();
        }

        [Fact]
        public void InsertMany()
        {
            var personList = Enumerable.Range(0, 100).Select(i => new Person
            {
                FirstName = "FirstName" + i,
                LastName = "LastName " + i,
                Gender = Gender.Female,
                DateCreated = DateTime.Now
            }).AsEnumerable();

            int rowsAffected = _db.Insert("persons", personList);

            Assert.Equal(100, rowsAffected);
        }

        [Fact]
        public void InsertManyIfMissing()
        {
            var carList = Enumerable.Range(0, 100).Select(i => new Car
            {
                Id = "CCC" + i,
                Make = "Car " + i
            });

            int insertCount1 = _db.InsertIfMissing("cars", carList.Take(25), "id");
            var insertCount2 = _db.InsertIfMissing("cars", carList, "id");

            Assert.Equal(25, insertCount1);
            Assert.Equal(75, insertCount2);
        }

        [Fact]
        public void UpsertMany()
        {
            var carList = Enumerable.Range(0, 100).Select(i => new Car
            {
                Id = "DDD" + i,
                Make = "Car " + i
            });

            var result1 = _db.Upsert("cars", carList, "id").ToList();
            int insertCount = result1.Count(inserted => inserted);

            var result2 = _db.Upsert("cars", carList, "id").ToList();
            int updateCount = result2.Count(inserted => !inserted);

            Assert.Equal(100, insertCount);
            Assert.Equal(100, updateCount);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

    }
}