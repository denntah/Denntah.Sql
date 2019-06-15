using Xunit;

namespace Denntah.Sql.Test
{
    public class DatabaseFixture
    {
        public DatabaseFixture()
        {
            DatabaseFactory.CreatePostgres();
        }
    }

    [CollectionDefinition("DBTest")]
    public class DBTest : ICollectionFixture<DatabaseFixture> { }
}