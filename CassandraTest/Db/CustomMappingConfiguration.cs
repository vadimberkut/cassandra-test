using Cassandra.Mapping;
using CassandraTest.Models;

namespace CassandraTest.Db
{
    public class CustomMappingConfiguration : Mappings
    {
        public CustomMappingConfiguration()
        {
            For<UserEntity>()
                .TableName("users")
                .PartitionKey(x => x.Id);

            For<NoteEntity>()
                .TableName("notes")
                .PartitionKey(x => x.Id);
        }
    }
}