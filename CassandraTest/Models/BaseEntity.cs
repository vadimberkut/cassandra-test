using System;

namespace CassandraTest.Models
{
    public class BaseEntity
    {
        protected BaseEntity()
        {
            Id = Guid.NewGuid().ToString();
        }
        
        public string Id { get; set; }
    }
}