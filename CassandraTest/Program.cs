using System;
using System.Linq;
using Cassandra;

namespace CassandraTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Cluster cluster = Cluster.Builder().AddContactPoint("127.0.0.1").Build();
            ISession session = cluster.Connect("demo");

            session.Execute(@"
                insert into users (lastname, age, city, email, firstname) values ('Jones', 35, 'Austin', 'bob@example.com', 'Bob')
            ");
            
            Row result = session.Execute("select * from users where lastname='Jones'").First();
            Console.WriteLine("{0} {1}", result["firstname"], result["age"]);

            Console.ReadKey();
        }
    }
}
