using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Controllers
{
    public class CommentReactionsController : Controller
    {
        private readonly IdentityDBContext _context;

        public CommentReactionsController(IdentityDBContext context)
        {
            _context = context;
        }

        // GET: CommentReactions
        public async Task<IActionResult> Index()
        {
            var identityDBContext = _context.CommentReactions.Include(c => c.Comment).Include(c => c.ReactionType).Include(c => c.User);
            return View(await identityDBContext.ToListAsync());
        }

        // GET: CommentReactions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentReaction = await _context.CommentReactions
                .Include(c => c.Comment)
                .Include(c => c.ReactionType)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CommentReactionID == id);
            if (commentReaction == null)
            {
                return NotFound();
            }

            return View(commentReaction);
        }

        // GET: CommentReactions/Create
        public IActionResult Create()
        {
            ViewData["CommentID"] = new SelectList(_context.Comments, "CommentID", "CommentText");
            ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName");
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: CommentReactions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CommentReactionID,CommentID,UserID,ReactionTypeID")] CommentReaction commentReaction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(commentReaction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CommentID"] = new SelectList(_context.Comments, "CommentID", "CommentText", commentReaction.CommentID);
            ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName", commentReaction.ReactionTypeID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", commentReaction.UserID);
            return View(commentReaction);
        }

        // GET: CommentReactions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentReaction = await _context.CommentReactions.FindAsync(id);
            if (commentReaction == null)
            {
                return NotFound();
            }
            ViewData["CommentID"] = new SelectList(_context.Comments, "CommentID", "CommentText", commentReaction.CommentID);
            ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName", commentReaction.ReactionTypeID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", commentReaction.UserID);
            return View(commentReaction);
        }

        // POST: CommentReactions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? id, [Bind("CommentReactionID,CommentID,UserID,ReactionTypeID")] CommentReaction commentReaction)
        {
            if (id != commentReaction.CommentReactionID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(commentReaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentReactionExists(commentReaction.CommentReactionID))
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
            ViewData["CommentID"] = new SelectList(_context.Comments, "CommentID", "CommentText", commentReaction.CommentID);
            ViewData["ReactionTypeID"] = new SelectList(_context.ReactionTypes, "ReactionTypeID", "ReactionName", commentReaction.ReactionTypeID);
            ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", commentReaction.UserID);
            return View(commentReaction);
        }

        // GET: CommentReactions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var commentReaction = await _context.CommentReactions
                .Include(c => c.Comment)
                .Include(c => c.ReactionType)
                .Include(c => c.User)
                .FirstOrDefaultAsync(m => m.CommentReactionID == id);
            if (commentReaction == null)
            {
                return NotFound();
            }

            return View(commentReaction);
        }

        // POST: CommentReactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var commentReaction = await _context.CommentReactions.FindAsync(id);
            if (commentReaction != null)
            {
                _context.CommentReactions.Remove(commentReaction);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentReactionExists(int? id)
        {
            return _context.CommentReactions.Any(e => e.CommentReactionID == id);
        }

        //my custom create comment reaction
        public async Task<IActionResult> CreateCommentReaction([Bind("CommentID,UserID,ReactionTypeID")] CommentReaction newReaction)
        {
            if (string.IsNullOrEmpty(newReaction.UserID))
            {
                // Redirect the user to the login page
                string script = "<script>alert('You need to be logged in to perform this action.');</script>";
                return Content(script, "text/html");
            }

            // Get the existing reaction for the current user, comment, and reaction type
            var existingReaction = await _context.CommentReactions
                .FirstOrDefaultAsync(r => r.CommentID == newReaction.CommentID && r.UserID == newReaction.UserID && r.ReactionTypeID == newReaction.ReactionTypeID);

            if (existingReaction == null)
            {
                // Remove any existing reactions for the current user and comment
                var existingReactions = await _context.CommentReactions
                    .Where(r => r.CommentID == newReaction.CommentID && r.UserID == newReaction.UserID)
                    .ToListAsync();
                _context.CommentReactions.RemoveRange(existingReactions);

                // Add the new reaction
                _context.CommentReactions.Add(newReaction);

                // Inside the RegisterCommentReactionNotification method
                var commentFromDB = await _context.Comments.FindAsync(newReaction.CommentID);
                if (commentFromDB != null)
                {
                    // Get the username by userId
                    var user = await _context.Users.FindAsync(newReaction.UserID);
                    string userName = user?.UserName ?? "Unknown";
                    // Register a notification for the comment author
                    RegisterCommentReactionNotification(commentFromDB.UserID, newReaction.ReactionTypeID, userName);
                }
            }
            else
            {
                // Remove the existing reaction
                _context.CommentReactions.Remove(existingReaction);
            }

            await _context.SaveChangesAsync();

            // Redirect back to the details page for the comment's blog
            var comment = await _context.Comments.FindAsync(newReaction.CommentID);
            return RedirectToAction("Details", "Blogs", new { id = comment.BlogID });
        }
        private void RegisterCommentReactionNotification(string commentAuthorUserId, int? reactionTypeId, string byWhome)
        {
            // Get the reaction type name
            var reactionType = _context.ReactionTypes.Find(reactionTypeId);
            var reactionName = reactionType?.ReactionName;

            // Create a new notification
            var notification = new Notification
            {
                UserID = commentAuthorUserId,
                NotificationType = $"New reaction ({reactionName}) on your comment by: {byWhome}",
                // You can set any additional properties for the Notification entity here
            };

            // Add the notification to the database context
            _context.Notifications.Add(notification);
        }
    }
}
