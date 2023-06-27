// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using MultipleAuthIdentity.Areas.Identity.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MultipleAuthIdentity.Controllers;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using Serilog;
using Google.Apis.Drive.v3.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MultipleAuthIdentity.Areas.Identity.Pages.Account
{
  
    public class LoginModel : PageModel
    {

        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly AuthDbContext _authDbContext;
        
        public LoginModel(SignInManager<AppUser> signInManager, ILogger<LoginModel> logger, AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _logger = logger;
            _authDbContext = authDbContext;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            ReturnUrl = returnUrl;

        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            //Logguri autentificare
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Debug()
                        .WriteTo.Console()
                        .WriteTo.File("log.txt")
                        .CreateLogger();
            if (!Url.IsLocalUrl(returnUrl)&& !returnUrl.IsNullOrEmpty())
            {
                Log.Error("Redirect URL invalid. User Email=" + Input.Email);
                MyError error=new MyError();
                error.Message = "Eroare de redirectare";
                error.Code = 400;
                error.Description = "Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului";

                Log.Warning("Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului - User: " + Input.Email);
                Log.CloseAndFlush();
                return RedirectToAction("ErrorPage", "Home", error);
            }
            

            returnUrl ??= Url.Content("~/");
 
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
          
            if (ModelState.IsValid)
            {
                
                var user = await _signInManager.UserManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    Log.Error("Utilizatorul nu exista : " + Input.Email);
                    return Page();
                }
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);
                
                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    
                    AdminController.growupOnlineUsers();
                    user.LastSignIn=DateTime.Now;
                    var ip=HttpContext.Connection.RemoteIpAddress.ToString();
                    user.IpAddress = ip;
                    _authDbContext.Update(user);
                    _authDbContext.SaveChanges();

                    Log.Information("Autentificare reușită pentru utilizatorul "+ user.Email);
                    var redirect = LocalRedirect(returnUrl);
                    Log.CloseAndFlush();
                    return redirect;
                   
                }
                if (result.RequiresTwoFactor)
                {
                    Log.Information("Autentificare parțial reușită. Utilizatorul este redirectat catre autentificarea cu doi pasi. " + user.Email);
                    Log.CloseAndFlush();
                    return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                }
                if (result.IsLockedOut)
                {
                    Log.Warning("Autentificare nereușită pentru utilizatorul "+ user.Email+" . Acesta a atins numarul maxim de incercari." );
                    _logger.LogWarning("User account locked out.");
                    Log.CloseAndFlush();
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    Log.Warning("Încercare de conectare nevalidă pentru utilizatorul: " + Input.Email+ " . Contul este blocat temporar pentru depasirea numarului de incercari.");
                    Log.CloseAndFlush();
                    ModelState.AddModelError(string.Empty, "Invalid login.");
                    return Page();
                }
            }
            Log.CloseAndFlush();
            return Page();
        }

    }
}
