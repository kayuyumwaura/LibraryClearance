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

                return View(await requests.OrderByDescending(r => r.SubmittedDate).ToListAsync());
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
                if (ModelState.IsValid)
                {
                    var user = await _userManager.GetUserAsync(User);
                    clearanceRequest.UserId = user.Id;
                    clearanceRequest.Status = RequestStatus.Pending;
                    clearanceRequest.SubmittedDate = DateTime.Now;

                    _context.Add(clearanceRequest);
                    await _context.SaveChangesAsync();

                    // Send notification to admin
                    var admins = await _userManager.GetUsersInRoleAsync("Admin");
                    foreach (var admin in admins)
                    {
                        await _emailService.SendNewRequestNotificationAsync(
                            admin.Email,
                            clearanceRequest.Title,
                            clearanceRequest.Id);
                    }

                    TempData["Success"] = "Your copyright clearance request has been submitted successfully.";
                    return RedirectToAction(nameof(Index));
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


