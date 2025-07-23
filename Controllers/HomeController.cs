
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Identity;
    using LibraryClearance.Models;

    namespace LibraryClearance.Controllers
    {
        public class HomeController : Controller
        {
            private readonly UserManager<ApplicationUser> _userManager;

            public HomeController(UserManager<ApplicationUser> userManager)
            {
                _userManager = userManager;
            }

            public async Task<IActionResult> Index()
            {
                if (User.Identity.IsAuthenticated)
                {
                    var user = await _userManager.GetUserAsync(User);
                    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                    ViewBag.IsAdmin = isAdmin;
                }

                return View();
            }
        }
    }

