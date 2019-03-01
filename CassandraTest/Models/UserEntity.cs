namespace CassandraTest.Models
{
    public class UserEntity : BaseEntity
    {
        public UserEntity() : base()
        {
            
        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public int Age { get; set; }
    }
}