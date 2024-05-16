﻿using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Controllers;

public class BlogsController : Controller
{
    private readonly IdentityDBContext _context;

    public BlogsController(IdentityDBContext context)
    {
        _context = context;
    }

    // GET: Blogs

    ///both pagination and filter as per question
    public async Task<IActionResult> Index(string filterOption, int? page)
    {
        var pageSize = 5; // Number of items per page
        var pageNumber = page ?? 1; // Use the provided page number or default to 1

        var myDbContext = _context.Blogs.Include(b => b.User);
        var blogs = await myDbContext.ToListAsync();
        var reactionCounts = await GetReactionCounts();
        var commentCounts = await GetCommentCounts(); // New line to get comment counts

        ViewData["ReactionCounts"] = reactionCounts;
        ViewData["CommentReactionCounts"] = await GetCommentReactionCounts();
        ViewData["CommentCounts"] = commentCounts; // Pass comment counts to the view

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userReactions = await _context.Reactions
            .Where(r => r.UserID == userId)
            .ToListAsync();

        ViewBag.BlogReactions = userReactions;

        switch (filterOption)
        {
            case "popular":
                blogs = blogs.OrderByDescending(b => CalculatePopularity(b, reactionCounts, commentCounts)).ToList();
                break;
            case "random":
                blogs = blogs.OrderBy(b => Guid.NewGuid()).ToList();
                break;
            case "recency":
                blogs = blogs.OrderByDescending(b => b.CreatedDate).ToList();
                break;
            default:
                // Default to recency if no valid filter option is provided
                blogs = blogs.OrderByDescending(b => b.CreatedDate).ToList();
                break;
        }

        ViewBag.FilterOption = filterOption;

        // Calculate total number of pages
        var pageCount = (int)Math.Ceiling(blogs.Count / (double)pageSize);

        // Ensure the page number is within the valid range
        pageNumber = Math.Max(1, Math.Min(pageNumber, pageCount));

        // Skip items based on the current page and take 'pageSize' items
        var paginatedBlogs = blogs.Skip((pageNumber - 1) * pageSize).Take(pageSize);

        // Set ViewBag values for pagination
        ViewBag.PageCount = pageCount;
        ViewBag.PageNumber = pageNumber;

        return View(paginatedBlogs);
    }

    // Method to calculate the popularity score for a blog post
    private int CalculatePopularity(Blog blog, Dictionary<int, (int upvotes, int downvotes)> reactionCounts,
        Dictionary<int, int> commentCounts)
    {
        if (reactionCounts == null || !reactionCounts.ContainsKey(blog.BlogID) ||
            !commentCounts.ContainsKey(blog.BlogID))
            // If reactionCounts or commentCounts is null or doesn't contain the blog ID, return a default popularity score
            return 0;
        var upvoteWeightage = 2;
        var downvoteWeightage = -1;
        var commentWeightage = 1;

        var upvotes = reactionCounts[blog.BlogID].upvotes;
        var downvotes = reactionCounts[blog.BlogID].downvotes;
        var comments = commentCounts[blog.BlogID]; // Retrieve comment count

        var popularity = upvoteWeightage * upvotes + downvoteWeightage * downvotes + commentWeightage * comments;

        return popularity;
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var blog = await _context.Blogs
            .Include(b => b.User)
            .FirstOrDefaultAsync(m => m.BlogID == id);
        if (blog == null) return NotFound();

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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("BlogID,Title,Body,UserID")] Blog blog,
        IFormFile ProfilePictureUpload)
    {
        blog.CreatedDate = DateTime.Now;
        blog.UserID = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(blog.UserID))
        {
            // Show alert message
            var script = "<script>alert('You need to be logged in to perform this action.');";
            // Redirect to Blogs page after the alert
            script += "window.history.back();</script>";
            return Content(script, "text/html");
        }

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
        if (id == null) return NotFound();

        var blog = await _context.Blogs.FindAsync(id);
        if (blog == null) return NotFound();
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", blog.UserID);
        return View(blog);
    }


    ////works
    [HttpPost]
    public async Task<IActionResult> Edit(int id, string title, string body, IFormFile blogPictureUpload)
    {
        // Retrieve the existing blog post from the database
        var blog = await _context.Blogs.FindAsync(id);

        if (blog == null) return NotFound();

        // Update the properties of the blog post
        blog.Title = title;
        blog.Body = body;

        if (blogPictureUpload != null && blogPictureUpload.Length > 0)
            // Read the file contents and store them in the BlogPicture property
            using (var memoryStream = new MemoryStream())
            {
                await blogPictureUpload.CopyToAsync(memoryStream);
                blog.BlogPicture = memoryStream.ToArray();
            }

        _context.Blogs.Update(blog);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    // GET: Blogs/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var blog = await _context.Blogs
            .Include(b => b.User)
            .FirstOrDefaultAsync(m => m.BlogID == id);
        if (blog == null) return NotFound();

        return View(blog);
    }

    // POST: Blogs/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var blog = await _context.Blogs.FindAsync(id);
        if (blog != null) _context.Blogs.Remove(blog);

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

        var upvoteReactionTypeId =
            _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Up Vote")?.ReactionTypeID;
        var downvoteReactionTypeId =
            _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Down Vote")?.ReactionTypeID;

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
                reactionCounts[blogId] = (upvotes.GetValueOrDefault(blogId, 0), downvotes.GetValueOrDefault(blogId, 0));
        }

        return reactionCounts;
    }

    private async Task<Dictionary<int, (int upvotes, int downvotes)>> GetCommentReactionCounts()
    {
        var reactionCounts = new Dictionary<int, (int upvotes, int downvotes)>();

        var upvoteReactionTypeId =
            _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Up Vote")?.ReactionTypeID;
        var downvoteReactionTypeId =
            _context.ReactionTypes.FirstOrDefault(rt => rt.ReactionName == "Down Vote")?.ReactionTypeID;

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
                reactionCounts[commentId] = (upvotes.GetValueOrDefault(commentId, 0),
                    downvotes.GetValueOrDefault(commentId, 0));
        }

        return reactionCounts;
    }

    // for comment count
    private async Task<Dictionary<int, int>> GetCommentCounts()
    {
        var commentCounts = await _context.Comments
            .GroupBy(c => c.BlogID)
            .ToDictionaryAsync(g => g.Key, g => g.Count());

        return commentCounts;
    }
}