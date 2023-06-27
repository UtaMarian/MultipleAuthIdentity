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
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Util.Store;
using System;
using System.Threading;
using Duende.IdentityServer.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Microsoft.AspNetCore.SignalR;

namespace MultipleAuthIdentity.Controllers
{

    public class JwtAuthController : Controller
    {
        private readonly AuthDbContext _dbcontext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;

        private const string GoogleTokenEndpoint = "https://oauth2.googleapis.com/token";
        private readonly string clientId= "";
        private readonly string clientSecret= "";
        private readonly string redirectUri= "";

        public JwtAuthController(AuthDbContext context, UserManager<AppUser> userManager, IConfiguration configuration, IJwtService jwtService, SignInManager<AppUser> signInManager)
        {
            _dbcontext = context;
            _userManager = userManager;
            _configuration = configuration;
            _jwtService = jwtService;
            clientId = _configuration.GetSection("GoogleMobile:ClientId").Value;
            clientSecret = _configuration.GetSection("GoogleMobile:ClientSecret").Value;
            redirectUri = _configuration.GetSection("GoogleMobile:RedirectUri").Value;
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

            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            user.IpAddress = ip;
            user.LastSignIn = DateTime.Now;
            _dbcontext.Users.Update(user);
            _dbcontext.SaveChanges();

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

        [HttpPost("HandleCode")]
        public async Task<ActionResult<LoginJwtResponse>> ExchangeCodeForTokens([FromHeader] string Authorization)
        {
            string googleCode = Authorization.Replace("Bearer ", string.Empty);
             
            using (HttpClient client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", googleCode },
                { "client_id",  clientId},
                { "client_secret",  clientSecret},
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            });

                var response = await client.PostAsync(GoogleTokenEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to exchange code for tokens: {responseContent}");
                }


                var tokenResponse = JsonConvert.DeserializeObject<GoogleTokenResponse>(responseContent);

                var handler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidAudience = clientId,
                    ValidIssuer = "https://accounts.google.com",
                    IssuerSigningKeys = await GetSigningKeysAsync()
                };

                try
                {
                    ClaimsPrincipal claimsPrincipal = handler.ValidateToken(tokenResponse.IdToken, validationParameters, out var validatedToken);

                    string Email = claimsPrincipal.FindFirstValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");

                    AppUser? user = await _userManager.FindByEmailAsync(Email);
                    if (user == null)
                    {
                        //utilizatorul nu are cont
                        user = new AppUser();
                        user.UserName =Email;
                        user.Email = Email;
                        user.NormalizedEmail =Email.ToUpper();
                        user.NormalizedUserName = Email.ToUpper();
                        user.TwoFactorEnabled = false;
                        user.PhoneNumberConfirmed = false;
                        user.EmailConfirmed = false;
                        user.Id = claimsPrincipal.FindFirstValue("sub");
                        var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                        user.IpAddress = ip;
                        user.LastSignIn = DateTime.Now;
                        await _userManager.CreateAsync(user);
                        user = await _userManager.FindByEmailAsync(Email);
                        await _userManager.AddToRoleAsync(user, "USER");
                        LoginJwtResponse responseRegiser = _jwtService.CreateToken(user);

                        return responseRegiser;

                    }
                    else
                    {
                        var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                        user.IpAddress = ip;
                        user.LastSignIn = DateTime.Now;
                        _dbcontext.Users.Update(user);
                        _dbcontext.SaveChanges();
                    }
                    LoginJwtResponse responseJwt = _jwtService.CreateToken(user);

                    return responseJwt;
                }
                catch (Exception ex)
                {
                    return Unauthorized("Token is invalid. " + ex.Message);
                }
            }

        
        }
        
      
        private async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync()
        {
            string GoogleSigningKeysEndpoint = "https://www.googleapis.com/oauth2/v3/certs";

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(GoogleSigningKeysEndpoint);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();

                var signingKeys = new List<SecurityKey>();

                var jwksDocument = new JsonWebKeySet(responseContent);

                foreach (var key in jwksDocument.Keys)
                {
                    signingKeys.Add(key);
                }

                return signingKeys;
            }
        }

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
    public class GoogleTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }

}

