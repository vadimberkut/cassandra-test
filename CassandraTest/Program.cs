using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using Cassandra.Mapping;
using CassandraTest.Db;
using CassandraTest.Models;

namespace CassandraTest
{
    class Program
    {
        static void Main(string[] args)
        {
            const string KEYSPACE = "cassandratest";
            
            // configure mappings
            MappingConfiguration.Global.Define<CustomMappingConfiguration>();

            var clusterManager = new ClusterManager();
            
            using (var session = clusterManager.CreateSession(KEYSPACE))
            {
                // create keyspace
                session.CreateKeyspaceIfNotExists(KEYSPACE, new Dictionary<string, string>()
                {
                    {"class", "SimpleStrategy"},
                    {"replication_factor", "3"},
                });
                session.ChangeKeyspace(KEYSPACE);
                
                // create test column families
                var keyspaceContext = new KeyspaceContext(session);
                keyspaceContext.Init().Wait();
                
                // seed them with huge amount of data
                DataSeeder seeder = new DataSeeder(keyspaceContext);
                seeder.Seed().Wait();
                
                // read that data
            }

            Console.WriteLine("Press ENTER: ");
            Console.ReadKey();
        }
    }
}
