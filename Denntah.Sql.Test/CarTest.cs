using Denntah.Sql.Test.Models;
using System;
using System.Data;
using System.Linq;
using Xunit;

namespace Denntah.Sql.Test
{
    [Collection("DBTest")]
    public class CarTest : IDisposable
    {
        private IDbConnection _db;

        public CarTest()
        {
            _db = DatabaseFactory.Connect();
        }

        [Fact]
        public void InsertAndUpsert()
        {
            Car _data = new Car
            {
                Id = "AAA001",
                Make = "Audi S4"
            };

            _db.Insert("cars", _data);

            _data.Make = "BMW M5";
            bool isInsert = _db.Upsert("cars", _data, "id");

            Car car = _db.Query<Car>("SELECT * FROM cars WHERE id=@Id", _data).FirstOrDefault();

            Assert.False(isInsert);
            Assert.Equal(_data.Make, car.Make);
        }

        [Fact]
        public void UpsertAndUpsert()
        {
            Car _data = new Car
            {
                Id = "AAA002",
                Make = "VW Passat"
            };

            bool isInsert = _db.Upsert("cars", _data, "id");

            _data.Make = "Volvo V70";
            bool isUpdate = !_db.Upsert("cars", _data, "id");

            Car car = _db.Query<Car>("SELECT * FROM cars WHERE id=@Id", _data).FirstOrDefault();

            Assert.True(isInsert);
            Assert.True(isUpdate);
            Assert.Equal(_data.Make, car.Make);
        }

        [Fact]
        public void InsertIfMissing()
        {
            Car _data = new Car
            {
                Id = "AAA003",
                Make = "Ferrari F40"
            };

            int affected = _db.InsertIfMissing("cars", _data, "id");

            _data.Make = "Lamborghini Aventador";
            int affected2 = _db.InsertIfMissing("cars", _data, "id");

            Car car = _db.Query<Car>("SELECT * FROM cars WHERE id=@Id", _data).FirstOrDefault();

            Assert.Equal(1, affected);
            Assert.Equal(0, affected2);
            Assert.Equal("Ferrari F40", car.Make);
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}