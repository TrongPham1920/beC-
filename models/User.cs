using System.Collections.Generic;

namespace CrudApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public int? Phone { get; set; }
        public string Email { get; set; }


        public List<Todo> ToDos { get; set; }
    }
}