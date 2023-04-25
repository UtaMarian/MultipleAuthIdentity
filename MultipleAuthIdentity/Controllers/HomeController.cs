using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Owin.Security.Cookies;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
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

        [HttpGet("getinfo")]
        public string getInfo()
        {
            return "Acesta este un test";
        }

        public IActionResult Index()
        {
            return View();
        }

       

        public IActionResult Privacy()
        {
            
            var data = dbContext.Review.ToList();
            Reviews reviews= new(data);

            return View(reviews);
            
        }
   
        [HttpPost]
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


        public IActionResult Google()
        {

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

      
    }
}