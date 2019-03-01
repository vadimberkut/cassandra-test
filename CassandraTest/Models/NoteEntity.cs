namespace CassandraTest.Models
{
    public class NoteEntity : BaseEntity
    {
        public NoteEntity() : base()
        {
            
        }

        public string UserId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        // public int Order { get; set; } = 0;
    }
}