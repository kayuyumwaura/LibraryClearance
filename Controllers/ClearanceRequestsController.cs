  using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using LibraryClearance.Data;
    using LibraryClearance.Models;
    using LibraryClearance.Services;

    namespace LibraryClearance.Controllers
    {
        [Authorize]
        public class ClearanceRequestsController : Controller
        {
            private readonly ApplicationDbContext _context;
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly IEmailService _emailService;

            public ClearanceRequestsController(
                ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                IEmailService emailService)
            {
                _context = context;
                _userManager = userManager;
                _emailService = emailService;
            }

            // GET: ClearanceRequests
            public async Task<IActionResult> Index()
            {
                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                IQueryable<ClearanceRequest> requests;

                if (isAdmin)
                {
                    requests = _context.ClearanceRequests.Include(r => r.User);
                }
                else
                {
                    requests = _context.ClearanceRequests.Where(r => r.UserId == user.Id);
                }


            var requestList = await requests.ToListAsync();

            // Debug: Check what's actually in the database
            foreach (var request in requestList)
            {
                System.Diagnostics.Debug.WriteLine($"Request ID: {request.Id}, SubmittedDate: {request.SubmittedDate}");
            }

            // Try ordering without calling OrderByDescending first to see if that's the issue
            var orderedRequests = requestList.OrderByDescending(r => r.SubmittedDate).ToList();

            return View(await requests.OrderByDescending(r => r.SubmittedDate).ToListAsync());
            }

        public async Task<IActionResult> Index1()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // First, let's get the raw data without any ordering
            List<ClearanceRequest> requests;

            if (isAdmin)
            {
                requests = await _context.ClearanceRequests
                    .Include(r => r.User)
                    .ToListAsync();
            }
            else
            {
                requests = await _context.ClearanceRequests
                    .Where(r => r.UserId == user.Id)
                    .ToListAsync();
            }

            // Debug: Output each request's details
            foreach (var request in requests)
            {
                var dateValue = request.SubmittedDate;
                var dateString = dateValue == default(DateTime) ? "DEFAULT/NULL" : dateValue.ToString("yyyy-MM-dd HH:mm:ss");

                // This will help you see what's actually happening
                ViewData["Debug"] += $"ID: {request.Id}, Title: {request.Title}, Date: {dateString}<br/>";
            }

            // Try different approaches to ordering
            try
            {
                // Approach 1: Order by date, but handle default dates
                var orderedRequests = requests
                    .Where(r => r.SubmittedDate != default(DateTime))
                    .OrderByDescending(r => r.SubmittedDate)
                    .Concat(requests.Where(r => r.SubmittedDate == default(DateTime)))
                    .ToList();

                return View(orderedRequests);
            }
            catch (Exception ex)
            {
                // If ordering fails, return unordered list
                ViewData["Error"] = $"Ordering failed: {ex.Message}";
                return View(requests);
            }
        }

        // GET: ClearanceRequests/Details/5
        public async Task<IActionResult> Details(int? id)
            {
                if (id == null) return NotFound();

                var clearanceRequest = await _context.ClearanceRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (clearanceRequest == null) return NotFound();

                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin && clearanceRequest.UserId != user.Id)
                {
                    return Forbid();
                }

                return View(clearanceRequest);
            }

            // GET: ClearanceRequests/Create
            public IActionResult Create()
            {
                return View();
            }

            // POST: ClearanceRequests/Create
            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(ClearanceRequest clearanceRequest)
            {
                try
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user == null)
                    {
                        ModelState.AddModelError("", "User not found. Please log in again.");
                        return View(clearanceRequest);
                    }

                    clearanceRequest.UserId = user.Id;
                    clearanceRequest.Status = RequestStatus.Pending;
                    clearanceRequest.AdminComments = "none";
                    clearanceRequest.SubmittedDate = DateTime.Now;

                    _context.Add(clearanceRequest);
                    await _context.SaveChangesAsync(); // Save first to generate the ID

                    // Now the ID is available for email notifications
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    if (admins.Any())
                    {
                        foreach (var admin in admins)
                        {
                            if (!string.IsNullOrEmpty(admin.Email))
                            {
                                try
                                {
                                    await _emailService.SendNewRequestNotificationAsync(
                                        admin.Email,
                                        clearanceRequest.Title,
                                        clearanceRequest.Id);
                                }
                                catch (Exception)
                                {
                                    // Email failure - continue processing other admins
                                    // In production, you might want to log this somewhere
                                }
                            }
                        }
                    }

                    TempData["Success"] = "Your copyright clearance request has been submitted successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "An error occurred while submitting your request. Please try again.");
                    return View(clearanceRequest);
                }
            return View(clearanceRequest);
        }

            // GET: ClearanceRequests/Review/5
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> Review(int? id)
            {
                if (id == null) return NotFound();

                var clearanceRequest = await _context.ClearanceRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (clearanceRequest == null) return NotFound();

                return View(clearanceRequest);
            }

            // POST: ClearanceRequests/Review/5
            [HttpPost]
            [ValidateAntiForgeryToken]
            [Authorize(Roles = "Admin")]
            public async Task<IActionResult> Review(int id, RequestStatus status, string adminComments)
            {
                var clearanceRequest = await _context.ClearanceRequests
                    .Include(r => r.User)
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (clearanceRequest == null) return NotFound();

                clearanceRequest.Status = status;
                clearanceRequest.AdminComments = adminComments;
                clearanceRequest.LastUpdated = DateTime.Now;

                _context.Update(clearanceRequest);
                await _context.SaveChangesAsync();

                // Send notification to user
                await _emailService.SendStatusUpdateNotificationAsync(
                    clearanceRequest.User.Email,
                    clearanceRequest.Title,
                    status.ToString(),
                    adminComments);

                TempData["Success"] = $"Request has been {status.ToString().ToLower()} successfully.";
                return RedirectToAction(nameof(Index));
            }
        }
    }


