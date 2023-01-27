using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultipleAuthIdentity.Areas.Identity.Data;
using MultipleAuthIdentity.Data;
using MultipleAuthIdentity.Models;

namespace MultipleAuthIdentity.Controllers
{
    
    public class AdminController : Controller
    {
        private readonly AuthDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        
        public AdminController(AuthDbContext context,UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager= userManager;
        }

        // GET: Admin
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Index()
        {
            List<Users> users = new List<Users>();
            List < AppUser > appUsers= _userManager.Users.ToList();
            foreach(var u in appUsers)
            {
                var roles=_userManager.GetRolesAsync(u).Result.FirstOrDefault();
                users.Add(new Users(u.Id, u.Email, roles, u.PhoneNumber));
            }
              return View(users);
        }

        // GET: Admin/Details/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var users = await _userManager.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (users == null)
            {
                return NotFound();
            }

            return View(users);
        }

        // GET: Admin/Create
        [Authorize(Roles = "ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Create
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Users users)
        {
            if (ModelState.IsValid)
            {
               // await _userManager.Users.Append(users);
                //await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // GET: Admin/Edit/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null )
            {
                return NotFound();
            }

            var users = await _userManager.FindByIdAsync(id);
            if (users == null)
            {
                return NotFound();
            }
            return View();
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Users users)
        {
        //    if (id != users.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(users);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!UsersExists(users.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
            return View();
        }

        // GET: Admin/Delete/5
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var users = await _userManager.FindByIdAsync(id);
            if (users == null)
            {
                return NotFound();
            }

            return View();
        }

        // POST: Admin/Delete/5
        [Authorize(Roles = "ADMIN")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            //if (_context.Users == null)
            //{
            //    return Problem("Entity set 'AuthDbContext.Users'  is null.");
            //}
            //var users = await _context.Users.FindAsync(id);
            //if (users != null)
            //{
            //    _context.Users.Remove(users);
            //}
            
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsersExists(string id)
        {
            
            return true;
        }
    }
}
