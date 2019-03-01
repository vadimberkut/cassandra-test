using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;

namespace CassandraTest.Extensions
{
    public static class DastaxTableExtensions
    {
        public static void Drop<T>(this Table<T> table)
        {
            table.DropAsync().Wait();
        }
        
        public static Task DropAsync<T>(this Table<T> table)
        {
            var statement = new SimpleStatement($"DROP TABLE {table.Name}");
            return table.GetSession().ExecuteAsync(statement);
        }

        public static void DropIfExists<T>(this Table<T> table)
        {
            table.DropIfExistsAsync().Wait();
        }
        
        public static Task DropIfExistsAsync<T>(this Table<T> table)
        {
            var statement = new SimpleStatement($"DROP TABLE IF EXISTS {table.Name}");
            return table.GetSession().ExecuteAsync(statement);
        }
    }
}