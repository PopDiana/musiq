using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using musiq.Models;
using musiq;
using musiq.Data;
using Microsoft.EntityFrameworkCore;

namespace musiq.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string searchString, string genre) { 
        
            if(!Authentication.Instance.isLoggedIn())
            {
                return Redirect("~/User/Login");
            }
            var  posts = _context.Post.Include(p => p.User);
            if(genre != null)
            {
                posts = posts.Where(p => p.Genre == genre).Include(p => p.User);
            }
            if(searchString != null)
            {
                posts = posts.Where(p => p.Media.Contains(searchString) || p.User.Nickname.Contains(searchString) ||
                p.Lyrics.Contains(searchString)).Include(p => p.User);
            }
            return View(await posts.ToListAsync());
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
