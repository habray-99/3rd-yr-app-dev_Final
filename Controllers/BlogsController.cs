using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class BlogsController : Controller
    {
        private readonly IdentityDBContext _context;

        public BlogsController(IdentityDBContext context)
        {
            _context = context;
        }

        // GET: Blogs
        public async Task<IActionResult> Index()
        {

            var myDbContext = _context.Blogs.Include(b => b.User);
            var blogs = await myDbContext.ToListAsync();
            var reactionCounts = await GetReactionCounts();

            ViewData["ReactionCounts"] = reactionCounts;

            var commentReactionCounts = await GetCommentReactionCounts();
            ViewData["CommentReactionCounts"] = commentReactionCounts;


            ////
            // Fetch the user's comment reactions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userReactions = await _context.Reactions
                .Where(r => r.UserID == userId)
                .ToListAsync();

            ViewBag.BlogReactions = userReactions;
            ///

            //var myDbContext = _context.Blogs.Include(b => b.User);
            return View(await myDbContext.ToListAsync());
        }

        // GET: Blogs/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    //fetching comments
        //    var comments = await _context.Comments
        //.Include(c => c.User)
        //.Where(c => c.BlogID == id)
        //.ToListAsync();

        //    ViewBag.Comments = comments;

        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var blog = await _context.Blogs
        //        .Include(b => b.User)
        //        .FirstOrDefaultAsync(m => m.BlogID == id);
        //    if (blog == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(blog);
        //}

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BlogID == id);
            if (blog == null)
            {
                return NotFound();
            }

            // Fetching comments
            var comments = await _context.Comments
                .Include(c => c.User)
                .Where(c => c.BlogID == id)
                .ToListAsync();

            ViewBag.Comments = comments;

            // Fetching comment vote counts
            var commentReactionCounts = await GetCommentReactionCounts();
            ViewData["CommentReactionCounts"] = commentReactionCounts;

            // Fetch the user's comment reactions
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userReactions = await _context.CommentReactions
                .Where(r => r.UserID == userId)
                .ToListAsync();

            ViewBag.UserReactions = userReactions;




            return View(blog);
        }


        // GET: Blogs/Create
        public IActionResult Create()
        {
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("BlogID,Title,Body,UserID,ImagePath")] Blog blog)
        //{
        //    blog.CreatedDate = DateTime.Now;
        //    blog.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    if (ModelState.IsValid)
        //    {
        //        // Set the CreatedDate property to the current date
        //        blog.CreatedDate = DateTime.Now;

        //        _context.Add(blog);
        //        await _context.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
        //    return View(blog);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BlogID,Title,Body,UserID")] Blog blog, IFormFile ProfilePictureUpload)
        {
            blog.CreatedDate = DateTime.Now;
            blog.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (ModelState.IsValid)
            {
                if (ProfilePictureUpload != null && ProfilePictureUpload.Length > 0)
                {
                    // Check the file size (in bytes)
                    const int maxFileSize = 3 * 1024 * 1024; // 3 MB
                    if (ProfilePictureUpload.Length > maxFileSize)
                    {
                        ModelState.AddModelError("ProfilePictureUpload", "The profile picture must be 3 MB or smaller.");
                        return View(blog);
                    }

                    using (var memoryStream = new MemoryStream())
                    {
                        await ProfilePictureUpload.CopyToAsync(memoryStream);
                        blog.BlogPicture = memoryStream.ToArray();
                    }
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(blog);
        }


        // GET: Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
            return View(blog);
        }

        // POST: Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]

        //public async Task<IActionResult> Edit(int? id, [Bind("BlogID,Title,Body,CreatedDate,UserID")] Blog blog, IFormFile ProfilePictureUpload)
        //{
        //    if (id != blog.BlogID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(blog);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!BlogExists(blog.BlogID))
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
        //    ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
        //    return View(blog);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("BlogID,Title,Body,CreatedDate,UserID")] Blog blog, IFormFile ProfilePictureUpload)
        {
            if (id != blog.BlogID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (ProfilePictureUpload != null && ProfilePictureUpload.Length > 0)
                    {
                        // Check the file size (in bytes)
                        const int maxFileSize = 3 * 1024 * 1024; // 3 MB
                        if (ProfilePictureUpload.Length > maxFileSize)
                        {
                            ModelState.AddModelError("ProfilePictureUpload", "The profile picture must be 3 MB or smaller.");
                            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
                            return View(blog);
                        }

                        using (var memoryStream = new MemoryStream())
                        {
                            await ProfilePictureUpload.CopyToAsync(memoryStream);
                            blog.BlogPicture = memoryStream.ToArray();
                        }
                    }

                    _context.Update(blog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.BlogID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
            return View(blog);
        }


        // GET: Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.BlogID == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // POST: Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                _context.Blogs.Remove(blog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int? id)
        {
            return _context.Blogs.Any(e => e.BlogID == id);
        }

        ///count
        private async Task<Dictionary<int, (int upvotes, int downvotes)>> GetReactionCounts()
        {
            var reactionCounts = new Dictionary<int, (int upvotes, int downvotes)>();

            var upvoteReactionTypeId = _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Up Vote")?.ReactionTypeID;
            var downvoteReactionTypeId = _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Down Vote")?.ReactionTypeID;

            if (upvoteReactionTypeId != null && downvoteReactionTypeId != null)
            {
                var upvotes = await _context.Reactions
                    .Where(r => r.ReactionTypeID == upvoteReactionTypeId)
                    .GroupBy(r => r.BlogID)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                var downvotes = await _context.Reactions
                    .Where(r => r.ReactionTypeID == downvoteReactionTypeId)
                    .GroupBy(r => r.BlogID)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                foreach (var blogId in upvotes.Keys.Union(downvotes.Keys))
                {
                    reactionCounts[blogId] = (upvotes.GetValueOrDefault(blogId, 0), downvotes.GetValueOrDefault(blogId, 0));
                }
            }

            return reactionCounts;
        }

        private async Task<Dictionary<int, (int upvotes, int downvotes)>> GetCommentReactionCounts()
        {
            var reactionCounts = new Dictionary<int, (int upvotes, int downvotes)>();

            var upvoteReactionTypeId = _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Up Vote")?.ReactionTypeID;
            var downvoteReactionTypeId = _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Down Vote")?.ReactionTypeID;

            if (upvoteReactionTypeId != null && downvoteReactionTypeId != null)
            {
                var upvotes = await _context.CommentReactions
                    .Where(r => r.ReactionTypeID == upvoteReactionTypeId)
                    .GroupBy(r => r.CommentID)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                var downvotes = await _context.CommentReactions
                    .Where(r => r.ReactionTypeID == downvoteReactionTypeId)
                    .GroupBy(r => r.CommentID)
                    .ToDictionaryAsync(g => g.Key, g => g.Count());

                foreach (var commentId in upvotes.Keys.Union(downvotes.Keys))
                {
                    reactionCounts[commentId] = (upvotes.GetValueOrDefault(commentId, 0), downvotes.GetValueOrDefault(commentId, 0));
                }
            }

            return reactionCounts;
        }

    }
}
