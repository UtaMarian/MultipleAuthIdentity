using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Google.Apis.Auth.AspNetCore3;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Authentication;
using Rsk.AspNetCore.Authentication.Saml2p;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");
//Add Email Configs
//var emailConfig = builder.Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>();
//builder.Services.AddSingleton(emailConfig);

//builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAdminService, AdminService>();


builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.Configure<AuthMessageSenderOptions>(builder.Configuration);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>();


builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication()
    .AddCookie(options =>
    {

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;  // to prevent cookies from being accessed by client-side scripts
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //to prevent cookies from being sent over unsecured connections
        options.Cookie.SameSite = SameSiteMode.Strict; // flag to prevent cross-site request forgery (CSRF) attacks
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
        options.CallbackPath = builder.Configuration["Google:CallbackPath"];
        options.SaveTokens = true;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.AuthorizationEndpoint += "?prompt=consent";
        options.SaveTokens = true;

    })

.AddFacebook("Facebook", options =>
 {
     options.AppId = builder.Configuration["Facebook:AppId"];
     options.AppSecret = builder.Configuration["Facebook:AppSecret"];
 })
.AddSaml2p("saml2", options =>
{
    options.Licensee = "DEMO";
    options.LicenseKey = "eyJTb2xkRm9yIjowLjAsIktleVByZXNldCI6NiwiU2F2ZUtleSI6ZmFsc2UsIkxlZ2FjeUtleSI6ZmFsc2UsIlJlbmV3YWxTZW50VGltZSI6IjAwMDEtMDEtMDFUMDA6MDA6MDAiLCJhdXRoIjoiREVNTyIsImV4cCI6IjIwMjMtMDYtMTFUMDM6MDQ6NTUuMjIzNTc2MSswMDowMCIsImlhdCI6IjIwMjMtMDUtMTJUMDM6MDQ6NTUiLCJvcmciOiJERU1PIiwiYXVkIjoyfQ==.AgY3CBqvUdb+zchvkQxegnskktd+3f6T7BiFwlzaN7s6kRSinj++wAqyKswsjWv2HeeEGW7au2yCU1Ug/MnsCfcfHvoVT16oRVdihs6x35pno5r0lQfj8+vNy0RxlasPJYQjmRQ1sbCMr8cX7oyk+m/Qw/kKy1aTf8Skonrwm0qAwnH6QicAdxpwvesMutQi2aN4b16j4kJzBsclW8Jv6F/Z4A60zWHH0zvf1iR5jRJKecCPK/nWcaFF8yPsc2AoxiBkpG+EVUvG/WgmbBaMJVzdA7/LAhh6kZOPLyigUWZALNORRLDAkJ5JhLxgCb3GBi3iFTvCcbRD6Ca8RLRgFMDqBSKdprNbaATmYjr3+52CnFt3NhxOlm9ECG01iXWZ4Nl1gnx/nMAuJjhPG0T54kdyY+iO6Gr/00lA/omi/EVPuWxVKrtQw3veH/vbgNHmary6HtB56KJGRqWfQLxnnSHQcO9nW+8vrf2zQR3BtKdvQYWuoQjoGi7PiEdnhAN5BN1qAqtxZwTAt869x/0pptxaYeB5WZVJhbiyh4DwG0QtTl53K4b267xyYWm1Ow2fanE5ScU8EoJ5WQ5/an8WOgMO4p3DJIcSHpxdgOhpXOIF9BD2hh2LZDvFUSK9gMSRdI9XdstEmnX0LcXsZG85OHRBQxRQ7ZTYgFKEdS94Q+0=";
    options.NameIdClaimType = "sub";
    options.CallbackPath = "/saml/SSO";
    //options.SignInScheme = "Cookie";
    options.IdentityProviderMetadataAddress = "https://dev-67434086.okta.com/app/exk9i2uwnzf79cS6s5d7/sso/saml/metadata";
    options.RequireValidMetadataSignature = false;
    options.TimeComparisonTolerance = 120;
    options.ServiceProviderOptions = new SpOptions
    {
        EntityId = "https://multipleauth.azurewebsites.net/saml/metadata",
        MetadataPath = "/saml/metadata"
    };

});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin",policy =>
    {
        policy.RequireUserName("marianuta112@gmail.com");
    });
    options.AddPolicy("Authenticated", policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
    //options.AddPolicy("JwtRequired", policy =>
    //{
       
    //});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "admin",
    pattern: "{controller=Admin}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
