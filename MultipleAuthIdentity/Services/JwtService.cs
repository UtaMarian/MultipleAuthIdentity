using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MultipleAuthIdentity.Areas.Identity.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MultipleAuthIdentity.Services
{
    public class JwtService
    {
        
            private readonly IConfiguration _configuration;
            private readonly UserManager<AppUser>  _userManager;
            JwtService(IConfiguration configuration, UserManager<AppUser> userManager)
            {
                _configuration = configuration;
                _userManager = userManager;
            }
            public string CreateToken(AppUser user)
            {

                var role = _userManager.GetRolesAsync(user);
            

                List<Claim> claims = new List<Claim>
                {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, role.Result.FirstOrDefault()),


                };

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                    _configuration.GetSection("AppSettings:Token").Value));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

                var token = new JwtSecurityToken(
                    issuer: "Licenta2022Backend",
                    claims: claims,
                    expires: DateTime.Now.AddHours(8),
                    signingCredentials: creds);

                var jwt = new JwtSecurityTokenHandler().WriteToken(token);

                return jwt;
            }
        }
}
