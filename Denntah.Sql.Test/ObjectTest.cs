using Denntah.Sql.Reflection;
using Denntah.Sql.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Denntah.Sql.Test
{
    public class ObjectTest
    {
        [Fact]
        public void Get()
        {
            Person person = null;
            int id = 0;

            using (var db = DatabaseFactory.CreatePostgres())
            {
                id = db.Insert<int>("persons", new Person { FirstName = "Foo" }, "id");
                person = db.Get<Person>(id);
            }

            Assert.NotNull(person);
            Assert.Equal(id, person.Id);
            Assert.Equal("Foo", person.FirstName);
        }

        [Fact]
        public void GetByMultipleIds()
        {
            int parentId = 1;
            int childId = 2;
            ParentChild parentChild = null;

            using (var db = DatabaseFactory.CreatePostgres())
            {
                parentChild = db.Get<ParentChild>(parentId, childId);
                if (parentChild == null)
                {
                    db.Insert(new ParentChild { ParentId = parentId, ChildId = childId });
                    parentChild = db.Get<ParentChild>(parentId, childId);
                }
            }

            Assert.NotNull(parentChild);
            Assert.Equal(parentId, parentChild.ParentId);
            Assert.Equal(childId, parentChild.ChildId);
        }

        [Fact]
        public void GetObjectWithoutKey()
        {
            using (var db = DatabaseFactory.CreatePostgres())
                Assert.Throws<ArgumentException>(() => db.Get<Document>(Guid.NewGuid()));
        }

        [Fact]
        public void GetObjectWhereKeyCountDoesntMatch()
        {
            using (var db = DatabaseFactory.CreatePostgres())
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

            using (var db = DatabaseFactory.CreatePostgres())
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

            using (var db = DatabaseFactory.CreatePostgres())
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

            using (var db = DatabaseFactory.CreatePostgres())
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
        public void UpdateObjectWithoutKey()
        {
            Document document = new Document
            {
                Name = "foo.txt"
            };

            using (var db = DatabaseFactory.CreatePostgres())
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

            using (var db = DatabaseFactory.CreatePostgres())
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

            using (var db = DatabaseFactory.CreatePostgres())
                Assert.Throws<ArgumentException>(() => db.Delete(document));
        }
    }
}
