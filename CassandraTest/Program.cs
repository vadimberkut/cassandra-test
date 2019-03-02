using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            // seeder.SeedAsync().Wait();
            //seeder.SeedUsingBatchAsync().Wait();
                
            // read that data
            using (var session = clusterManager.CreateSession())
            {
                var keyspaceContext = clusterManager.CreateKeyspaceContext(session);

                var notes = keyspaceContext.NoteTable.SetPageSize(1000).Take(10000).ExecutePaged();
                
                Console.WriteLine("PagingState:");
                Console.WriteLine(notes.PagingState.ToString());
                Console.WriteLine(notes.PagingState.Length.ToString());
                Console.WriteLine(Encoding.UTF8.GetString(notes.PagingState));
                Console.WriteLine();
                
                //var noteCount = keyspaceContext.NoteTable.Count().Execute();
                //Console.WriteLine($"Notes: {noteCount}");
                using (var stream = new StreamWriter("notes.debug.txt", false))
                {
                    foreach (var note in notes)
                    {
                        var text = $"{note.UserId} {note.Title}";
                        stream.WriteLine(text);
                    }
                    Console.WriteLine();
                }
            }

            Console.WriteLine("Press ENTER: ");
            Console.ReadKey();
        }
    }
}
