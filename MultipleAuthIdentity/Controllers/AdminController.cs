using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;
using Newtonsoft.Json.Linq;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MultipleAuthIdentity.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class AdminController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private static int onlineUsers;
        private readonly IAdminService _adminService;

        public AdminController(AuthDbContext context,UserManager<AppUser> userManager, IAdminService adminService)
        {
            _context = context;
            _userManager= userManager;
            _adminService= adminService;
           
        }

        public  IActionResult Index()
        {
            var users =  _userManager.Users.ToList();

            return View(users);
        }

        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role =   _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            UserEdit model= new UserEdit(user, role);
         
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserEdit user)
        {
            var appuser = await _userManager.FindByIdAsync(user.Id);
            if(!string.IsNullOrEmpty(user.Password))
            {
                await _userManager.AddPasswordAsync(appuser,user.Password);
            }
            if (!string.IsNullOrEmpty(user.Email))
            {
                appuser.Email = user.Email;
            }
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                appuser.PhoneNumber = user.PhoneNumber;
            }
            if (!string.IsNullOrEmpty(user.Role))
            {
                var role = _userManager.GetRolesAsync(appuser).Result.FirstOrDefault();
                await _userManager.RemoveFromRoleAsync(appuser, role);
                await _userManager.AddToRoleAsync(appuser, user.Role);
            }
           
            
            _userManager.UpdateAsync(appuser);
            UserEdit model = new UserEdit(appuser, user.Role);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var result=await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["del"] = "Utiliatorul a fost sters cu succes!";
            }
            var users = _userManager.Users.ToList();

            return View("Index", users);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> SuperAdmin()
        {
            List<string> cities = new List<string>();
            List<AppUser> users=_context.Users.Where(o=>o.IpAddress!=null).ToList();
            
            foreach (var u in users)
            {
                if (u.IpAddress != null)
                {
                    string url = "http://ip-api.com/json/";
                    url += u.IpAddress;
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);

                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();
                            JObject json = JObject.Parse(jsonResponse);
                            string regionName = (string)json["regionName"];
                            if(regionName!= null)
                                cities.Add(regionName);
                        }
                        else
                        {
                            Console.WriteLine("Request failed with status code: " + response.StatusCode);
                        }
                    }
                }
               
            }
            ViewData["Cities"] = cities;
            return View(cities);
        }

        [HttpGet]
        public IActionResult GetLogs()
        {
            string logContent = "";
            string filePath = "log.txt";

            Log.CloseAndFlushAsync();
            logContent = System.IO.File.ReadAllText(filePath);
            var logs = logContent.Split('\n');

            return Json(logs);
        }

        public IActionResult AdminPanel()
        {
            AdminPanelModel model = new AdminPanelModel();
            
            List<float> prices=_context.Reservations.Select(r => r.Price).ToList();
            List<AppUser> totalUsers = _context.Users.ToList();
            List<int> providers = new List<int>();
            var prov=_context.UserLogins.ToList();
            int google = 0;
            int facebook = 0;
            int cookie = 0;
            int okta = 0;
            foreach(var p in prov)
            {
                if (p.ProviderDisplayName == "Google")
                    google++;
                else if (p.ProviderDisplayName == "Facebook")
                    facebook++;
                else 
                    okta++;
            }
            cookie = totalUsers.Count() - google - facebook-okta;

            providers.Add(google);
            providers.Add(facebook);
            providers.Add(cookie);
            providers.Add(okta);

            _adminService.changeUsersPanel(DateTime.Now.Month);

            //utilizatorii zilnici in luna curenta
            var now = DateTime.Now;
            var numberOfDays = DateTime.DaysInMonth(now.Year, now.Month);
            List<int> days = new List<int>();

            List<string> list = _adminService.GetDailyUserCount();
            for (int i = 0; i < numberOfDays; i++)
            {
                days.Add(int.Parse(list[i]));
            }

            model.dailyUsers = days;
            model.montlyOnlineUsers = _adminService.getMonthlyUsers();
            model.soldedTickets = prices.Count();
            model.totalUsers = totalUsers.Count();
            model.onlineUsers = onlineUsers;
            model.totalMoney = prices.Sum();
            model.providers = providers;


            
            

            return View(model);
        }

        public int getOnlineUsers()
        {
            return onlineUsers;
        }
        public static void growupOnlineUsers()
        {
            onlineUsers++;
        }
        public static void growDownOnlineUsers()
        {
            onlineUsers--;
        }

        


        
    }
   
}
