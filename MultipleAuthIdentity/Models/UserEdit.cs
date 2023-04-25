using MultipleAuthIdentity.Areas.Identity.Data;

namespace MultipleAuthIdentity.Models
{
    public class UserEdit
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Role { get; set; }

        public UserEdit() { }

        public UserEdit(AppUser user,string role) 
        {
            Id = user.Id;
            Email = user.Email;
            PhoneNumber= user.PhoneNumber;
            Role = role;
        }

    }
}
