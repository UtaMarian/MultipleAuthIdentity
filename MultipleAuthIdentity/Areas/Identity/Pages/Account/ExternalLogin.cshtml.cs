// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using MultipleAuthIdentity.Areas.Identity.Data;
using Google.Apis.Drive.v3.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Controllers;
using MultipleAuthIdentity.Models;
using System.Linq;
using Serilog;
using static Duende.IdentityServer.Models.IdentityResources;
using static Rsk.Saml.SamlConstants;

namespace MultipleAuthIdentity.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;
        private readonly AuthDbContext _authDbContext;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender,
            AuthDbContext authDbContext)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _logger = logger;
            _emailSender = emailSender;
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
        public string ProviderDisplayName { get; set; }

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
        }
        
        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            try
            {
                // Request a redirect to the external login provider.
                var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback", values: new { returnUrl });
                var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
                return new ChallengeResult(provider, properties);
            }
            catch (Exception ex)
            {
                MyError error = new MyError();
                error.Message = "Internal server error";
                error.Code = 500;
                error.Description = ex.Message;

                return RedirectToAction("ErrorPage", "Home", error);
            }
        }

        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            MyError error = new MyError();
            try
            {
                Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.Console()
                            .WriteTo.File("log.txt")
                            .CreateLogger();

               
                returnUrl = returnUrl ?? Url.Content("~/");
                if (remoteError != null)
                {
                    Log.Error("Eroare de la provideul de identitate " + remoteError);
                    Log.CloseAndFlush();
                    ErrorMessage = $"Error from external provider: {remoteError}";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }
                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    Log.Error("Eroare incarcare informatii de login");
                    Log.CloseAndFlush();
                    ErrorMessage = "Error loading external login information.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
               
                if (result.Succeeded)
                {
                    var email = "";
                    if (info.LoginProvider.Equals("saml2"))
                    {
                        email = info.Principal.Claims.FirstOrDefault().Value;
                    }
                    else
                    {
                        email = info.Principal.Claims.Where(c => c.Type == ClaimTypes.Email).FirstOrDefault().Value;
                    }

                   
                    Log.Information(email + " s-a autentificat cu contul de " + info.LoginProvider);
                    _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);

                    AppUser user = await _userManager.FindByEmailAsync(email);
                    if (user != null)
                    {
                        if (!HasAccess(email))
                        {
                            Log.Error("Utilizatorul nu are acces in aplicatie pe baza ACL-ului=" + Input.Email);

                            error.Message = "Neautorizat";
                            error.Code = 403;
                            error.Description = "Se pare ca nu aveti acces la aplicatie sau un admin v-a banat";
                            return RedirectToAction("ErrorPage", "Home", error);
                        }
                        var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                        user.IpAddress = ip;
                        user.LastSignIn = DateTime.Now;
                        _authDbContext.Users.Update(user);
                        _authDbContext.SaveChanges();
                    }

                    AdminController.growupOnlineUsers();
                    Log.CloseAndFlush();
                    if (!Url.IsLocalUrl(returnUrl) && (returnUrl != null))
                    {
                        Log.Error("Redirect URL invalid. User Email=" + email);
                       
                        error.Message = "Eroare de redirectare";
                        error.Code = 400;
                        error.Description = "Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului";

                        Log.Warning("Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului - User: " + email);
                        Log.CloseAndFlush();
                        return RedirectToAction("ErrorPage", "Home", error);
                    }
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    Log.CloseAndFlush();
                    return RedirectToPage("./Lockout");
                }
                else
                {
                  
                    ReturnUrl = returnUrl;
                    ProviderDisplayName = info.ProviderDisplayName;
                    if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                    {
                        Input = new InputModel
                        {
                            Email = info.Principal.FindFirstValue(ClaimTypes.Email)
                        };
                    }
                    Log.CloseAndFlush();
                   
                    return await OnPostConfirmationAsync(returnUrl);
                }
            }
            catch (Exception ex)
            {
      
                error.Message = "Internal server error";
                error.Code = 500;
                error.Description = ex.Message;

                return RedirectToAction("ErrorPage", "Home", error);
            }
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
        {
            try
            {
                Log.Logger = new LoggerConfiguration()
                            .MinimumLevel.Debug()
                            .WriteTo.Console()
                            .WriteTo.File("log.txt")
                            .CreateLogger();

                returnUrl = returnUrl ?? Url.Content("~/");

                var info = await _signInManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    Log.Error("Eroare incarcare informatii de la providerul: " + info.LoginProvider);
                    Log.CloseAndFlush();
                    ErrorMessage = "Error loading external login information during confirmation.";
                    return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
                }

                if (ModelState.IsValid)
                {
                    var user = CreateUser();
                    var email = "";
                    if (info.LoginProvider.Equals("saml2"))
                    {
                        email = info.Principal.Claims.FirstOrDefault().Value;
                    }
                    else
                    {
                        email = Input.Email;
                    }
                    await _userStore.SetUserNameAsync(user, email, CancellationToken.None);
                    await _emailStore.SetEmailAsync(user, email, CancellationToken.None);
                    user.EmailConfirmed = true;
                    user.LastSignIn = DateTime.Now;
                    var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                    user.IpAddress = ip;

                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "USER");
                        result = await _userManager.AddLoginAsync(user, info);
                        if (result.Succeeded)
                        {
                            _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                            Log.Information("Utilizatorul " + info.Principal.Identity.Name + " a creat un cont cu contul de " + info.LoginProvider);

                            var userId = await _userManager.GetUserIdAsync(user);
                            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                            Console.WriteLine(code);
                            Console.WriteLine(userId);
                            Console.WriteLine(Request.Scheme);

                            var callbackUrl = Url.Page(
                                "/Account/ConfirmEmail",
                                pageHandler: null,
                                values: new { area = "Identity", userId = userId, code = code },
                                protocol: Request.Scheme);

                            await _emailSender.SendEmailAsync(email, "Confirmati emailul",
                                $"Confirmati contul aici: {HtmlEncoder.Default.Encode(callbackUrl)}");

                            user.EmailConfirmed = true;


                            await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                            Log.CloseAndFlush();
                            if (!Url.IsLocalUrl(returnUrl) && (returnUrl!=null))
                            {
                                Log.Error("Redirect URL invalid. User Email=" + Input.Email);
                                MyError error = new MyError();
                                error.Message = "Eroare de redirectare";
                                error.Code = 400;
                                error.Description = "Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului";

                                Log.Warning("Se pare ca url-ul este unul malițios deoarece încearcă sa vă redirecteze in afara domeniului - User: " + Input.Email);
                                Log.CloseAndFlush();
                                return RedirectToAction("ErrorPage", "Home", error);
                            }
                            return LocalRedirect(returnUrl);

                        }
                    }
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                ProviderDisplayName = info.ProviderDisplayName;
                ReturnUrl = returnUrl;
                Log.CloseAndFlush();
                return Page();
            }
            catch (Exception ex)
            {
                MyError error = new MyError();
                error.Message = "Internal server error";
                error.Code = 500;
                error.Description = ex.Message;

                return RedirectToAction("ErrorPage", "Home", error);
            }
        }

        private AppUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<AppUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(AppUser)}'. " +
                    $"Ensure that '{nameof(AppUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the external login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
            }
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<AppUser>)_userStore;
        }

        public bool HasAccess(string email)
        {
            List<AccessListItem> acl = _authDbContext.AccessListItem.ToList();
            if (acl.Count > 0)
            {
                foreach (var a in acl)
                {
                    if (a.Email.Contains(email))
                        return false;
                }
            }
            return true;
        }
    }
}
