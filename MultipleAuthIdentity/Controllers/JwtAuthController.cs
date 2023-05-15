using Google.Apis.Auth;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.DTO;
using MultipleAuthIdentity.Services;
using NuGet.Common;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace MultipleAuthIdentity.Controllers
{

    public class JwtAuthController : Controller
    {
        private readonly AuthDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        public JwtAuthController(AuthDbContext context, UserManager<AppUser> userManager, IConfiguration configuration, IJwtService jwtService)
        {
            _dbcontext = context;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
        }

        [HttpPost("loginJWT")]
        public async Task<ActionResult<LoginJwtResponse>> Login(UserDto request)
        {
            

            AppUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound("User not found");
            }


            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                return BadRequest("Wrong password");
            }

            LoginJwtResponse response = _jwtService.CreateToken(user);

            return response;

        }


        [HttpPost("registerJWT")]
        public async Task<ActionResult<LoginJwtResponse>> Register(UserDto request)
        {
            AppUser? user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                user = new AppUser();
                user.UserName = request.Email;
                user.Email = request.Email;
                await _userManager.AddPasswordAsync(user,request.Password);
                user.NormalizedEmail = request.Email.ToUpper();
                user.NormalizedUserName = request.Email.ToUpper();
                user.TwoFactorEnabled = false;
                user.PhoneNumberConfirmed = false;
                user.EmailConfirmed = false;

                await _userManager.CreateAsync(user);
                user = await _userManager.FindByEmailAsync(request.Email);
                await _userManager.AddToRoleAsync(user, "USER");
                LoginJwtResponse response = _jwtService.CreateToken(user);
                
                return response;
            }
            else
            {
                return NotFound("User already exist");
            }
        }
        
        //[Authorize]
        [HttpGet("info")]
        public ResponseDto testJwtAuth()
        {
            ResponseDto res = new ResponseDto();
            if (_jwtService.VerifyToken())
            {
                res.Message = "Merge";
                return res;
            }
            res.Message = "Nu merge";
            return res;
        }

        [HttpPost("loginGoogle")]
        public async Task<ActionResult<LoginJwtResponse?>> loginWithIdToken(string id)
        {

            // The CLIENT_ID is the audience parameter, which identifies the client that is requesting the token.
            // You can obtain the CLIENT_ID from the Google Cloud Console.
            const string CLIENT_ID = "967836242772-d3sb3adjjephnmr9bpfchfnd5k8qs5ir.apps.googleusercontent.com";

            // The tokenString parameter is the ID token that you want to verify.
            GoogleJsonWebSignature.Payload payload = null;
            try
            {
                GoogleJsonWebSignature.ValidationSettings settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { CLIENT_ID }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(id, settings);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
             
            }

            if (payload != null)
            {

                string userId = payload.Subject;
                string email = payload.Email;

                AppUser? user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    //user = new AppUser();
                    //user.UserName = request.Email;
                    //user.Email = request.Email;
                    //user.NormalizedEmail = request.Email.ToUpper();
                    //user.NormalizedUserName = request.Email.ToUpper();
                    //user.TwoFactorEnabled = false;
                    //user.PhoneNumberConfirmed = false;
                    //user.EmailConfirmed = false;

                    //await _userManager.CreateAsync(user);
                    //user = await _userManager.FindByEmailAsync(request.Email);
                    //await _userManager.AddToRoleAsync(user, "USER");
                    //LoginJwtResponse response = _jwtService.CreateToken(user);
                }
                else
                {
                    LoginJwtResponse response = _jwtService.CreateToken(user);
                }
                

            }
            return Unauthorized("User not found");
           
        }
    

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        //[HttpPost("refresh-token"), Authorize]
        //public async Task<ActionResult<string>> RefreshToken([FromHeader] string Authorization)
        //{
        //    string jwt = Authorization.Substring("Bearer ".Length).Trim();
        //    string? email = getEmailFromJWT(jwt);
        //    Console.WriteLine(email);


        //    AppUser? user = await _userManager.FindByEmailAsync(email);
        //    if (user == null)
        //    {
        //        return Unauthorized("User not authenticated.");
        //    }
        //    var refreshToken = Request.Cookies["refreshToken"];

        //    if (!user.RefreshToken.Equals(refreshToken))
        //    {
        //        return Unauthorized("Invalid Refresh Token.");
        //    }
        //    //else if (user.TokenExpires < DateTime.Now)
        //    //{
        //    //    return Unauthorized("Token expired.");
        //    //}

        //    string token = CreateToken(user);
        //    var newRefreshToken = GenerateRefreshToken(user);
        //    SetRefreshToken(newRefreshToken, user);

        //    return Ok(token);
        //}

        //private RefreshToken GenerateRefreshToken(User user)
        //{
        //    var refreshToken = new RefreshToken
        //    {
        //        Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
        //        Expires = DateTime.Now.AddDays(7),
        //        Created = DateTime.Now
        //    };

        //    return refreshToken;
        //}

        //private void SetRefreshToken(RefreshToken newRefreshToken, User user)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Expires = newRefreshToken.Expires
        //    };
        //    Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);

        //    user.RefreshToken = newRefreshToken.Token;
        //    user.TokenCreated = newRefreshToken.Created;
        //    user.TokenExpires = newRefreshToken.Expires;

        //    dbReservation.Users.Update(user);
        //    dbReservation.SaveChanges();
        //}

        private string? getEmailFromJWT(string jwt)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(jwt);

            var claims = token.Claims;
            foreach (var claim in claims)
            {
                if (claim.Type.Contains("email"))
                {
                    return claim.Value;
                }
            }
            return null;
        }

    }
    public class UserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
    public class ResponseDto
    {
        public string Message { get; set; } = string.Empty;

    }
}

