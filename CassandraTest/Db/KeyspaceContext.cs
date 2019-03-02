using System.Collections.Generic;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTest.Extensions;
using CassandraTest.Models;

namespace CassandraTest.Db
{
    public class KeyspaceContext
    {
        private readonly ISession _session = null;

        public KeyspaceContext(ISession session)
        {
            _session = session;
            
            UserTable = new Table<UserEntity>(_session);
            NoteTable = new Table<NoteEntity>(_session);
        }

        public async Task Init()
        {
            await CreateTables();
        }

        public Task CreateTables()
        {
            return Task.WhenAll(new List<Task>()
            {
                Task.Run(() => UserTable.CreateIfNotExists()),
                Task.Run(() => NoteTable.CreateIfNotExists()),
            });
        }

        public Task DropTables()
        {
            return Task.WhenAll(new List<Task>()
            {
                UserTable.DropIfExistsAsync(),
                NoteTable.DropIfExistsAsync(),
            });
        }
        
        public async Task ResetKeyspace()
        {
            await DropTables();
            await CreateTables();
        }
        
        public Table<UserEntity> UserTable { get; set; }
        public Table<NoteEntity> NoteTable { get; set; }
    }
}
