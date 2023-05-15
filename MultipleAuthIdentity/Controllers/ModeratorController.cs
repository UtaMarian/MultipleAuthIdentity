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

        public ModeratorController(AuthDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult BusRoutes()
        {
            List<Routes> routes=_context.Routes.ToList();
            List<Bus> buses=_context.Bus.ToList();

            BusRoutes model = new BusRoutes();
            model.routes=routes;
            model.buses=buses;
            return View(model);
        }
    }
}
