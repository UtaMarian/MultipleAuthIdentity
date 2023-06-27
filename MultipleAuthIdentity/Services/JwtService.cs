using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MultipleAuthIdentity.Services
{
    public class JwtService : IJwtService
    {
        
        private readonly IConfiguration _configuration;
        private readonly UserManager<AppUser>  _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

 
        public JwtService(IConfiguration configuration, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public LoginJwtResponse CreateToken(AppUser user)
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
                audience:"marian",
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: creds);

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            LoginJwtResponse res = new LoginJwtResponse(user.Id, user.Email, user.UserName, jwt);
            return res;
        }

  

        public bool VerifyToken()
        {
            var token = _httpContextAccessor.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            return token != null && VerifyValidityToken(token);
        }

        public bool VerifyValidityToken(string token)
        {

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value));

            var validationParameters = new TokenValidationParameters { 
                ValidateIssuerSigningKey = true, 
                IssuerSigningKey = key,
                ValidateIssuer = true,
                ValidIssuer = "Licenta2022Backend", 
                ValidateAudience = true, 
                ValidAudience = "marian",
                RequireExpirationTime = true, 
                ValidateLifetime = true, 
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


    }
}
