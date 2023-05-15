using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;
using MultipleAuthIdentity.Services;

namespace MultipleAuthIdentity.Controllers
{
  
    public class AdminController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private static int onlineUsers = 0;
        private readonly IAdminService _adminService;


        public AdminController(AuthDbContext context,UserManager<AppUser> userManager, IAdminService adminService)
        {
            _context = context;
            _userManager= userManager;
            _adminService= adminService;
        }

        [Authorize(Roles ="ADMIN")]
        public  IActionResult Index()
        {
            var users =  _userManager.Users.ToList();

            return View(users);
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role =   _userManager.GetRolesAsync(user).Result.FirstOrDefault();
            UserEdit model= new UserEdit(user, role);
         
            return View(model);
        }
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
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

        [Authorize(Roles = "ADMIN")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
            return View();
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
            foreach(var p in prov)
            {
                if (p.ProviderDisplayName == "Google")
                    google++;
                else if(p.ProviderDisplayName=="Facebook")
                        facebook++;
            }
            cookie = totalUsers.Count() - google - facebook;

            providers.Add(google);
            providers.Add(facebook);
            providers.Add(cookie);

            if (DateTime.Now.Day == 1)
            {
                _adminService.changeUsersPanel(DateTime.Now.Month-1);
            }

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
