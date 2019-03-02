using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Data.Linq;
using CassandraTest.Models;

namespace CassandraTest.Db
{
    public class DataSeeder
    {
        const int USER_COUNT = 2000;
        const int USER_NOTE_COUNT = 100;

        const int CHUNK_SIZE = 50;
        const int BATCH_SIZE = 10;
        
        private readonly ClusterManager _clusterManager = null;
        
        public DataSeeder(ClusterManager clusterManager)
        {
            _clusterManager = clusterManager;
        }

        public async Task SeedAsync()
        {
            int chunkCount = (int)Math.Ceiling((double)USER_COUNT / (double)CHUNK_SIZE);

            ConcurrentBag<int> precessedChunks = new ConcurrentBag<int>();
            
            Console.WriteLine($"Processing chunks: {chunkCount} ...");
            var tasks = new List<Task>();
            for (int chunk = 0; chunk < chunkCount; chunk++)
            {
                var task = Task.Run(async () =>
                {
                    // Create separate session for chunk
                    var session = _clusterManager.CreateSession();
                    var keyspaceContext = _clusterManager.CreateKeyspaceContext(session);
                                
                    Random random = new Random();
                    var sessionTasks = new List<Task>();
                    for (int i = 0; i < CHUNK_SIZE; i++)
                    {
                        var user = new UserEntity
                        {
                            Email = $"test-user-{i}@test.com",
                            PasswordHash = $"password-hash-{i}",
                            FirstName = $"John{i}",
                            LastName = $"Doe{i}",
                            Age = random.Next(13, 76),
                        };
                        sessionTasks.Add(keyspaceContext.UserTable.Insert(user).ExecuteAsync());

                        for (int j = 0; j < USER_NOTE_COUNT; j++)
                        {
                            var note = new NoteEntity
                            {
                                UserId = user.Id,
                                Title = $"Test note {i} {j}",
                                Text = $"Blah Blah Blah Blah Blah Blah Blah {i} {j}",
                                // Order = j,
                            };
                            sessionTasks.Add(keyspaceContext.NoteTable.Insert(note).ExecuteAsync());
                        }

                        // cassandara session has max pending connections restrictions
                        // TODO: use some sort of manager or Task queue to process them in  batches (Tasks)
                        if (sessionTasks.Count >= 1000)
                        {
                            await Task.WhenAll(sessionTasks);
                            sessionTasks.Clear();
                        }
                    }
                
                    precessedChunks.Add(chunk);
                    Console.WriteLine($"Users {precessedChunks.Count * CHUNK_SIZE}/{USER_COUNT}. Chunk: {chunk + 1}/{chunkCount}");
                });
                
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
        }
        
        /**
         * Cassandara batch is used to achieve atomicity and
         * not to insert data in a batch.
         * If batch is addressed to different partitions (rows)
         * it will cause latency.
         * Better option is to use single async inserts.
         */
        public async Task SeedUsingBatchAsync()
        {
            int chunkCount = (int)Math.Ceiling((double)USER_COUNT / (double)BATCH_SIZE);

            ConcurrentBag<int> precessedChunks = new ConcurrentBag<int>();
            
            Console.WriteLine($"Processing chunks: {chunkCount} ...");
            Parallel.For(0, chunkCount, chunk =>
            {
                // Create separate session for chunk
                var session = _clusterManager.CreateSession();
                var keyspaceContext = _clusterManager.CreateKeyspaceContext(session);

                // prepare data
                var users = new List<UserEntity>();
                var notes = new List<NoteEntity>();
                
                Random random = new Random();
                for (int i = 0; i < CHUNK_SIZE; i++)
                {
                    var user = new UserEntity
                    {
                        Email = $"test-user-{i}@test.com",
                        PasswordHash = $"password-hash-{i}",
                        FirstName = $"John{i}",
                        LastName = $"Doe{i}",
                        Age = random.Next(13, 76),
                    };
                    users.Add(user);

                    for (int j = 0; j < USER_NOTE_COUNT; j++)
                    {
                        var note = new NoteEntity
                        {
                            UserId = user.Id,
                            Title = $"Test note {i} {j}",
                            Text = $"Blah Blah Blah Blah Blah Blah Blah {i} {j}",
                            // Order = j,
                        };
                        notes.Add(note);
                    }
                }
                
                // batch insert
                var userProps = typeof(UserEntity).GetProperties();
                var noteProps = typeof(NoteEntity).GetProperties();
                var userPropsCql = String.Join(", ", userProps.Select(x => x.Name.ToLower()));
                var notePropsCql = String.Join(", ", noteProps.Select(x => x.Name.ToLower()));
                var userPropsParamsCql = String.Join(", ", userProps.Select(x => "?"));
                var notePropsParamsCql = String.Join(", ", noteProps.Select(x => "?"));
                
                var userInsertStatement = session.Prepare($"INSERT INTO {keyspaceContext.UserTable.Name} ({userPropsCql}) VALUES({userPropsParamsCql})");
                var noteInsertStatement = session.Prepare($"INSERT INTO {keyspaceContext.NoteTable.Name} ({notePropsCql}) VALUES({notePropsParamsCql})");
                
                var batch1 = new BatchStatement();
                var batch2 = new BatchStatement();
                foreach (var user in users)
                {
                    var bindParams = new List<object>();
                    bindParams.AddRange(userProps.Select(x => x.GetValue(user)));
                    batch1.Add(userInsertStatement.Bind(bindParams.ToArray()));
                }
                foreach (var note in notes)
                {
                    var bindParams = new List<object>();
                    bindParams.AddRange(noteProps.Select(x => x.GetValue(note)));
                    batch2.Add(noteInsertStatement.Bind(bindParams.ToArray()));
                }

                session.Execute(batch1);
                session.Execute(batch2);
                
                precessedChunks.Add(chunk);
                Console.WriteLine($"Users {precessedChunks.Count * CHUNK_SIZE}/{USER_COUNT}. Chunk: {chunk + 1}/{chunkCount}");
            });
        }
    }
}