using System.Collections.Generic;
using Cassandra;

namespace CassandraTest.Db
{
    public class ClusterManager
    {
        const string KEYSPACE = "cassandratest";
        private Cluster _cluster = null;
        
        public ClusterManager()
        {
            _cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
        }

        public void CreateKeyspaceIfNotExists()
        {
            var session = _cluster.Connect();
            session.CreateKeyspaceIfNotExists(KEYSPACE, new Dictionary<string, string>()
            {
                {"class", "SimpleStrategy"},
                {"replication_factor", "3"},
            });
        }

        public ISession CreateSession()
        {
            return _cluster.Connect(KEYSPACE);
        } 
        
        public KeyspaceContext CreateKeyspaceContext(ISession session)
        {
            return new KeyspaceContext(session);
        } 
    }
}