using Cassandra;

namespace CassandraTest.Db
{
    public class ClusterManager
    {
        private Cluster _cluster = null;
        
        public ClusterManager()
        {
            _cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
        }

        public ISession CreateSession(string keySpace)
        {
            return _cluster.Connect(keySpace);
        } 
        
        public KeyspaceContext CreateKeyspaceContext(ISession session)
        {
            return new KeyspaceContext(session);
        } 
    }
}