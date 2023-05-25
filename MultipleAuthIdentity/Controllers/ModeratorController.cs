using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;

namespace MultipleAuthIdentity.Controllers
{
    public class ModeratorController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ModeratorController(AuthDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BusRoutesAsync()
        {
            AppUser? user =await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            if(user == null)
            {
                return View();
            }
            List<Routes> routes = _context.Routes.Where(r => r.UserId == user.Id).ToList();
            List<Bus> buses=_context.Bus.Where(r => r.UserId == user.Id).ToList();

            BusRoutes model = new BusRoutes();
            model.routes=routes;
            model.buses=buses;
            return View(model);
        }

        [HttpGet]
        public IActionResult DeleteRoute(string id)
        {

            return Ok("salut");
        }
        [HttpPost]
        public async Task<IActionResult> AddRoute(Routes route)
        {

            AppUser? user = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            route.UserId = user.Id;
            _context.Routes.Add(route);
            _context.SaveChanges();
            return Ok("Success");
        }


    }
}
