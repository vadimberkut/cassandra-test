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
            // configure mappings
            MappingConfiguration.Global.Define<CustomMappingConfiguration>();

            var clusterManager = new ClusterManager();
            
            // create keyspace
            clusterManager.CreateKeyspaceIfNotExists();
            
            using (var session = clusterManager.CreateSession())
            {
                // create test column families
                var keyspaceContext = clusterManager.CreateKeyspaceContext(session);
                keyspaceContext.Init().Wait();
                
                // reset keyspace
                // keyspaceContext.ResetKeyspace().Wait();
            }
            
            // seed them with huge amount of data
            DataSeeder seeder = new DataSeeder(clusterManager);
            seeder.SeedAsync().Wait();
            //seeder.SeedUsingBatchAsync().Wait();
                
            // read that data

            Console.WriteLine("Press ENTER: ");
            Console.ReadKey();
        }
    }
}
