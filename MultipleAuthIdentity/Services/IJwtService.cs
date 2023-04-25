using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.DTO;

namespace MultipleAuthIdentity.Services
{
    public interface IJwtService
    {
        public LoginJwtResponse CreateToken(AppUser user);
        public bool VerifyToken();
    }
}
