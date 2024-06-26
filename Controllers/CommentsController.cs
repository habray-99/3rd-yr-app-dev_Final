﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Controllers;

public class CommentsController : Controller
{
    private readonly IdentityDBContext _context;

    public CommentsController(IdentityDBContext context)
    {
        _context = context;
    }

    // GET: Comments
    public async Task<IActionResult> Index()
    {
        var identityDBContext = _context.Comments.Include(c => c.Blog).Include(c => c.User);
        return View(await identityDBContext.ToListAsync());
    }

    // GET: Comments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var comment = await _context.Comments
            .Include(c => c.Blog)
            .Include(c => c.User)
            .FirstOrDefaultAsync(m => m.CommentID == id);
        if (comment == null) return NotFound();

        return View(comment);
    }

    // GET: Comments/Create
    public IActionResult Create()
    {
        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body");
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
        return View();
    }

    // POST: Comments/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CommentID,BlogID,UserID,CommentText,CreatedDate")] Comment comment)
    {
        if (ModelState.IsValid)
        {
            _context.Add(comment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", comment.BlogID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", comment.UserID);
        return View(comment);
    }

    // GET: Comments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var comment = await _context.Comments.FindAsync(id);
        if (comment == null) return NotFound();
        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", comment.BlogID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", comment.UserID);
        return View(comment);
    }

    // POST: Comments/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("CommentID,BlogID,UserID,CommentText,CreatedDate")] Comment comment)
    {
        comment.CreatedDate = DateTime.Now;
        if (id != comment.CommentID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(comment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CommentExists(comment.CommentID))
                    return NotFound();
                throw;
            }

            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Details", "Blogs", new { id = comment.BlogID });
        }

        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", comment.BlogID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", comment.UserID);
        return View(comment);
    }

    // GET: Comments/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var comment = await _context.Comments
            .Include(c => c.Blog)
            .Include(c => c.User)
            .FirstOrDefaultAsync(m => m.CommentID == id);
        if (comment == null) return NotFound();

        return View(comment);
    }

    // POST: Comments/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment != null) _context.Comments.Remove(comment);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool CommentExists(int id)
    {
        return _context.Comments.Any(e => e.CommentID == id);
    }

    /// Custom my own for blog
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateComment(Comment comment)
    {
        if (string.IsNullOrEmpty(comment.UserID))
        {
            // Show alert message
            var script = "<script>alert('You need to be logged in to perform this action.');";
            // Redirect to Blogs page after the alert
            script += "window.history.back();</script>";
            return Content(script, "text/html");
        }

        if (ModelState.IsValid)
        {
            comment.CreatedDate = DateTime.Now; // Set the created date
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            var blog = await _context.Blogs.FindAsync(comment.BlogID);
            if (blog != null)
            {
                var user = await _context.Users.FindAsync(comment.UserID);
                var userName = user?.UserName ?? "Unknown";
                // Invoke the CreateCommentNotification method
                comment.CreateCommentNotification(_context, blog.UserID, userName);
            }

            return RedirectToAction("Details", "Blogs", new { id = comment.BlogID });
        }

        // Handle the case where the model state is invalid
        return View(comment);
    }
}