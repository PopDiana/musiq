using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using musiq.Data;
using musiq.Models;

namespace musiq.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _appEnvironment;
        public UserController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _appEnvironment = hostingEnvironment;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {

            var dbUser = _context.User.FirstOrDefault(u => u.Email == user.Email);

            // search database for username and password
            if (dbUser == null)
            {
                ModelState.AddModelError("", "The username does not exist.");
                return View();
            }

            if (!Authentication.Instance.ValidatePassword(user.Password, dbUser.Password, dbUser.PassSalt))
            {
                ModelState.AddModelError("", "You introduced a wrong password.");
                return View();
            }

            // login as typical user or admin
            if (dbUser.IsAdmin == true)
            {
                Authentication.Instance.AdminLogin(dbUser);
            }
            else
            {
                Authentication.Instance.UserLogin(dbUser);
            }

            await _context.SaveChangesAsync();

            // redirect to main page after successful login
            return Redirect("~/Home/Index");

        }
        public IActionResult Register()
        {          
           return View();              
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("UserId,Email,Password,ConfirmPassword,PassSalt,IsAdmin,Nickname,Description,LikedGenres,ProfilePicture")] User user)
        {

            var existingUser = _context.User.FirstOrDefault(u => u.Email == user.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "The username already exists.");
            }

            if(user.Password != user.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
            }
            if(user.ProfilePicture == null || user.ProfilePicture == "")
            {
                user.ProfilePicture = "avatar.png";
            }
            if (ModelState.IsValid && existingUser == null)
            {
                // hash password

                String salt = Authentication.Instance.GetRandomSalt();
                user.PassSalt = salt;
                user.Password = Authentication.Instance.HashPassword(user.Password, salt);
                user.Nickname = "newuser";
                _context.Add(user);
                await _context.SaveChangesAsync();
                Authentication.Instance.UserLogin(user);
                return Redirect("~/Home/Index");
            }
            return View(user);

        }

        public IActionResult Logout()
        {
            Authentication.Instance.Logout();
            return Redirect("~/User/Login");
        }

        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }
            user.Posts = _context.Post.Where(p => p.UserId == id).ToList();

            return View(user);
        }

        // GET: User/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("UserId,Email,Password,PassSalt,IsAdmin,Nickname,Description,LikedGenres,ProfilePicture,File")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }
            var file = user.File;
            if (file != null && file.Length > 0 && user.ProfilePicture == null)
            {
                var uploads = Path.Combine(_appEnvironment.WebRootPath, "uploads");
                var extension = Path.GetExtension(file.FileName);
               
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + extension;
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        user.ProfilePicture = fileName;
                    }
                }
            } else
            {
                user.ProfilePicture = "avatar.png";
            }
            user.Password = Authentication.Instance.getCurrentUser().Password;
            user.PassSalt = Authentication.Instance.getCurrentUser().PassSalt;
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    Authentication.Instance.UserLogin(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("~/Home/Index");
            }
            return View(user);
        }

        // GET: User/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.UserId == id);
        }
    }
}
