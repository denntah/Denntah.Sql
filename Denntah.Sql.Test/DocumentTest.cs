using Denntah.Sql.Test.Models;
using System;
using System.Data;
using System.Linq;
using System.Text;
using Xunit;

namespace Denntah.Sql.Test
{
    public class DocumentTest : IDisposable
    {
        private IDbConnection _db;
        private Document _data;

        public DocumentTest()
        {
            _db = DatabaseFactory.CreatePostgres();

            _data = new Document
            {
                Id = Guid.NewGuid(),
                Name = "file.txt",
                Data = Encoding.UTF8.GetBytes("Hello world!"),
                DateCreated = DateTime.Now
            };
        }

        [Fact]
        public void Insert()
        {
            _db.Insert("document", _data);

            Guid id = _db.Scalar<Guid>("SELECT id FROM document WHERE id=@Id", _data);

            Assert.Equal(_data.Id, id);
        }

        [Fact]
        public void Update()
        {
            _db.Insert("document", _data);
            _data.Name = "changed.txt";

            int affected = _db.Update("document", _data, "id=@Id");

            string newName = _db.Scalar<string>("SELECT name FROM document WHERE id=@Id", _data);

            Assert.Equal(1, affected);
            Assert.Equal(_data.Name, newName);
        }

        [Fact]
        public void Delete()
        {
            _db.Insert("document", _data);

            int affected = _db.Delete("document", "id=@Id", _data);

            Assert.Equal(1, affected);
        }

        [Fact]
        public void Query()
        {
            _db.Insert("document", _data);

            Document doc = _db.Query<Document>("SELECT * FROM document WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(doc);
            Assert.Equal(_data.Id, doc.Id);
            Assert.Equal(_data.Name, doc.Name);
            Assert.Equal(Encoding.UTF8.GetString(_data.Data), Encoding.UTF8.GetString(doc.Data));
            Assert.Equal(_data.DateCreated.ToString(), doc.DateCreated.ToString());
        }

        [Fact]
        public void QueryAssoc()
        {
            _db.Insert("document", _data);

            var doc = _db.QueryAssoc("SELECT * FROM document WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(doc);
            Assert.Equal(_data.Id, doc["id"]);
            Assert.Equal(_data.Name, doc["name"]);
            Assert.Equal(Encoding.UTF8.GetString(_data.Data), Encoding.UTF8.GetString((byte[])doc["data"]));
            Assert.Equal(_data.DateCreated.ToString(), doc["date_created"].ToString());
        }

        [Fact]
        public void QueryArray()
        {
            _db.Insert("document", _data);

            var doc = _db.QueryArray("SELECT id,name,data,date_created FROM document WHERE id=@Id", _data).FirstOrDefault();

            Assert.NotNull(doc);
            Assert.Equal(_data.Id, doc[0]);
            Assert.Equal(_data.Name, doc[1]);
            Assert.Equal(Encoding.UTF8.GetString(_data.Data), Encoding.UTF8.GetString((byte[])doc[2]));
            Assert.Equal(_data.DateCreated.ToString(), doc[3].ToString());
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}