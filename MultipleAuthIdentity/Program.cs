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

    //.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme,options =>
    //{

    //    options.TokenValidationParameters = new TokenValidationParameters
    //    {

    //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretKeyTopRosiaBarcelona"))
    //    };
    //    options.Events = new JwtBearerEvents
    //    {
    //        OnTokenValidated = context =>
    //        {
    //            var identity = context.Principal.Identity as ClaimsIdentity;
    //            if (identity != null)
    //            {
    //                var userName = identity.FindFirst(ClaimTypes.Name)?.Value;
    //                if (!string.IsNullOrEmpty(userName))
    //                {
    //                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userName));
    //                }
    //            }
    //            return Task.CompletedTask;
    //        }
    //    };
    //})
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
