﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Controllers;

//[Authorize(Roles = "Blogger")]
public class ReactionsController : Controller
{
    private readonly IdentityDBContext _context;

    public ReactionsController(IdentityDBContext context)
    {
        _context = context;
    }

    // GET: Reactions
    public async Task<IActionResult> Index()
    {
        var identityDBContext =
            _context.Reactions.Include(r => r.Blog).Include(r => r.ReactionType).Include(r => r.User);
        return View(await identityDBContext.ToListAsync());
    }

    // GET: Reactions/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var reaction = await _context.Reactions
            .Include(r => r.Blog)
            .Include(r => r.ReactionType)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.ReactionID == id);
        if (reaction == null) return NotFound();

        return View(reaction);
    }

    // GET: Reactions/Create
    public IActionResult Create()
    {
        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body");
        ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName");
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
        return View();
    }

    // POST: Reactions/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ReactionID,BlogID,UserID,ReactionTypeID")] Reaction reaction)
    {
        if (ModelState.IsValid)
        {
            _context.Add(reaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", reaction.BlogID);
        ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName",
            reaction.ReactionTypeID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", reaction.UserID);
        return View(reaction);
    }

    // GET: Reactions/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var reaction = await _context.Reactions.FindAsync(id);
        if (reaction == null) return NotFound();
        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", reaction.BlogID);
        ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName",
            reaction.ReactionTypeID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", reaction.UserID);
        return View(reaction);
    }

    // POST: Reactions/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, [Bind("ReactionID,BlogID,UserID,ReactionTypeID")] Reaction reaction)
    {
        if (id != reaction.ReactionID) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(reaction);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReactionExists(reaction.ReactionID))
                    return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        ViewData["BlogID"] = new SelectList(_context.Blogs, "BlogID", "Body", reaction.BlogID);
        ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName",
            reaction.ReactionTypeID);
        ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", reaction.UserID);
        return View(reaction);
    }

    // GET: Reactions/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var reaction = await _context.Reactions
            .Include(r => r.Blog)
            .Include(r => r.ReactionType)
            .Include(r => r.User)
            .FirstOrDefaultAsync(m => m.ReactionID == id);
        if (reaction == null) return NotFound();

        return View(reaction);
    }

    // POST: Reactions/Delete/5
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var reaction = await _context.Reactions.FindAsync(id);
        if (reaction != null) _context.Reactions.Remove(reaction);

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ReactionExists(int? id)
    {
        return _context.Reactions.Any(e => e.ReactionID == id);
    }

    /// My own Create method for Upvotes
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVoteReaction([Bind("BlogID,UserID,ReactionTypeID")] Reaction newReaction)
    {
        if (string.IsNullOrEmpty(newReaction.UserID))
        {
            // Show alert message
            var script = "<script>alert('You need to be logged in to perform this action.');";
            // Redirect to Blogs page after the alert
            //script += "window.location.href = '/Blogs';</script>";
            script += "window.history.back();</script>";
            return Content(script, "text/html");
        }


        // Get the existing reaction for the current user, blog, and reaction type
        var existingReaction = await _context.Reactions
            .FirstOrDefaultAsync(r =>
                r.BlogID == newReaction.BlogID && r.UserID == newReaction.UserID &&
                r.ReactionTypeID == newReaction.ReactionTypeID);

        if (existingReaction == null)
        {
            // Remove any existing reactions for the current user and blog
            var existingReactions = await _context.Reactions
                .Where(r => r.BlogID == newReaction.BlogID && r.UserID == newReaction.UserID)
                .ToListAsync();
            _context.Reactions.RemoveRange(existingReactions);

            // Add the new reaction
            _context.Reactions.Add(newReaction);
            var blog = await _context.Blogs.FindAsync(newReaction.BlogID);
            if (blog != null)
            {
                var user = await _context.Users.FindAsync(newReaction.UserID);
                var userName = user?.UserName ?? "Unknown";
                // Register a notification for the blog owner
                newReaction.CreateReactionNotification(_context, blog.UserID, userName);
            }
        }
        else
        {
            // Remove the existing reaction
            _context.Reactions.Remove(existingReaction);
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index), "Blogs");
    }
}