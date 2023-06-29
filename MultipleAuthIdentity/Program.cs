using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using Rsk.AspNetCore.Authentication.Saml2p;
using MultipleAuthIdentity.Controllers;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AuthDbContextConnection") ?? throw new InvalidOperationException("Connection string 'AuthDbContextConnection' not found.");

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
        options.ExpireTimeSpan = TimeSpan.FromHours(1); //timpul de viata al cookie-ului
        options.Cookie.HttpOnly = true;  // pentru a preveni accesarea cookie-urilor din scripturi
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; //pentru a preveni trimiterea cookie-urilor pe o retea nesecurizata
        options.Cookie.SameSite = SameSiteMode.Strict; // previne atacurile de tip CSRF
    })
    .AddGoogle("Google", options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
        options.CallbackPath = builder.Configuration["Google:CallbackPath"];
        options.SaveTokens = false;
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");
        options.AuthorizationEndpoint += "?prompt=consent";

        options.Events.OnCreatingTicket = context =>
        {
            Console.WriteLine(context.AccessToken);
            Console.WriteLine(context.User);
            return Task.CompletedTask;
        };

    })
.AddFacebook("Facebook", options =>
 {
     options.AppId = builder.Configuration["Facebook:AppId"];
     options.AppSecret = builder.Configuration["Facebook:AppSecret"];
 })
.AddSaml2p("saml2", options =>
{
    options.Licensee = "DEMO";
    options.LicenseKey = builder.Configuration["Okta:LicenseKey"];
    options.ForceAuthentication = true;
    options.NameIdClaimType = "sub";
    options.CallbackPath = "/saml/SSO";
    options.IdentityProviderMetadataAddress = builder.Configuration["Okta:MetadataAdress"];
    options.RequireValidMetadataSignature = false;
    options.TimeComparisonTolerance = 320;
    options.ServiceProviderOptions = new SpOptions
    {
        EntityId = builder.Configuration["Okta:EntityId"],
        MetadataPath = "/saml/metadata"
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MainAdmin", policy =>
    {
        policy.RequireUserName("marianuta11@yahoo.com") ;
    });
    options.AddPolicy("SecondAdmin", policy =>
    {
        policy.RequireUserName("danstefan1918@gmail.com");
    });
    options.AddPolicy("Authenticated", policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });
   
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
