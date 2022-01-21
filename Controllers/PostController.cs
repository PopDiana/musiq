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
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _appEnvironment;
        public PostController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _appEnvironment = hostingEnvironment;
            _context = context;
        }

        // GET: Post/Create
        public IActionResult Create()
        {
            if (!Authentication.Instance.isLoggedIn())
            {
                return Redirect("~/Home/Index");
            }
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email");
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,UserId,Genre,Created,Description,Lyrics,YoutubeLink,Media,File")] Post post)
        {
            if(post.File == null && post.YoutubeLink==null)
            {
                ModelState.AddModelError("", "Please upload some music.");
            }
            if (post.File != null && post.YoutubeLink != null)
            {
                ModelState.AddModelError("", "Cannot add both types of media.");
            }
       
            var file = post.File;

            if (file != null && file.Length > 0)
            {             
                var uploads = Path.Combine(_appEnvironment.WebRootPath, "uploads");
                var extension = Path.GetExtension(file.FileName);
                if (extension != ".mp4")
                {
                    ModelState.AddModelError("", "Wrong type of media.");
                }
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + extension;
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        post.Media = fileName;
                    }
                }
            }
            if (ModelState.IsValid)
            {
                post.Created = DateTime.Now;
                post.UserId = Authentication.Instance.getCurrentUser().UserId;
                _context.Add(post);
                await _context.SaveChangesAsync();
                return Redirect("~/Home/Index");
            }
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email", post.UserId);
            return View(post);
        }

        // GET: Post/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Post.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!Authentication.Instance.isLoggedIn())
            {
                return Redirect("~/Home/Index");
            }
            if(post.UserId != Authentication.Instance.getCurrentUser().UserId && !Authentication.Instance.isAdmin())
            {
                return Redirect("~/Home/Index");
            }
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email", post.UserId);
            return View(post);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,UserId,Genre,Created,Description,Lyrics,YoutubeLink,Media,File")] Post post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }
            if (post.File == null && post.YoutubeLink == null)
            {
                ModelState.AddModelError("", "Please upload some music.");
            }
            if (post.File != null && post.YoutubeLink != null)
            {
                ModelState.AddModelError("", "Cannot add both types of media.");
            }
            var file = post.File;

            if (file != null && file.Length > 0)
            {
                var uploads = Path.Combine(_appEnvironment.WebRootPath, "uploads");
                var extension = Path.GetExtension(file.FileName);
                if (extension != ".mp4")
                {
                    ModelState.AddModelError("", "Wrong type of media.");
                }
                if (file.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString().Replace("-", "") + extension;
                    using (var fileStream = new FileStream(Path.Combine(uploads, fileName), FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                        post.Media = fileName;
                    }
                }
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(post);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
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
            ViewData["UserId"] = new SelectList(_context.User, "UserId", "Email", post.UserId);
            return View(post);
        }

        private bool PostExists(int id)
        {
            return _context.Post.Any(e => e.PostId == id);
        }
    }
}
