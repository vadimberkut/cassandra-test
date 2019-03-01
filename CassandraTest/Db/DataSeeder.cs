using System;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTest.Models;

namespace CassandraTest.Db
{
    public class DataSeeder
    {
        private readonly ClusterManager _clusterManager = null;
        
        public DataSeeder(ClusterManager clusterManager)
        {
            _clusterManager = clusterManager;
        }

        public async Task Seed()
        {
            const int USER_COUNT = 100;
            const int USER_NOTE_COUNT = 3;

            var session = _clusterManager.CreateSession("cassandratest");
            var keyspaceContext = _clusterManager.CreateKeyspaceContext(session);
            
            await keyspaceContext.DropTables();
            await keyspaceContext.CreateTables();
            
            // TODO - use chunks
            // for each chunk create separate session
            Parallel.For(0, USER_COUNT, i =>
            {
                if (i % 10 == 0)
                {
                    Console.WriteLine($"Users {i} / {USER_COUNT}");
                }
                var user = new UserEntity
                {
                    Email = $"test-user-{i}@test.com",
                    PasswordHash = $"password-hash-{i}",
                    FirstName = $"John{i}",
                    LastName = $"Doe{i}",
                };
                _keyspaceContext.UserTable.Insert(user).Execute();

                for (int j = 0; j < USER_NOTE_COUNT; j++)
                {
                    var note = new NoteEntity
                    {
                        UserId = user.Id,
                        Title = $"Test note {i} {j}",
                        Text = $"Blah Blah Blah Blah Blah Blah Blah {i} {j}",
                        // Order = j,
                    };
                    _keyspaceContext.NoteTable.Insert(note).Execute();
                }
            });
        }
    }
}