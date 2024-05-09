﻿// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Rendering;
// using Microsoft.EntityFrameworkCore;
// using WebApplication6.Areas.Identity.Data;
// using WebApplication6.Models;

// namespace WebApplication6.Controllers
// {
//     public class UsersController : Controller
//     {
//         private readonly IdentityDBContext _context;

//         public UsersController(IdentityDBContext context)
//         {
//             _context = context;
//         }

//         // GET: Users
//         public async Task<IActionResult> Index()
//         {
//             var identityDBContext = _context.User.Include(u => u.CustomUser);
//             return View(await identityDBContext.ToListAsync());
//         }

//         // GET: Users/Details/5
//         public async Task<IActionResult> Details(string id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var user = await _context.User
//                 .Include(u => u.CustomUser)
//                 .FirstOrDefaultAsync(m => m.UserID == id);
//             if (user == null)
//             {
//                 return NotFound();
//             }

//             return View(user);
//         }

//         // GET: Users/Create
//         public IActionResult Create()
//         {
//             ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id");
//             return View();
//         }

//         // POST: Users/Create
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Create([Bind("UserID,Username,Email,Password,Role")] User user)
//         {
//             if (ModelState.IsValid)
//             {
//                 _context.Add(user);
//                 await _context.SaveChangesAsync();
//                 return RedirectToAction(nameof(Index));
//             }
//             ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", user.UserID);
//             return View(user);
//         }

//         // GET: Users/Edit/5
//         public async Task<IActionResult> Edit(string id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var user = await _context.User.FindAsync(id);
//             if (user == null)
//             {
//                 return NotFound();
//             }
//             ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", user.UserID);
//             return View(user);
//         }

//         // POST: Users/Edit/5
//         // To protect from overposting attacks, enable the specific properties you want to bind to.
//         // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
//         [HttpPost]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> Edit(string id, [Bind("UserID,Username,Email,Password,Role")] User user)
//         {
//             if (id != user.UserID)
//             {
//                 return NotFound();
//             }

//             if (ModelState.IsValid)
//             {
//                 try
//                 {
//                     _context.Update(user);
//                     await _context.SaveChangesAsync();
//                 }
//                 catch (DbUpdateConcurrencyException)
//                 {
//                     if (!UserExists(user.UserID))
//                     {
//                         return NotFound();
//                     }
//                     else
//                     {
//                         throw;
//                     }
//                 }
//                 return RedirectToAction(nameof(Index));
//             }
//             ViewData["UserID"] = new SelectList(_context.Users, "Id", "Id", user.UserID);
//             return View(user);
//         }

//         // GET: Users/Delete/5
//         public async Task<IActionResult> Delete(string id)
//         {
//             if (id == null)
//             {
//                 return NotFound();
//             }

//             var user = await _context.User
//                 .Include(u => u.CustomUser)
//                 .FirstOrDefaultAsync(m => m.UserID == id);
//             if (user == null)
//             {
//                 return NotFound();
//             }

//             return View(user);
//         }

//         // POST: Users/Delete/5
//         [HttpPost, ActionName("Delete")]
//         [ValidateAntiForgeryToken]
//         public async Task<IActionResult> DeleteConfirmed(string id)
//         {
//             var user = await _context.User.FindAsync(id);
//             if (user != null)
//             {
//                 _context.User.Remove(user);
//             }

//             await _context.SaveChangesAsync();
//             return RedirectToAction(nameof(Index));
//         }

//         private bool UserExists(string id)
//         {
//             return _context.User.Any(e => e.UserID == id);
//         }
//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly IdentityDBContext _context;

    public UsersController(IdentityDBContext context)
    {
        _context = context;
    }

    // GET: Users
    //public async Task<IActionResult> Index()
    //{
    //    var users = await _context.Users.ToListAsync();
    //    return View(users);
    //}
    public async Task<IActionResult> Index(string searchQuery)
    {
        var users = from u in _context.Users
                    select u;

        if (!String.IsNullOrEmpty(searchQuery))
        {
            users = users.Where(u => u.UserName.Contains(searchQuery) ||
                                     u.Email.Contains(searchQuery) ||
                                     u.PhoneNumber.Contains(searchQuery) ||
                                     u.Address.Contains(searchQuery));
        }

        var results = await users.ToListAsync();

        return View(results);
    }


    // GET: Users/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
           .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount,Role,Address,ProfilePicture,Blogs,Comments,Reactions,Notifications,CommentReactions,BlogMetrics,UserMetrics")] CustomUser user)
    {
        if (ModelState.IsValid)
        {
            var passwordHasher = new PasswordHasher<CustomUser>();
            user.PasswordHash = passwordHasher.HashPassword(user, user.PasswordHash);
            _context.Add(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Users/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Id,UserName,NormalizedUserName,Email,NormalizedEmail,EmailConfirmed,PasswordHash,SecurityStamp,ConcurrencyStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEnd,LockoutEnabled,AccessFailedCount,Role,Address,ProfilePicture,Blogs,Comments,Reactions,Notifications,CommentReactions,BlogMetrics,UserMetrics")] CustomUser user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.Id))
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
        return View(user);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
           .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}

