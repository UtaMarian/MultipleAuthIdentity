using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;
using System.Data;

namespace MultipleAuthIdentity.Controllers
{
    [Authorize(Roles = "MODERATOR")]
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoute(Routes route)
        {
            AppUser? user = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            route.UserId = user.Id;
            _context.Routes.Add(route);
            _context.SaveChanges();
            TempData["msg"] = "Ruta a fost adaugata";
            return View("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBus(Bus bus)
        {
            AppUser? user = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            bus.UserId = user.Id;
            _context.Bus.Add(bus);
            _context.SaveChanges();
            TempData["msg"] = "Autobuzul a fost adaugat";
            return View("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditRoute(int id)
        {
            Routes? route = await _context.Routes.FindAsync(id);
            if (route == null)
            {
                TempData["error"] = "Routa respectiva nu mai exista";
                return View();
            }
            RouteModel model = new RouteModel(route);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoute(RouteModel model)
        {
           Routes? route = await _context.Routes.FindAsync(model.Id);
            if(route != null)
            {
                route.ArrivalDate = model.ArrivalDate;
                route.Arrival = model.Arrival;
                route.DepartureDate = model.DepartureDate;
                route.Departure = model.Departure;
                route.BusId= model.BusId;
                route.Price = model.Price;
                _context.Routes.Update(route);
                _context.SaveChanges();
                TempData["msg"] = "Modificarile au fost salvate";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRoute(int id)
        {
            Routes? route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                _context.Routes.Remove(route);
                _context.SaveChanges();
                TempData["del"] = "Ruta a fost stearsa";
            }

            AppUser? user = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            if (user == null)
            {
                return View();
            }
            List<Routes> routes = _context.Routes.Where(r => r.UserId == user.Id).ToList();
            List<Bus> buses = _context.Bus.Where(r => r.UserId == user.Id).ToList();

            BusRoutes model = new BusRoutes();
            model.routes = routes;
            model.buses = buses;
            return View("BusRoutes",model);

        }

        [HttpGet]
        public async Task<IActionResult> EditBus(int id)
        {
            Bus? bus = await _context.Bus.FindAsync(id);
            if (bus == null)
            {
                TempData["error"] = "Autobuzul respectiv nu mai exista";
                return View();
            }
            BusModel model = new BusModel(bus);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBus(BusModel model)
        {
            Bus? bus = await _context.Bus.FindAsync(model.Id);
            if (bus != null)
            {
                bus.Bus_number = model.Bus_number;
                bus.Bus_Plate_number = model.Bus_Plate_number;
                bus.Bus_Type = model.Bus_Type;
                bus.Capacity = model.Capacity;
                _context.Bus.Update(bus);
                _context.SaveChanges();
                TempData["msg"] = "Modificarile au fost salvate";
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBus(int id)
        {
            Bus? bus = await _context.Bus.FindAsync(id);
            if (bus != null)
            {
                
                
                List<Routes> RoutesList = _context.Routes.ToList();
                foreach(Routes r in RoutesList)
                {
                    if (r.BusId == id)
                    {
                        _context.Routes.Remove(r);
                    }
                }
                _context.Bus.Remove(bus);
                _context.SaveChanges();
                TempData["del"] = "Autobuzul si rutele respective au fost sterse ";
            }
            AppUser? user = await _userManager.FindByEmailAsync(HttpContext.User.Identity.Name);
            if (user == null)
            {
                return View();
            }
            List<Routes> routes = _context.Routes.Where(r => r.UserId == user.Id).ToList();
            List<Bus> buses = _context.Bus.Where(r => r.UserId == user.Id).ToList();

            BusRoutes model = new BusRoutes();
            model.routes = routes;
            model.buses = buses;
            return View("BusRoutes", model);

        }
    }
}
