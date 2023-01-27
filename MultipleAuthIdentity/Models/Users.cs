using MultipleAuthIdentity.Areas.Identity.Data;

namespace MultipleAuthIdentity.Models
{
    public class Users
    {
       public string Id { get; set; }
       public string Email { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }

        public Users(string id, string email, string role, string phone)
        {
            Id = id;
            Email = email;
            Role = role;
            Phone = phone;
        }
    }
}
