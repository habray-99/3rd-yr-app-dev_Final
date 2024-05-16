using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplication6.Areas.Identity.Data;
using WebApplication6.Models;

namespace WebApplication6.Areas.Identity.Pages.Account;

public class ProfilePage_cshtml : PageModel
{
    private readonly IdentityDBContext _context;
    private readonly SignInManager<CustomUser> _signInManager;
    private readonly UserManager<CustomUser> _userManager;

    public ProfilePage_cshtml(
        UserManager<CustomUser> userManager,
        SignInManager<CustomUser> signInManager,
        IdentityDBContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [BindProperty]
    public InputModel? Input { get; set; }

    public ICollection<UserMetric>? UserMetrics { get; set; }

    private async Task LoadAsync(CustomUser user)
    {
        // User = user;
        var userName = await _userManager.GetUserNameAsync(user);
        var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
        var email = await _userManager.GetEmailAsync(user);
        var UserMetrics = await GetMetricsAsync(user);
        Debug.Assert(userName != null, nameof(userName) + " != null");
        Input = new InputModel
        {
            PhoneNumber = phoneNumber,
            Username = userName,
            Address = user.Address,
            Email = email
        };
    }

    // Add a method to fetch UserMetrics for a user
    private async Task<ICollection<UserMetric>> GetMetricsAsync(CustomUser user)
    {
        return await _context.UserMetrics
            .Where(m => m.UserID == user.Id)
            .ToListAsync();
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");

        await LoadAsync(user);
        return Page();
    }

    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required]
        [StringLength(256, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 3)]
        [Display(Name = "Username")]
        public string Username { get; set; }

        [Phone]
        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [MaxLength(30)]
        [Display(Name = "Address")]
        public string Address { get; set; }

        [BindProperty]
        [Display(Name = "Profile Picture")]
        [MaxLength(3 * 1024 * 1024)]
        public IFormFile ProfilePicture { get; set; }

        public CustomUser? User { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}