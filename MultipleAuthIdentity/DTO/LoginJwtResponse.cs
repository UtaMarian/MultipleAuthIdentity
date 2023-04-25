using Newtonsoft.Json;

namespace MultipleAuthIdentity.DTO
{
    public class LoginJwtResponse
    {
        public string? Id { get; set; }

        public string? Email { get; set; }


        public string? Name { get; set; }

        public string? Token { get; set; }

        public LoginJwtResponse(string? id, string? email, string? name, string? token)
        {
            this.Id = id;
            this.Email = email;
            this.Name = name;
            this.Token = token;
        }
        
    }
}
