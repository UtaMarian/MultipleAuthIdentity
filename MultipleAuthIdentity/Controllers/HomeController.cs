using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Owin.Security.Cookies;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Text;


namespace MultipleAuthIdentity.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AuthDbContext dbContext;

        public HomeController(ILogger<HomeController> logger, AuthDbContext dbContext)
        {
            _logger = logger;
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {

            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ContactPost( ContactDto dto)
        {
            Console.WriteLine(dto.Name);
            return View();
        }

        public IActionResult Privacy()
        {
            
            var data = dbContext.Review.ToList();
            Reviews reviews= new(data);

            return View(reviews);
            
        }
   
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Privacy(string Subject,string Content)
        {
            Review review= new();
            review.Subject = Subject;
            review.Content = Content;
            review.Email = HttpContext.User.Identity.Name;
            review.Date= DateTime.Now;
            dbContext.Review.Add(review);
            dbContext.SaveChanges();



            var data = dbContext.Review.ToList();
            Reviews reviews = new(data);

            return View(reviews);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Admin()
        {

            return View();
        }

        [Authorize]
        public IActionResult Google()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult ErrorPage(MyError error)
        {
            return View(error);
        }

        public class ContactDto
        {
            public string Name { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;

        }
    }
}