namespace CrudApi.Models
{
    public class Todo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public int Status { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedDate { get; set; }

        public User User { get; set; }
    }
}
